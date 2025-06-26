using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views
{
    public partial class SplashScreenPage : ContentPage
    {
        // Niz poruka koje će se ciklično prikazivati
        private readonly string[] _loadingMessages = new[]
      {
    "Willkommen! Gleich gibt’s frisches Schnitzel vom Feinsten.",
    "Ob im Restaurant oder draußen am Parkplatz – bestellen geht ganz bequem per App.",
    "Deine Bestellung? Bald in der Küche! Frisch, lecker & nur für dich zubereitet.",
    "Fast fertig... gleich kannst du losschlemmen, sammeln und sparen!"
};

        // Trenutni indeks u nizu
        private int _currentMessageIndex = 0;

        public SplashScreenPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Pokretanje animacija
            _ = RunIntroAndPulseAsync();
            _ = FadeInLoadingLabelAsync();

            // Startujemo timer za smenjivanje teksta
            StartLoadingTextRotation();

            // Učitavanje i navigacija
            _ = LoadDataAndNavigateAsync();
        }

        void StartLoadingTextRotation()
        {
            // Dispatcher.StartTimer vraća true da bi se timer ponovo pokrenuo
            Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                // inkrementiramo indeks i obezbeđujemo ciklično ponašanje
                _currentMessageIndex = (_currentMessageIndex + 1) % _loadingMessages.Length;
                LoadingLabel.Text = _loadingMessages[_currentMessageIndex];
                return true;
            });
        }

        async Task RunIntroAndPulseAsync()
        {
            // kratki “crni ekran”
            await Task.Delay(100);

            // 1) Fade-in
            await LogoImage.FadeTo(1, 400);

            // 2) Scale-to-normal→large
            await LogoImage.ScaleTo(1.2, 250, Easing.CubicIn);

            // 3) Start beskonačne pulsacije
            StartPulseAnimation();
        }

        void StartPulseAnimation()
        {
            var pulse = new Animation();

            // Prva polovina: 1.2 → 1.25
            pulse.Add(0, 0.5,
                new Animation(v => LogoImage.Scale = v, 1.2, 1.25, Easing.SinInOut));

            // Druga polovina: 1.25 → 1.2
            pulse.Add(0.5, 1,
                new Animation(v => LogoImage.Scale = v, 1.25, 1.2, Easing.SinInOut));

            pulse.Commit(
                owner: this,
                name: "LogoPulse",
                length: 1400,
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
            // ovde ide tvoja logika inicijalizacije
            await Task.Delay(3000);

            // nakon što se load završi
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
