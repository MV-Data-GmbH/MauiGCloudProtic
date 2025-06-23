using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GCloud.Controllers.ViewModels.Store
{
    public class NotificationEditViewModel
    {
        public List<NotificationDaySelectionViewModel> DaySelection { get; set; }
        [Required]
        public Guid StoreId { get; set; }
        [DisplayName("Nachricht")]
        [Required]
        [MaxLength(100, ErrorMessage = "Die Nachricht darf maximal 100 Zeichen lang sein")]
        public string Message { get; set; }
    }

    public class NotificationDaySelectionViewModel
    {
        [DisplayName("Wochentag")]
        public DayOfWeek DayOfWeek { get; set; }
        [DisplayName("Vormittag")]
        public bool Vormittag { get; set; }
        [DisplayName("Mittag")]
        public bool Mittag { get; set; }
        [DisplayName("Nachmittag")]
        public bool Nachmittag { get; set; }
        [DisplayName("Abend")]
        public bool Abend { get; set; }
    }
}