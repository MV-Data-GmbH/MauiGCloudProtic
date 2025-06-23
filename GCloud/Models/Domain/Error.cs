using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace GCloud.Models.Domain
{
    public class Error : IIdentifyable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string HttpMethod { get; set; }
        public string RequestContent { get; set; }
        public string Url { get; set; }
        public string ExceptionType { get; set; }   
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Data
        {
            get => JsonConvert.SerializeObject(_dataDictionary);
            set => _dataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
        }

        public string IpAddress { get; set; }
        public DateTime CreationDateTime { get; set; }
        public bool IsDeleted { get; set; }
        public Guid GetId()
        {
            return Id;
        }

        [NotMapped]
        private Dictionary<string, object> _dataDictionary = new Dictionary<string, object>();

        public void AddData(string key, object data)
        {
            _dataDictionary.Add(key, data);
        }

        public void ParseExceptionData(Exception exception)
        {
            switch (exception)
            {
                case DbEntityValidationException entityValidationException:
                    AddData(nameof(DbEntityValidationResult), string.Join(",",entityValidationException.EntityValidationErrors.SelectMany(ex => ex.ValidationErrors).Select(x => $"{x.PropertyName} -> {x.ErrorMessage}")));
                    break;
            }
        }
    }
}