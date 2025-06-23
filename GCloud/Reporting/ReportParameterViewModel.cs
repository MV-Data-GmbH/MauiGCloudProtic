using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;

namespace GCloud.Reporting
{
    /// <summary>
    /// Is used as the DTO between the ReportingView and the <see cref="GCloud.Controllers.ReportsController"/>
    /// </summary>
    public class ReportParameterViewModel
    {
        [DisplayName("Bericht-Gruppe")]
        public string ReportGroup { get; set; }
        [DisplayName("Bericht")]
        public string ReportName { get; set; }

        public object this[string key]
        {
            get => _parameters.ContainsKey(key) ? _parameters[key] : null;
            set => Add(key, value);
        }

        public int Count => _parameters.Count;

        /// <summary>
        /// Is a Key-Value Pair, that represents the selected "Settings" for a Parameters
        /// </summary>
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public object GetParameterAsString(string keyName)
        {
            string propertyValue;

            if (_parameters[keyName] is string[])
            {
                propertyValue = ((string[])_parameters[keyName]).FirstOrDefault();
            }
            else
            {
                propertyValue = (string)_parameters[keyName];
            }

            if (DateTime.TryParse(propertyValue, out var dateTimeValue))
            {
                return dateTimeValue;
            }

            return propertyValue;
        }

        /// <summary>
        /// Fügt einen Parameter hinzu oder überschreibt diesen mit dem neuen Wert
        /// </summary>
        /// <param name="keyname">Der Name des Parameters</param>
        /// <param name="value">Der Wert des Parameters</param>
        public void Add(string keyname, object value)
        {
            if (_parameters.ContainsKey(keyname))
            {
                _parameters[keyname] = value;
            }
            else
            {
                _parameters.Add(keyname, value);
            }
        }

        public ReportParameterViewModel Clone()
        {
            ReportParameterViewModel reportParameterViewModel = new ReportParameterViewModel();
            reportParameterViewModel.ReportName = new string(ReportName.ToCharArray());
            reportParameterViewModel._parameters = new Dictionary<string, object>();
            _parameters.ForEach(kvp => reportParameterViewModel._parameters.Add(kvp.Key, kvp.Value));
            return reportParameterViewModel;
        }
    }

    public class ReportParameterViewModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;

            var reportGroup = request.Form.Get("ReportGroup");
            var reportName = request.Form.Get("ReportName");
            //with the substring function i remove the brackets "B := {[,]}"
            var parameters = request.Form.AllKeys.Where(key => key.StartsWith("[") && key.EndsWith("]")).ToDictionary(key => key.Substring(1,key.Length-2), key => request.Form.Get(key));

            var model = new ReportParameterViewModel
            {
                ReportGroup = reportGroup,
                ReportName = reportName
            };

            parameters.ForEach(param => model.Add(param.Key, param.Value));

            return model;
        }
    }
}