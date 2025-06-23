using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views
{
    public partial class SplashScreenPage : ContentPage
    {
        public SplashScreenPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _ = RunIntroAndPulseAsync();
            _ = FadeInLoadingLabelAsync();
            _ = LoadDataAndNavigateAsync();
        }

        // Kombinovana metoda: intro + pokretanje pulsiranja
        async Task RunIntroAndPulseAsync()
        {
            // kratki “crni ekran”
            await Task.Delay(100);

            // 1) Fade-in
            await LogoImage.FadeTo(1, 400);

            // 2) Scale-to-normal→large, npr. 0.5→1.2
            await LogoImage.ScaleTo(1.2, 250, Easing.CubicIn);

            // 3) Start beskonačne pulsacije koristeći Animation API (ne blokira)
            StartPulseAnimation();
        }

        void StartPulseAnimation()
        {
            // Kreiramo animaciju koja pulsira između 1.2 i 1.25
            var pulse = new Animation();

            // Prva polovina: 1.2 → 1.25
            pulse.Add(0, 0.5,
                new Animation(v => LogoImage.Scale = v, 1.2, 1.25, Easing.SinInOut));

            // Druga polovina: 1.25 → 1.2
            pulse.Add(0.5, 1,
                new Animation(v => LogoImage.Scale = v, 1.25, 1.2, Easing.SinInOut));

            // Commit sa ponavljanjem
            pulse.Commit(
                owner: this,
                name: "LogoPulse",
                length: 1400,         // možeš i smanjiti duration ako želiš brže pulsiranje
                easing: null,
                repeat: () => true
            );
        }


        async Task FadeInLoadingLabelAsync()
        {
            await Task.Delay(200);
            await LoadingLabel.FadeTo(1, 400);
        }

        async Task LoadDataAndNavigateAsync()
        {
            // ovde idu tvoje stvarne inicijalizacije, npr. ImportDataAsync()
            await Task.Delay(3000);

            // nakon što se load završi
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
