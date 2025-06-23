using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GCloud.Controllers.ViewModels.Store;

namespace GCloud.Controllers.ModelBinders
{
    public class NotificationModelBinder : DefaultModelBinder
    {
        private static readonly ICollection<DayOfWeek> Days = new List<DayOfWeek>{DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday,DayOfWeek.Sunday};

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;

            var model = new NotificationEditViewModel{DaySelection = new List<NotificationDaySelectionViewModel>(7)};

            foreach (var dayOfWeek in Days)
            {
                var day = dayOfWeek;
                var vormittag = request.Form.Get($"{dayOfWeek}.Vormittag")?.Trim().ToUpper().Equals("on".ToUpper()) ?? false;
                var mittag = request.Form.Get($"{dayOfWeek}.Mittag")?.Trim().ToUpper().Equals("on".ToUpper()) ?? false;
                var nachmittag = request.Form.Get($"{dayOfWeek}.Nachmittag")?.Trim().ToUpper().Equals("on".ToUpper()) ?? false;
                var abend = request.Form.Get($"{dayOfWeek}.Abend")?.Trim().ToUpper().Equals("on".ToUpper()) ?? false;
                var dayModel = new NotificationDaySelectionViewModel
                {
                    DayOfWeek = day,
                    Vormittag = vormittag,
                    Nachmittag = nachmittag,
                    Mittag = mittag,
                    Abend = abend
                };
                model.DaySelection.Add(dayModel);
            }

            model.StoreId = Guid.Parse(request.Form.Get("StoreId"));
            model.Message = request.Form.Get("Message");

            return model;
        }
    }
}