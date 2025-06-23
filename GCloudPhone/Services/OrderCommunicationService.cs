using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GCloudShared.Repository;
using GCloudShared.Shared;
using GCloudPhone.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;

namespace GCloudPhone.Services
{
    public class OrderCommunicationService
    {
        /// <summary>
        /// Šalje narudžbinu SignalR-om na odabranu kasu, uz retry logiku.
        /// </summary>
        public async Task<bool> SendOrderToServerAsync(Orders order, List<OrderItems> orderItems)
        {
            var orderWithItems = new OrderWithItems
            {
                Order = order,
                Items = orderItems ?? new List<OrderItems>()
            };

            if (Config.PaymentWithoutDataTransfer == "No")
            {
                try
                {
                    string serializedOrderWithItems = OrderTools.SerializeOrderWithItemsAsync(orderWithItems);
                    string cashRegisterName = await GetCashRegisterNameAsync();

                    Debug.WriteLine($"[SendOrder] Raw cashRegisterName = '{cashRegisterName}'");
                    Debug.WriteLine($"[SendOrder] OnlineUsers = {string.Join(", ", App.SignalR.OnlineUsers)}");

                    if (!string.IsNullOrEmpty(cashRegisterName))
                    {
                        // Normalizuj i uporedi insenzitivno na velika/mala slova
                        var crName = cashRegisterName.Trim();
                        bool isOnline = App.SignalR.OnlineUsers
                            .Any(u => u.Trim().Equals(crName, StringComparison.OrdinalIgnoreCase));

                        if (!isOnline)
                        {
                            Debug.WriteLine($"[SendOrder] Target '{crName}' offline according to SignalR list.");
                            return false;
                        }

                        int retryCount = 0;
                        const int maxRetries = 5;
                        const int retryDelay = 10000; // 10 sekundi

                        while (retryCount < maxRetries)
                        {
                            bool sent = await App.SignalR.SendMessageToUser(crName, serializedOrderWithItems);
                            if (sent)
                            {
                                Debug.WriteLine("Order sent successfully.");
                                return true;
                            }
                            else
                            {
                                retryCount++;
                                Debug.WriteLine($"Target offline. Retrying... ({retryCount}/{maxRetries})");
                                await Task.Delay(retryDelay);
                            }
                        }

                        Debug.WriteLine("Order could not be sent after multiple attempts.");
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("No cash register name available. Cannot send the order.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error sending order to server: {ex}");
                    return false;
                }
            }

            // Ako ne šaljemo preko data transfera, tretiramo kao neuspeh ovde
            return false;
        }

        /// <summary>
        /// Vraća naziv kase za odabrani store iz baze, sa trimovanjem i logovanjem.
        /// </summary>
        public async Task<string> GetCashRegisterNameAsync()
        {
            // 1) Ispiši šta je u Preferences
            var prefId = Preferences.Get("SelectedStoreId", string.Empty);
            Debug.WriteLine($"[GetCashRegisterName] SelectedStoreId from Preferences = '{prefId}'");

            if (string.IsNullOrWhiteSpace(prefId))
            {
                Debug.WriteLine("[GetCashRegisterName] PrefId je prazan ili null.");
                return null;
            }

            // 2) Ako postoji metoda za sve prodavnice, ispiši ih da vidiš šta baza drži
            try
            {
                var all = await SQL.GetAllStoresAsync();
                foreach (var s in all)
                {
                    Debug.WriteLine($"[GetAllStores] Id='{s.Id}', Name='{s.Name}', CashRegisterName='{s.CashRegisterName}'");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetCashRegisterName] Ne mogu da dohvatim sve prodavnice: {ex.Message}");
            }

            // 3) Dohvati baš tu jednu
            Stores store = await SQL.GetStoreByID(prefId);
            if (store == null)
            {
                Debug.WriteLine($"[GetCashRegisterName] NIJE pronađen store za Id '{prefId}'");
                return null;
            }

            // 4) Ispiši njegove vrednosti
            Debug.WriteLine($"[GetCashRegisterName] Fetched Store.Id = '{store.Id}'");
            Debug.WriteLine($"[GetCashRegisterName] Fetched Store.Name = '{store.Name}'");
            Debug.WriteLine($"[GetCashRegisterName] Fetched raw CashRegisterName = '{store.CashRegisterName}'");

            // 5) Trimuj i vrati
            var trimmed = store.CashRegisterName?.Trim() ?? "";
            Debug.WriteLine($"[GetCashRegisterName] Trimmed CashRegisterName = '{trimmed}'");
            return trimmed;
        }

    }
}
