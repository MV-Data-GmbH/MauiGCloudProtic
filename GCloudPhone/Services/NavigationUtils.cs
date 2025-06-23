using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.OrderProccess;

namespace GCloudPhone.Services
{
    public static class NavigationUtils
    {
        public static async Task NavigateToCategoriesPage(INavigation navigation)
        {
            await navigation.PushAsync(new CategoriesPage());
        }
    }
}
