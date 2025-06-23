using Newtonsoft.Json;
using GCloudPhone.Models;
using GCloudPhone.Views.Shop;
using GCloudPhone.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using GCloudPhone.Views.Shop.OrderProccess;

namespace GCloudPhone.Services
{
    public class QRCodeHandler
    {
        private readonly INavigation _navigation;
        // Staticka zastavica koja signalizira da se QR kod već obrađuje
        private static bool _isProcessing = false;

        public QRCodeHandler(INavigation navigation)
        {
            _navigation = navigation;
        }

        public async Task HandleQRCode(string qrCodeData)
        {
            // Ako je već u toku obrada QR koda, izlazimo odmah da se ne duplira logika
            if (_isProcessing)
            {
                Logger.LogInfo("Obrada QR koda već je u toku. Preskačem ponovnu obradu.");
                return;
            }

            _isProcessing = true; // Postavlja zastavicu da je obrada u toku

            string shortId = "";
            string tableNumber = "";

            Logger.LogInfo($"QR Code Data: {qrCodeData}");

            // Podešavanja za deserializaciju (ignoriše nepostojeće članove)
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try
            {
                // Očekujemo JSON koji sadrži "FID" i "TN"
                QRCodeData data = JsonConvert.DeserializeObject<QRCodeData>(qrCodeData, settings);

                shortId = data.ShortId;       // vrijednost iz "FID"
                tableNumber = data.TableNumber; // vrijednost iz "TN"

                // Čuvanje podataka u Preferences (ključ "FilialeID" se koristi za kompatibilnost)
                Preferences.Set("FilialeID", shortId);
                Preferences.Set("TableNumber", tableNumber);

                Logger.LogInfo("Podaci iz QR koda uspješno deserializovani i sačuvani u Preferences.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Greška prilikom deserializacije QR koda:");
                Logger.LogError(ex);
            }

            try
            {
                await HandleSuccessfulConnection(shortId);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async Task HandleSuccessfulConnection(string shortId)
        {
            if (string.IsNullOrWhiteSpace(shortId))
            {
                Logger.LogError("Greška: ShortId je prazan ili null.");
                return;
            }

            try
            {
                // Učitava se lista prodavnica iz SQLite baze
                List<Stores> stores = await SQL.GetAllStoresAsync();

                Logger.LogInfo("Dohvaćene prodavnice iz SQLite:");
                foreach (var store in stores)
                {
                    Logger.LogInfo($"Store Id: {store.Id}, Name: {store.Name}, ShortId: '{store.ShortId}'");
                }

                Stores targetStore = stores.FirstOrDefault(store =>
                    !string.IsNullOrWhiteSpace(store.ShortId) &&
                    store.ShortId.Trim().Equals(shortId.Trim(), StringComparison.OrdinalIgnoreCase)
                );

                if (targetStore != null)
                {
                    try
                    {
                        Preferences.Set("SelectedStoreId", targetStore.Id.ToString());
                        Preferences.Set("SelectedStoreName", targetStore.Name);
                        await _navigation.PushAsync(new CategoriesPage());
                        Logger.LogInfo("Navigacija na CategoriesPage je uspješna.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Greška pri navigaciji:");
                        Logger.LogError(ex);
                    }
                }
                else
                {
                    Logger.LogError("Error: Store not found with the provided ShortId.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("An error occurred during connection handling:");
                Logger.LogError(ex);
            }
        }
    }
}