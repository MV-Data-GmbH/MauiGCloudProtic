using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Models.Domain
{
    public class Cashback : TurnoverJournal
    {
        public decimal CreditOld { get; set; }
        public decimal CreditChange { get; set; }
        public decimal CreditNew { get; set; }
    }
}