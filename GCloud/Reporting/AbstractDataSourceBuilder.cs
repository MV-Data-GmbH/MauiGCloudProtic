using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting
{
    public abstract class AbstractDataSourceBuilder<T> : IDataSourceBuilder where T : class, new()
    {
        protected ReportParameterViewModel ParameterViewModel;
        protected readonly Store _store;

        protected readonly IProcedureRepository _repository;

        private readonly List<string> _baseParameters = new List<string>() { "@companyId" };
        protected bool IgnoreCompanyParam = false;

        /// <summary>
        /// Declared if this Report is visible in the Report Selection for the Manager-User
        /// </summary>
        public bool IsVisible { get; set; } = true;

        protected AbstractDataSourceBuilder(bool isVisible)
        {
            IsVisible = isVisible;
        }

        /// <summary>
        /// This constructor is only used by the reflection framework to discover the reports by runtime.
        /// So don't use it in your code
        /// </summary>
        protected AbstractDataSourceBuilder(IProcedureRepository repository, ReportParameterViewModel parameterViewModel, Store store, bool isVisible)
        {
            _repository = repository;
            ParameterViewModel = parameterViewModel;
            _store = store;
            IsVisible = isVisible;
        }

        public abstract ReportGroup GetReportGroup();
        public abstract string GetReportName();
        public abstract string GetReportFolderName();
        public abstract string GetReportFileName();
        public abstract string GetStoredProcedureName();

        public virtual string GetSchema()
        {
            return "dbo";
        }

        public List<string> GetParametersForProcedure()
        {
            if (IgnoreCompanyParam)
                return GetProcedureParameterNames();
            return _baseParameters.Union(GetProcedureParameterNames()).Distinct().ToList();
        }

        /// <summary>
        /// Specifies the parameters of the StoredProcedure associated with this DataSourceBuilder.
        /// You don't need to specify the parameter "@companyId", because it is always included by default
        /// </summary>
        /// <returns>A list of all the names for the SP_parameter names. For example:
        ///<code>return new List&lt;string&gt;{"@startDate", "@endDate"};</code>
        /// </returns>
        public abstract List<string> GetProcedureParameterNames();

        public IList<SqlParameter> GetParameters()
        {
            return GetParametersForProcedure().Select(name =>
            {
                string parameterValue;
                if (ParameterViewModel.Count > 0 && ParameterViewModel[name] is string[])
                {
                    parameterValue = ((string[])ParameterViewModel[name]).First();
                }
                else if (ParameterViewModel[name] is Guid guid)
                {
                    return new SqlParameter(name, guid);
                }
                else
                {
                    parameterValue = ParameterViewModel[name].ToString();
                }

                if (DateTime.TryParse(parameterValue, out var datetime))
                {
                    return new SqlParameter(name, datetime);
                }
                return new SqlParameter(name, parameterValue);
            }).ToList();
        }

        public virtual List<Microsoft.Reporting.WebForms.ReportParameter> GetReportParameters()
        {
            List<Microsoft.Reporting.WebForms.ReportParameter> parameters = new List<Microsoft.Reporting.WebForms.ReportParameter>();
            parameters.Add(ReportParameter.CompanyName.WithValue(_store.Name));
            parameters.Add(ReportParameter.CompanyAddress.WithValue(_store.Street));
            parameters.Add(ReportParameter.DateFrom.WithValue(ParameterViewModel.GetParameterAsString("@startDate").ToString()));
            parameters.Add(ReportParameter.DateTo.WithValue(ParameterViewModel.GetParameterAsString("@endDate").ToString()));
            return parameters;
        }

        public IEnumerable LoadData()
        {
            return _repository.ExecuteProcedure<T>(GetStoredProcedureName(), GetParameters());
        }

        public abstract void HandleSubreportProcessing(object sender, SubreportProcessingEventArgs e);
    }

}