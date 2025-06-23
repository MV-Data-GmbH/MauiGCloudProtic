using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using GCloud.Shared.Dto.Domain;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudShared.Service;
using GCloudPhone.Models;

namespace GCloudPhone.Services
{
    public class NFCDataHandler
    {
        private readonly INavigation _navigation;

        public NFCDataHandler(INavigation navigation)
        {
            _navigation = navigation;
        }

        public async Task HandleNFCData(string nfcDataJson)
        {
            Debug.WriteLine($"[NFCDataHandler.HandleNFCData] Ulazim sa JSON: {nfcDataJson}");

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try
            {
                var data = JsonConvert.DeserializeObject<NFCData>(nfcDataJson, settings);
                if (data == null)
                {
                    Debug.WriteLine("[NFCDataHandler.HandleNFCData] Deserializacija vratila null!");
                    return;
                }

                var filialeID = data.FilialeID;
                var tableNumber = data.TableNumber;
                var socketAddress = data.SocketIPAddress;

                Debug.WriteLine($"[NFCDataHandler.HandleNFCData] Parsed: FilialeID={filialeID}, TableNumber={tableNumber}, SocketIP={socketAddress}");

                Preferences.Set("IPAdresse", socketAddress);
                Preferences.Set("FilialeID", filialeID);
                Preferences.Set("TableNumber", tableNumber);

                await HandleSuccessfulConnection(filialeID);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCDataHandler.HandleNFCData] Error during deserialization: {ex}");
            }
        }

        private async Task HandleSuccessfulConnection(string filialeID)
        {
            Debug.WriteLine($"[NFCDataHandler.HandleSuccessfulConnection] Počinjem, FilialeID={filialeID}");
            try
            {
                var storeService = new StoreService();
                Debug.WriteLine("[NFCDataHandler.HandleSuccessfulConnection] Pozivam GetStores()");

                var result = await storeService.GetStores();
                if (result is List<StoreDto> stores)
                {
                    Debug.WriteLine($"[NFCDataHandler.HandleSuccessfulConnection] Dobijeno prodavnica: {stores.Count}");
                }
                else
                {
                    Debug.WriteLine("[NFCDataHandler.HandleSuccessfulConnection] GetStores() nije vratio List<StoreDto>!");
                }

                var storesList = result as List<StoreDto> ?? new List<StoreDto>();
                var targetStore = storesList.FirstOrDefault(s => s.Id == new Guid(filialeID));

                if (targetStore != null)
                {
                    Debug.WriteLine($"[NFCDataHandler.HandleSuccessfulConnection] Pronađena prodavnica: {targetStore.Name} ({targetStore.Id})");
                    Preferences.Set("SelectedStoreId", targetStore.Id.ToString());
                    Preferences.Set("SelectedStoreName", targetStore.Name);

                    Debug.WriteLine("[NFCDataHandler.HandleSuccessfulConnection] Navigacija ka CategoriesPage");
                    await _navigation.PushAsync(new CategoriesPage());
                }
                else
                {
                    Debug.WriteLine($"[NFCDataHandler.HandleSuccessfulConnection] Prodavnica sa ID={filialeID} nije pronađena!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCDataHandler.HandleSuccessfulConnection] Greška: {ex}");
            }
        }
    }
}
