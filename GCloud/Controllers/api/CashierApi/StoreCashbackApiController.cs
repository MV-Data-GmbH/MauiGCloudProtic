using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using GCloud.Service;
using GCloud.Shared.Dto.Domain;
using Microsoft.Ajax.Utilities;

namespace GCloud.Controllers.api.CashierApi
{
    [AllowAnonymous]
    public class StoreCashbackApiController : ApiController
    {
        private readonly ICashbackService _cashbackService;

        public StoreCashbackApiController(ICashbackService cashbackService)
        {
            _cashbackService = cashbackService;
        }

        /// <summary>
        /// Wenn der Kunde KEIN Cashback aufbrauchen möchte, sondern einfach nur Cashback sammeln möchte
        /// </summary>
        /// <param name="storeApiToken">Der API Token welcher den Stroe identifiziert</param>
        /// <param name="userId">Identifiziert den User, welcher das Cashback verwendet</param>
        /// <param name="invoiceAmount">Der Betrag der Rechnung welcher zum aufladen des Guthabens als Referenzwert verwendet wird.</param>
        /// <param name="cashRegisterId">Der Geräte identifier für das registrierte Gerät</param>
        /// <returns>Den neuen Cashback Eintrag welcher den neuen Stand des Cashbacks enthält</returns>
        public CashbackDto Post(string storeApiToken, string userId, decimal invoiceAmount, Guid cashRegisterId)
        {
            return Mapper.Map<CashbackDto>(_cashbackService.ApplyCashback(storeApiToken, userId, invoiceAmount, cashRegisterId));
        }

        /// <summary>
        /// Wenn der Kunde seine Rechnung teilweise order ganz mit Cashback bezahlt wurde
        /// </summary>
        /// <param name="storeApiToken">Der API Token welcher den Store identifiziert</param>
        /// <param name="userId">Identifiziert den User, welcher das Cashback verwendet</param>
        /// <param name="paymentAmount">Gibt an wie viel Geld der User von seinem Cashback-Guthaben verwenden/verbrauchen möchte</param>
        /// <param name="invoiceAmount">Der Betrag der Rechnung vor Abzug des Cashbacks</param>
        /// <param name="cashRegisterId">Der Geräte identifier für das registrierte Gerät</param>
        /// <returns>Den neuen Cashback Eintrag welcher den neuen Stand des Cashbacks enthält</returns>
        public CashbackDto Post(string storeApiToken, string userId, decimal paymentAmount, decimal invoiceAmount, Guid cashRegisterId)
        {
            return Mapper.Map<CashbackDto>(_cashbackService.UseCashback(storeApiToken, userId, paymentAmount, invoiceAmount, cashRegisterId));
        }
    }
}
