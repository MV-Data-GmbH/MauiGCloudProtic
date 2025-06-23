using System;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views.Templates
{
    public partial class NavigationBar : ContentView
    {
        public NavigationBar()
        {
            InitializeComponent();
        }

        // Javni događaji na koje se roditeljske stranice mogu pretplatiti
        public event EventHandler HomeTapped;
        public event EventHandler ProductTapped;
        public event EventHandler BestellenTapped;
        public event EventHandler AktionenTapped;
        public event EventHandler PunkteTapped;

        private void OnHomeTapped(object sender, EventArgs e)
        {
            HomeTapped?.Invoke(this, e);
        }

       
        private void OnBestellenTapped(object sender, EventArgs e)
        {
            BestellenTapped?.Invoke(this, e);
        }

        private void OnAktionenTapped(object sender, EventArgs e)
        {
            AktionenTapped?.Invoke(this, e);
        }

        private void OnPunkteTapped(object sender, EventArgs e)
        {
            PunkteTapped?.Invoke(this, e);
        }
        private void OnSettingsTapped(object sender, EventArgs e)
        {
            ProductTapped?.Invoke(this, e);
        }

    }
}
