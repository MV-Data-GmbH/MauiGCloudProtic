using System;
using System.Linq;
using Microsoft.Maui.Controls;
using GCloudShared.Interface;
using GCloudPhone.Views;
using GCloudPhone.Views.Settings.MyAccount; // Obavezno da LoginPage, RegisterPage, ForgotPassword i ChangePassword budu dostupni

namespace GCloudPhone.Views.Templates
{
    public partial class BackBtnText : ContentView
    {
        // Javna property koju roditeljska stranica treba da postavi
        public IAuthService AuthService { get; set; }

        public BackBtnText()
        {
            InitializeComponent();
        }

        // Event handler za tap gesture
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            // Dobijamo trenutnu stranicu iz NavigationStack-a
            var currentPage = Navigation?.NavigationStack?.LastOrDefault();

            // Ako je trenutna stranica RegisterPage, ForgotPassword ili ChangePassword, navigiraj na LoginPage
            if (currentPage is RegisterPage || currentPage is ForgotPassword || currentPage is ChangePassword)
            {
                if (AuthService != null)
                {
                    await Navigation.PushAsync(new LoginPage(AuthService));
                }
                else
                {
                    // Ako AuthService nije postavljen, fallback na pop
                    await Navigation.PopAsync();
                }
            }
            else
            {
                // U svim ostalim slučajevima samo se vrati na prethodnu stranicu
                await Navigation.PopAsync();
            }
        }
    }
}
