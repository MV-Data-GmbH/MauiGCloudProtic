using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Shared.Dto;
using GCloud.Shared.Exceptions.Anonymous;
using GCloud.Shared.Exceptions.User;

namespace GCloud.Service.Impl
{
    public class BillService : AbstractService<Bill>, IBillService
    {
        private readonly IAnonymousMobilePhoneRepository _anonymousMobilePhoneRepository;
        private readonly IAnonymousUserRepository _anonymousUserRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStoreRepository _storeRepository;

        public BillService(IAbstractRepository<Bill> repository,
            IAnonymousMobilePhoneRepository anonymousMobilePhoneRepository,
            IAnonymousUserRepository anonymousUserRepository,
            IUserRepository userRepository,
            IStoreRepository storeRepository) : base(repository)
        {
            _anonymousMobilePhoneRepository = anonymousMobilePhoneRepository;
            _anonymousUserRepository = anonymousUserRepository;
            _userRepository = userRepository;
            _storeRepository = storeRepository;
        }

        public void DeleteAnonymousUser(Guid anonymousUserId)
        {
            var anonymousUser = _anonymousUserRepository.FindById(anonymousUserId);
            if (anonymousUser != null && anonymousUser.User == null)
            {
                _anonymousUserRepository.Delete(anonymousUser);
            }

            var anonymousMobilePhone =
                _anonymousMobilePhoneRepository.FindBy(x => x.AnonymousUserId == anonymousUserId).FirstOrDefault();

            if (anonymousMobilePhone != null)
            {
                _anonymousMobilePhoneRepository.Delete(anonymousMobilePhone);
            }
        }

        public AnonymousUser AddAnonymousUserPhone(string firebaseToken, Guid? anonymousUserId)
        {
            if (string.IsNullOrWhiteSpace(firebaseToken))
            {
                throw new InvalidFirebaseTokenException(firebaseToken);
            }

            var anonymousMobilePhone =
                _anonymousMobilePhoneRepository.FindFirstOrDefault(x => x.FirebaseInstanceId == firebaseToken) ??
                new AnonymousMobilePhone
                {
                    FirebaseInstanceId = firebaseToken
                };

            var anonymousUser =
                anonymousMobilePhone.AnonymousUser ?? _anonymousUserRepository.FindById(anonymousUserId);
            if (anonymousUser == null)
            {
                anonymousUser = new AnonymousUser
                {
                    AnonymousMobilePhones = new List<AnonymousMobilePhone> {anonymousMobilePhone}
                };
                _anonymousUserRepository.Add(anonymousUser);
            }
            else
            {
                if (anonymousUser.AnonymousMobilePhones.All(x => x.Id != anonymousMobilePhone.Id))
                {
                    anonymousMobilePhone.AnonymousUserId = anonymousUser.Id;
                    _anonymousMobilePhoneRepository.Add(anonymousMobilePhone);
                }
            }

            return anonymousUser;
        }

        public List<Bill> FindAllForUser(string userId, List<Guid> alreadyGot = null)
        {
            var user = _userRepository.FindById(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            var bills = _repository.FindBy(bill => bill.UserId == userId).ToList();
            var anonymousBills = user.AnonymousUsers.SelectMany(anonymous => anonymous.Bills).ToList();

            if (alreadyGot == null)
            {
                alreadyGot = new List<Guid>();
            }

            var allBills = bills.Union(anonymousBills).Where(b => !alreadyGot.Contains(b.Id)).ToList();
            //allBills.ForEach(x =>
            //{
            //    var store = _storeRepository.FindById(x.CashRegister.StoreId);
            //    x.Invoice.Tag = store?.EBillCategory?.Name ?? "";
            //});

            //chnaged:
            List<Bill> billsForView = new List<Bill>();

            foreach (var bill in allBills)
            {
                if(bill.Invoice != null)
                {
                    var store = _storeRepository.FindById(bill.CashRegister.StoreId);
                    bill.Invoice.Tag = store?.EBillCategory?.Name ?? "";

                    billsForView.Add(bill);
                }
            }

            //return allBills
            return billsForView;
            
        }

        public List<Bill> FindAllForAnonymousUser(Guid anonymousUserId, List<Guid> alreadyGot = null)
        {
            alreadyGot = alreadyGot ?? new List<Guid>();
            var anonymousUser = _anonymousUserRepository.FindById(anonymousUserId);
            if (anonymousUser == null)
            {
                throw new AnonymousUserNotFoundException(anonymousUserId);
            }

            var bills = (anonymousUser?.Bills ?? new List<Bill>()).Where(b => !alreadyGot.Contains(b.Id)).ToList();
            //bills.ForEach(x =>
            //{
            //    var store = _storeRepository.FindById(x.CashRegister.StoreId);
            //    x.Invoice.Tag = store?.EBillCategory?.Name ?? "";
            //});

            //chnaged:
            List<Bill> billsForView = new List<Bill>();

            foreach (var bill in bills)
            {
                if (bill.Invoice != null)
                {
                    var store = _storeRepository.FindById(bill.CashRegister.StoreId);
                    bill.Invoice.Tag = store?.EBillCategory?.Name ?? "";

                    billsForView.Add(bill);
                }
            }

            //return bills
            return billsForView;
            
        }

        public Bill FindByIdForUser(string userId, Guid billId)
        {
            var user = _userRepository.FindById(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            var bill = user.Bills?.Union(user.AnonymousUsers.SelectMany(u => u.Bills))
                .FirstOrDefault(b => b.Id == billId);
            var store = _storeRepository.FindById(bill?.CashRegister.StoreId);
            //if (bill != null)
            //{
            //    bill.Invoice.Tag = store?.EBillCategory?.Name ?? "";
            //}

            if (bill.Invoice != null)
            {
                
                bill.Invoice.Tag = store?.EBillCategory?.Name ?? "";
            }

            return bill;
        }

        public Bill FindByIdForAnonymous(Guid anonymousUserId, Guid billId)
        {
            var anonymousUser = _anonymousUserRepository.FindById(anonymousUserId);
            if (anonymousUser == null)
            {
                throw new AnonymousUserNotFoundException(anonymousUserId);
            }

            var bill = anonymousUser?.Bills?.Where(b => b.Id == billId).FirstOrDefault();
            var store = _storeRepository.FindById(bill?.CashRegister.StoreId);
            //if (bill != null)
            //{
            //    bill.Invoice.Tag = store?.EBillCategory?.Name ?? "";
            //}

            if (bill.Invoice != null)
            {

                bill.Invoice.Tag = store?.EBillCategory?.Name ?? "";
            }

            return bill;
        }

        public String BillsCsv(List<Invoice> billList)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "InvoiceNumber;InvoiceDate;VATIdentificationNumber;StoreName;BillerStreet;BillerTown;BillerPlz;BillerCountry;ComanyName;ProductOrderItem;Quantity;Price;Tag;Signature");
            foreach (var bill in billList)
            {
                foreach (var listItem in bill.Details.ItemList.ListLineItem)
                {
                    builder.Append($"{bill.InvoiceNumber};");
                    builder.Append($"{bill.InvoiceDate};");
                    builder.Append($"{bill.Biller.VATIdentificationNumber};");
                    builder.Append($"{bill.Biller.Address.Name};");
                    builder.Append($"{bill.Biller.Address.Street};");
                    builder.Append($"{bill.Biller.Address.Town};");
                    builder.Append($"{bill.Biller.Address.ZIP};");
                    builder.Append($"{bill.Biller.Address.Country.Value};");
                    builder.Append($"{bill.Biller.ComanyName};");
                    builder.Append($"{listItem.Description};");
                    builder.Append($"{listItem.Quantity.Value} {listItem.Quantity.Unit};");
                    builder.Append($"{listItem.LineItemAmount};");
                    builder.Append($"{bill.Tag};");
                    builder.Append($"{bill.JwsSignature};");
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}