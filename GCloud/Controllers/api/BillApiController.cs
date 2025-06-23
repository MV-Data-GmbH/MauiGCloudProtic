using AutoMapper.QueryableExtensions;
using GCloud.Models.Domain;
using GCloud.Service;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AutoMapper;
using GCloud.Controllers.ViewModels.Bill;
using GCloud.Reporting;
using GCloud.Reporting.EBillReport;
using GCloud.Repository;
using GCloud.Shared.Dto;
using GCloud.Shared.Exceptions.Anonymous;
using GCloud.Shared.Exceptions.Bill;
using GCloud.Shared.Exceptions.Cashier;
using GCloud.Shared.Exceptions.Store;
using GCloud.Shared.Exceptions.User;
using Microsoft.Reporting.WebForms;
using DateTime = System.DateTime;


namespace GCloud.Controllers.api
{
    [System.Web.Http.RoutePrefix("api/BillApi")]
    public class BillApiController : ApiController
    {
        private IProcedureRepository _procedureRepository;
        private readonly IBillService _billService;
        private readonly IMobilePhoneService _mobilePhoneService;
        private readonly IFirebaseNotificationService _firebaseNotificationService;
        private readonly IStoreService _storeService;
        private readonly ICashRegisterRepository _cashRegisterRepository;
        private readonly IUserService _userService;
        private readonly IAnonymousUserRepository _anonymousUserRepository;

        public BillApiController(IProcedureRepository procedureRepository,
            IBillService billService,
            IMobilePhoneService mobilePhoneService,
            IFirebaseNotificationService firebaseNotificationService,
            IStoreService storeService,
            ICashRegisterRepository cashRegisterRepository,
            IUserService userService,
            IAnonymousUserRepository anonymousUserRepository)
        {
            _procedureRepository = procedureRepository;
            _billService = billService;
            _mobilePhoneService = mobilePhoneService;
            _firebaseNotificationService = firebaseNotificationService;
            _storeService = storeService;
            _cashRegisterRepository = cashRegisterRepository;
            _userService = userService;
            _anonymousUserRepository = anonymousUserRepository;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CreateAnonymousUser")]
        public AnonymousUser CreateAnonymousUser()
        {
            var anonymousUser = new AnonymousUser
            {
                CreationDateTime = DateTime.Now
            };
            _anonymousUserRepository.Add(anonymousUser);
            return anonymousUser;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("DeleteAnonymousUser")]
        public void DeleteAnonymousUser(Guid anonymousUserId)
        {
            _billService.DeleteAnonymousUser(anonymousUserId);
        }

        public GetBillsResponseModel GetAnonymous(Guid anonymousUserId, List<Guid> alreadyGot)
        {
            return new GetBillsResponseModel(
                Mapper.Map<List<Bill_Out_Dto>>(_billService.FindAllForAnonymousUser(anonymousUserId, alreadyGot)));
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("Profile")]
        public ProfileViewModel Profile(Guid anonymousUserId)
        {
            var anonymousUser = _anonymousUserRepository.FindById(anonymousUserId);
            if (anonymousUser == null)
            {
                throw new AnonymousUserNotFoundException(anonymousUserId);
            }

            var result = new ProfileViewModel {RegisteredUser = anonymousUser.User != null};
            if (anonymousUser.User != null)
            {
                result.Name = anonymousUser.User.FirstName;
                result.SurName = anonymousUser.User.LastName;
                var bills = _billService.FindAllForUser(anonymousUser.UserId).ToList();
                result.BillCount = bills.Count();
                result.BillSum = bills.Sum(x => x.Invoice.TotalGrossAmount);
                result.StoreCount = bills.Select(x => x.CashRegister.StoreId).Distinct().Count();
            }
            else
            {
                var bills = _billService.FindAllForAnonymousUser(anonymousUser.Id);
                result.BillCount = bills.Count();
                result.BillSum = bills.Sum(x => x.Invoice.TotalGrossAmount);
                result.StoreCount = bills.Select(x => x.CashRegister.StoreId).Distinct().Count();
            }

            return result;
        }

        // to call from end user to get last bills
        [System.Web.Http.Authorize]
        [System.Web.Http.HttpGet, System.Web.Http.HttpPost]
        [System.Web.Http.Route("Get")]
        public GetBillsResponseModel Get()
        {
            // was a method param first, but didn't work with refit plain because of missing headers
            // ToDo: Fix this problem in the iOS app!
            List<Guid> alreadyGot = null;
            try
            {
                var userId = User.Identity.GetUserId();
                if (alreadyGot == null || alreadyGot.Count < 1)
                    return new GetBillsResponseModel(
                        Mapper.Map<List<Bill_Out_Dto>>(_billService.FindAllForUser(userId).ToList()));
                else
                    return new GetBillsResponseModel(
                        Mapper.Map<List<Bill_Out_Dto>>(_billService.FindAllForUser(userId, alreadyGot.ToList())));
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.Message
                    });
            }
        }

        // to call from end user to get last bills
        [System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetById")]
        public Bill_Out_Dto GetById(Guid id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var f = _billService.FindByIdForUser(userId, id);
                return Mapper.Map<Bill_Out_Dto>(f);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.Message
                    });
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetAnonymousById")]
        public Bill_Out_Dto GetAnonymousById(Guid anonymousUserId, Guid billId)
        {
            var bill = _billService.FindByIdForAnonymous(anonymousUserId, billId);
            var billDto = Mapper.Map<Bill_Out_Dto>(bill);
            return billDto;
        }

        // to call from kassa
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Add")]
        public async Task<HttpResponseMessage> Add(BillAddRequestModel model)
        {
            try
            {
                var user = _userService.FindById(model.UserId);
                if (user == null && model.UserId != null)
                {
                    throw new UserNotFoundException(model.UserId);
                }

                var store = _storeService.FindByApiToken(model.StoreApiToken);
                if (store == null)
                {
                    throw new ApiTokenInvalidException(model.StoreApiToken);
                }

                var cashRegister = store.CashRegisters.FirstOrDefault(c => c.Id == model.CashRegisterId);
                if (cashRegister == null)
                {
                    throw new CashRegisterNotInStoreException(model.CashRegisterId);
                }

                if (string.IsNullOrWhiteSpace(model.UserId) && !model.AnonymousUserId.HasValue)
                {
                    throw new NoUserIdProvidedException();
                }

                var response = new HttpResponseMessage(HttpStatusCode.OK);

                foreach (var invoice in model.Invoices)
                {
                    invoice.Biller = new InvoiceBiller
                    {
                        ComanyName = store.Company.Name,
                        VATIdentificationNumber = store.Company.TaxNumber,
                        Address = new InvoiceBillerAddress
                        {
                            Street = $"{store.Street} {store.HouseNr}",
                            ZIP = (ushort) (ushort.TryParse(store.Plz, out var plsValue) ? plsValue : 0),
                            Town = store.City,
                            Name = store.Name,
                            Country = new InvoiceBillerAddressCountry
                            {
                                Value = store.Country.Name
                            }
                        }
                    };
                }

                if (ValidateBill(model) == false)
                    throw new HttpResponseException(
                        new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                        {
                            ReasonPhrase = "Bill not valid"
                        });

                foreach (var invoice in model.Invoices)
                {
                    var bill = new Bill
                    {
                        Amount = invoice.TotalGrossAmount,
                        Company = store.Company.Name,
                        ImportedAt = DateTime.Now,
                        Invoice = invoice,
                        InvoiceNumber = invoice.InvoiceNumber,
                        InvoiceDate = invoice.InvoiceDate,
                        CashRegisterId = cashRegister.Id,
                        StoreApiToken=model.StoreApiToken
                    };
                    List<Bill> currentBillsOfUser = null;
                    if (string.IsNullOrWhiteSpace(model.UserId))
                    {
                        bill.AnonymousUserId = model.AnonymousUserId;
                        if (model.AnonymousUserId.HasValue)
                        {
                            currentBillsOfUser = _billService.FindAllForAnonymousUser(model.AnonymousUserId.Value);
                        }
                    }
                    else
                    {
                        bill.UserId = model.UserId;
                        currentBillsOfUser = _billService.FindAllForUser(model.UserId);
                    }

                    if (currentBillsOfUser == null || currentBillsOfUser.Any(b =>
                            b.InvoiceNumber == bill.InvoiceNumber && b.Company == bill.Company &&
                            b.InvoiceDate.Date == bill.InvoiceDate.Date))
                    {
                        continue;
                    }

                    _billService.Add(bill);

                    var phones = _mobilePhoneService.FindBy(p => p.UserId == model.UserId).ToList();
                    var anonymousMobilePhones = new List<AnonymousMobilePhone>();
                    if (model.AnonymousUserId.HasValue)
                    {
                        var anonymousUser = _anonymousUserRepository.FindById(model.AnonymousUserId.Value);
                        if (anonymousUser != null)
                        {
                            anonymousMobilePhones.AddRange(anonymousUser.AnonymousMobilePhones);
                        }
                    }

                    foreach (var p in phones)
                    {
                        var companyName = bill.Company.Length > 10
                            ? $"{bill.Company.Substring(0, 10)}..."
                            : bill.Company;
                        var n = new FirebaseNotification
                        {
                            Body =
                                $"Rechnung für {companyName} um {bill.Amount.ToString($"0.00 {invoice.InvoiceCurrency}")} vom {bill.Invoice.InvoiceDate:dd.MM.yyyy}",
                            CreatedOn = DateTime.Now,
                            LastAttemptOn = DateTime.Now,
                            DeviceId = p.Id,
                            Title = "Neue Rechnung!",
                            Type = "bill",
                            BillId = bill.Id
                        };
                        _firebaseNotificationService.Add(n);

                        // todo send notification to phone
                        await _firebaseNotificationService.Send(n);
                    }

                    foreach (var p in anonymousMobilePhones)
                    {
                        var companyName = bill.Company.Length > 10
                            ? $"{bill.Company.Substring(0, 10)}..."
                            : bill.Company;
                        var n = new FirebaseNotification
                        {
                            Body =
                                $"Rechnung für {companyName} um {bill.Amount.ToString($"0.00 {invoice.InvoiceCurrency}")} vom {bill.Invoice.InvoiceDate:dd.MM.yyyy}",
                            CreatedOn = DateTime.Now,
                            LastAttemptOn = DateTime.Now,
                            AnonymousMobilePhoneId = p.Id,
                            Title = "Neue Rechnung!",
                            Type = "bill",
                            BillId = bill.Id
                        };
                        _firebaseNotificationService.Add(n);

                        // todo send notification to phone
                        await _firebaseNotificationService.Send(n);
                    }
                }

                return response;
            }
            catch (HttpResponseException he)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.Message
                    });
            }
        }

        // to call from kassa
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("AddV2")]
        public HttpResponseMessage AddV2(BillAddRequestModelV2 model)
        {
            try
            {
                var user = _userService.FindById(model.UserId);
                if (user == null && model.UserId != null)
                {
                    throw new UserNotFoundException(model.UserId);
                }

                var store = _storeService.FindByApiToken(model.StoreApiToken);
                if (store == null)
                {
                    throw new ApiTokenInvalidException(model.StoreApiToken);
                }

                var cashRegister = store.CashRegisters.FirstOrDefault(c => c.Id == model.CashRegisterId);
                if (cashRegister == null)
                {
                    throw new CashRegisterNotInStoreException(model.CashRegisterId);
                }

                //if (string.IsNullOrWhiteSpace(model.UserId) && !model.AnonymousUserId.HasValue)
                //{
                //    throw new NoUserIdProvidedException();
                //}

                var response = new HttpResponseMessage(HttpStatusCode.OK);

                //foreach (var invoice in model.Invoices)
                //{
                //    invoice.Biller = new InvoiceBiller
                //    {
                //        ComanyName = store.Company.Name,
                //        VATIdentificationNumber = store.Company.TaxNumber,
                //        Address = new InvoiceBillerAddress
                //        {
                //            Street = $"{store.Street} {store.HouseNr}",
                //            ZIP = (ushort)(ushort.TryParse(store.Plz, out var plsValue) ? plsValue : 0),
                //            Town = store.City,
                //            Name = store.Name,
                //            Country = new InvoiceBillerAddressCountry
                //            {
                //                Value = store.Country.Name
                //            }
                //        }
                //    };
                //}

                //if (ValidateBill(model) == false)
                //    throw new HttpResponseException(
                //        new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                //        {
                //            ReasonPhrase = "Bill not valid"
                //        });

                //foreach (var invoice in model.Invoices)
                //{
                    var bill = new Bill
                    {
                        Amount = model.Amount,
                        Company = null,
                        ImportedAt = DateTime.Now,
                        Invoice = null,
                        InvoiceNumber = null,
                        InvoiceDate = DateTime.Now,
                        CashRegisterId = cashRegister.Id,
                        StoreApiToken = model.StoreApiToken,
                        IsDeleted=false,
                        UserId= model.UserId
                    };
                    //List<Bill> currentBillsOfUser = null;
                    //if (string.IsNullOrWhiteSpace(model.UserId))
                    //{
                    //    bill.AnonymousUserId = model.AnonymousUserId;
                    //    if (model.AnonymousUserId.HasValue)
                    //    {
                    //        currentBillsOfUser = _billService.FindAllForAnonymousUser(model.AnonymousUserId.Value);
                    //    }
                    //}
                    //else
                    //{
                    //    bill.UserId = model.UserId;
                    //    currentBillsOfUser = _billService.FindAllForUser(model.UserId);
                    //}

                    //if (currentBillsOfUser == null || currentBillsOfUser.Any(b =>
                    //        b.InvoiceNumber == bill.InvoiceNumber && b.Company == bill.Company &&
                    //        b.InvoiceDate.Date == bill.InvoiceDate.Date))
                    //{
                    //    continue;
                    //}

                    _billService.Add(bill);

                    //var phones = _mobilePhoneService.FindBy(p => p.UserId == model.UserId).ToList();
                    //var anonymousMobilePhones = new List<AnonymousMobilePhone>();
                    //if (model.AnonymousUserId.HasValue)
                    //{
                    //    var anonymousUser = _anonymousUserRepository.FindById(model.AnonymousUserId.Value);
                    //    if (anonymousUser != null)
                    //    {
                    //        anonymousMobilePhones.AddRange(anonymousUser.AnonymousMobilePhones);
                    //    }
                    //}

                    //foreach (var p in phones)
                    //{
                    //    var companyName = bill.Company.Length > 10
                    //        ? $"{bill.Company.Substring(0, 10)}..."
                    //        : bill.Company;
                    //    var n = new FirebaseNotification
                    //    {
                    //        Body =
                    //            $"Rechnung für {companyName} um {bill.Amount.ToString($"0.00 {invoice.InvoiceCurrency}")} vom {bill.Invoice.InvoiceDate:dd.MM.yyyy}",
                    //        CreatedOn = DateTime.Now,
                    //        LastAttemptOn = DateTime.Now,
                    //        DeviceId = p.Id,
                    //        Title = "Neue Rechnung!",
                    //        Type = "bill",
                    //        BillId = bill.Id
                    //    };
                    //    _firebaseNotificationService.Add(n);

                    //    // todo send notification to phone
                    //    await _firebaseNotificationService.Send(n);
                    //}

                    //foreach (var p in anonymousMobilePhones)
                    //{
                    //    var companyName = bill.Company.Length > 10
                    //        ? $"{bill.Company.Substring(0, 10)}..."
                    //        : bill.Company;
                    //    var n = new FirebaseNotification
                    //    {
                    //        Body =
                    //            $"Rechnung für {companyName} um {bill.Amount.ToString($"0.00 {invoice.InvoiceCurrency}")} vom {bill.Invoice.InvoiceDate:dd.MM.yyyy}",
                    //        CreatedOn = DateTime.Now,
                    //        LastAttemptOn = DateTime.Now,
                    //        AnonymousMobilePhoneId = p.Id,
                    //        Title = "Neue Rechnung!",
                    //        Type = "bill",
                    //        BillId = bill.Id
                    //    };
                    //    _firebaseNotificationService.Add(n);

                    //    // todo send notification to phone
                    //    await _firebaseNotificationService.Send(n);
                    //}
                //}

                return response;
            }
            catch (HttpResponseException he)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.Message
                    });
            }
        }

        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("AddAnonymousMobilePhone")]
        public AnonymousUserNewResponseModel AddAnonymousMobilePhone(AnonymousUserNewRequestModel model)
        {
            var anonymousUser = _billService.AddAnonymousUserPhone(model.FirebaseToken, model.AnonymousUserId);
            return new AnonymousUserNewResponseModel
            {
                AnonymousUserId = anonymousUser.Id
            };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("TagSummary")]
        public List<TagSummaryItem> TagSummary(Guid anonymousUserId)
        {
            var anonymousUser = _anonymousUserRepository.FindById(anonymousUserId);
            if (anonymousUser == null)
            {
                throw new AnonymousUserNotFoundException(anonymousUserId);
            }

            var bills = anonymousUser.UserId != null
                ? _billService.FindAllForUser(anonymousUser.UserId)
                : _billService.FindAllForAnonymousUser(anonymousUserId);

            var result = (from a in bills
                group a by a.CashRegister.Store.EBillCategory.Name
                into g
                select new TagSummaryItem()
                {
                    TagName = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(y => y.Amount)
                }).ToList();
            return result;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CSV")]
        public HttpResponseMessage CSV(Guid anonymousUserId)
        {
            var currentUser = User.Identity.GetUserId();
            List<Invoice> bills = null;
            if (currentUser != null)
            {
                bills = _billService.FindAllForUser(currentUser).Select(x => x.Invoice).ToList();
            }
            else
            {
                bills = _billService.FindAllForAnonymousUser(anonymousUserId).Select(x => x.Invoice).ToList();
            }

            var path = System.Web.Hosting.HostingEnvironment.MapPath(
                $"~/UploadedFiles/EBill/CSV/{anonymousUserId}.csv");

            var resultText = _billService.BillsCsv(bills);

            File.WriteAllText(path, resultText);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        private void GetHeader(Dictionary<string, string> result, Type t, object o,
            string path = "")
        {
            PropertyInfo[] fields = t.GetProperties();
            foreach (var field in fields)
            {
                if (field.PropertyType.IsPrimitive || field.PropertyType == typeof(string) ||
                    field.PropertyType == typeof(DateTime))
                {
                    result.Add(path + "." + field.Name, o == null ? null : field.GetValue(o).ToString());
                }
                else if (field.PropertyType.IsArray)
                {
                    var arrayType = field.PropertyType.GetElementType();
                    GetHeader(result, arrayType, path);
                }
                else
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        GetHeader(result, field.PropertyType, null, field.Name);
                    }
                    else
                    {
                        GetHeader(result, field.PropertyType, field.GetValue(o), path + "." + field.Name);
                    }
                }
            }
        }

        public string ToCsvFields(string separator, Type type, object o, int arrayPosition = 0)
        {
            StringBuilder linie = new StringBuilder();

            PropertyInfo[] fields = type.GetProperties();
            foreach (var field in fields)
            {
                if (field.DeclaringType != null && !field.DeclaringType.Assembly.GetName().Name.StartsWith("GCloud"))
                {
                    continue;
                }

                if (field.PropertyType.IsPrimitive || field.PropertyType == typeof(string) ||
                    field.PropertyType == typeof(DateTime))
                {
                    if (o == null)
                    {
                        linie.Append(separator);
                        continue;
                    }

                    var x = field.GetValue(o);

                    if (x != null)
                    {
                        linie.Append(x.ToString());
                    }

                    linie.Append(separator);
                }
                else if (field.PropertyType.IsArray)
                {
                    if (o == null)
                    {
                        linie.Append(ToCsvFields(separator, field.PropertyType.GetElementType(), null,
                            arrayPosition));
                        break;
                    }

                    var item = (Object[]) field.GetValue(o);
                    linie.Append(ToCsvFields(separator, field.PropertyType.GetElementType(), item[arrayPosition],
                        arrayPosition));
                }
                else
                {
                    linie.Append(ToCsvFields(separator, field.PropertyType, o != null ? field.GetValue(o) : null));
                }
            }

            return linie.ToString();
        }

        private bool ValidateBill(BillAddRequestModel model)
        {
            if (model?.Invoices == null || !model.Invoices.Any() ||
                model.Invoices.Any(i => string.IsNullOrEmpty(i.InvoiceNumber)))
                return false;

            var store = _storeService.FindByApiToken(model.StoreApiToken);

            if (store == null)
            {
                throw new ApiTokenInvalidException(model.StoreApiToken);
            }

            return true;
        }
    }
}