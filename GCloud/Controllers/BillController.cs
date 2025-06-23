using GCloud.Service;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GCloud.Controllers
{
    [Authorize(Roles = "Managers,Administrators")]
    public class BillController : Controller
    {
        private readonly IBillService _billService;

        public BillController(IBillService billService)
        {
            _billService = billService;
        }

        // GET: Bill
        public ActionResult Index()
        {
            var ebs = _billService.FindAll().Include(b => b.User).OrderByDescending(b => b.ImportedAt);
            return View(ebs.ToList());
        }

        // GET: Bill/Details/5
        public ActionResult Details(Guid id)
        {
            var eb = _billService.FindById(id);
            if (eb == null)
                return HttpNotFound();
            return View(eb);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { }
            base.Dispose(disposing);
        }
    }
}
