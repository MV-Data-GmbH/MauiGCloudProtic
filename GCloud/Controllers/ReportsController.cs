using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using GCloud.Models.Domain;
using GCloud.Reporting;
using GCloud.Repository;
using GCloud.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Reporting.WebForms;

namespace GCloud.Controllers
{
    public class ReportsController : Controller
    {
        private IProcedureRepository _procedureRepository;
        private IUserRepository _userRepository;
        private readonly IReportService _reportService;
        private readonly IStoreService _storeService;

        public IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        public ReportsController(IProcedureRepository procedureRepository, IUserRepository userRepository, IReportService reportService, IStoreService storeService)
        {
            _procedureRepository = procedureRepository;
            _userRepository = userRepository;
            _reportService = reportService;
            _storeService = storeService;
        }
        // GET: Report

        [Authorize(Roles = "Managers")]
        public ActionResult Index()
        {
            var reportGroups = GetReportGroups().ToList();
            var firstReportGroup = reportGroups.FirstOrDefault();
            var firstReportName = firstReportGroup.GroupReports.FirstOrDefault();

            ViewBag.ReportGroups = new SelectList(reportGroups.Select(x => x.GroupName), firstReportGroup);
            ViewBag.ReportNames = new SelectList(firstReportGroup.GroupReports.Select(x => x.ReportName), firstReportName);

            var parameters = new ReportParameterViewModel();
            parameters.Add("@startDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd"));
            parameters.Add("@endDate", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));

            return View(parameters);
        }

        [HttpPost()]
        [Authorize(Roles = "Managers,Administrators")]
        public ActionResult ShowReport([ModelBinder(typeof(ReportParameterViewModelBinder))] ReportParameterViewModel parameterViewModel)
        {
            var currentUser = User.Identity.GetUserId();
            var store = _userRepository.FindById(currentUser).Companies.FirstOrDefault()?.Stores.FirstOrDefault();

            if (store == null)
            {
                return View("_ReportResult");
            }

            //ToDo: Include check if a store is available!

            parameterViewModel.Add("@companyId", store.Company?.Id);

            var testReportDataSourceBuilder = GetDataSourceRepository(parameterViewModel, store);
            if ((testReportDataSourceBuilder == null))
                return View("_ReportResult");

            var reportFolder = Path.Combine(Request.MapPath(Request.ApplicationPath), $@"bin\Reporting\{testReportDataSourceBuilder.GetReportFolderName()}");
            var reportFile = Directory.GetFiles(reportFolder).FirstOrDefault(item => Path.GetFileNameWithoutExtension(item) == testReportDataSourceBuilder.GetReportFileName() && Path.GetExtension(item) == ".rdlc");

            if (!System.IO.File.Exists(reportFile))
                return View("_ReportResult");

            var reportViewer = new GustavReportViewer();

            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.SizeToReportContent = true;
            reportViewer.ZoomMode = ZoomMode.PageWidth;
            reportViewer.Width = Unit.Percentage(100);
            reportViewer.Height = Unit.Percentage(100);
            reportViewer.AsyncRendering = true;
            reportViewer.ShowRefreshButton = false; //may cause problems on subreports
            reportViewer.ShowPrintButton = true;
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Data", testReportDataSourceBuilder.LoadData()));
            reportViewer.LocalReport.ReportPath = reportFile;
            reportViewer.LocalReport.SetParameters(testReportDataSourceBuilder.GetReportParameters());
            reportViewer.DataSourceBuilder = testReportDataSourceBuilder;
            ViewBag.ReportViewer = reportViewer;

            return View("_ReportResult");
        }

        [HttpGet]
        [Authorize]
        public ActionResult MobileReports()
        {
            return View();
        }
        
        public ActionResult CouponUsages(Guid? storeGuid, DateTime? dateFrom, DateTime? dateTo)
        {
            var userId = User.Identity.GetUserId();
            ViewBag.Stores = _storeService.FindBy(x => x.Company.User.Id == userId).ToList();

            var companyId = _storeService.FindByUserId(User.Identity.GetUserId()).FirstOrDefault()?.CompanyId;

            var model = _reportService.GetCouponUsages(storeGuid.GetValueOrDefault(Guid.Empty),
                companyId.GetValueOrDefault(Guid.Empty), dateFrom.GetValueOrDefault(DateTime.Now.AddDays(-7)),
                dateTo.GetValueOrDefault(DateTime.Now));
            return View(model);
        }

        public ActionResult CouponUserUsages(DateTime? datefrom, DateTime? dateTo)
        {
            var companyId = _storeService.FindByUserId(User.Identity.GetUserId()).FirstOrDefault()?.CompanyId;
            var result = _reportService.GetCouponUserUsages(companyId.Value,
                datefrom.GetValueOrDefault(DateTime.Now).AddDays(-7), dateTo.GetValueOrDefault(DateTime.Now));
            return companyId.HasValue ? View(result) : View();
        }

        public ActionResult CouponUsagePerTime(DateTime? datefrom, DateTime? dateTo)
        {
            var companyId = _storeService.FindByUserId(User.Identity.GetUserId()).FirstOrDefault()?.CompanyId;
            var result = _reportService.GetCouponUsagePerTime(companyId.GetValueOrDefault(Guid.Empty),
                datefrom.GetValueOrDefault(DateTime.Now.AddDays(-7)), dateTo.GetValueOrDefault(DateTime.Now));

            return companyId.HasValue ? View(result) : View();
        }

        private IEnumerable<ReportGroupDataModel> GetReportGroups()
        {
            var groupResult = new List<ReportGroupDataModel>();

            var interfaceType = typeof(IDataSourceBuilder);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesLoaded).Where(s => interfaceType.IsAssignableFrom(s) && !s.IsInterface && !s.IsAbstract);
            var reportTypes = types as Type[] ?? types.ToArray();
            foreach (var reportType in reportTypes)
            {
                var groupInstance = (IDataSourceBuilder)Activator.CreateInstance(reportType);
                if (groupInstance.GetReportGroup().GetIsVisible())
                {
                    ReportGroupDataModel reportGroupData = groupResult.FirstOrDefault(item => item.GroupName == groupInstance.GetReportGroup().GetGroupName());
                    if (reportGroupData == null)
                        reportGroupData = new ReportGroupDataModel() { GroupName = groupInstance.GetReportGroup().GetGroupName() };
                    else
                        continue;

                    foreach (var nameType in reportTypes)
                    {
                        var nameInstance = (IDataSourceBuilder)Activator.CreateInstance(nameType);
                        if (nameInstance.IsVisible && nameInstance.GetReportGroup().Equals(groupInstance.GetReportGroup()))
                            reportGroupData.GroupReports.Add(new ReportNameDataModel() { ReportName = nameInstance.GetReportName() });
                    }
                    groupResult.Add(reportGroupData);
                }
            }
            return groupResult;
        }


        [HttpGet]
        [Authorize(Roles = "Managers,Administrators")]
        public ActionResult GetReportNames(string groupName)
        {
            if (!string.IsNullOrWhiteSpace(groupName))
            {
                var reportGroups = GetReportGroups();
                return Json(reportGroups.FirstOrDefault(x => x.GroupName == groupName)?.GroupReports,JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        private IDataSourceBuilder GetDataSourceRepository(ReportParameterViewModel parameterViewModel, Store store)
        {
            var interfaceType = typeof(IDataSourceBuilder);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesLoaded).Where(s => interfaceType.IsAssignableFrom(s) && !s.IsInterface && !s.IsAbstract);
            foreach (var type in types)
            {
                var instance = (IDataSourceBuilder)Activator.CreateInstance(type);
                if (instance.GetReportName() == parameterViewModel.ReportName && instance.GetReportGroup().GetGroupName() == parameterViewModel.ReportGroup)
                    return (IDataSourceBuilder)Activator.CreateInstance(type, _procedureRepository, parameterViewModel, store);
            }
            return null/* TODO Change to default(_) if this is not a reference type */;
        }


        private Type[] GetTypesLoaded(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(item => item != null).ToArray();
            }

            return types;
        }

    }
}