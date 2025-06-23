using GCloudPhone.Models;
using GCloudPhone.Services;
using Plugin.NFC;
using System;
using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.ShoppingCart;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views
{
    public partial class NFCReaderPage : ContentPage
    {
        private NFCData _storedNfcData;

        public NFCReaderPage(INfcService nfcService)
        {
            InitializeComponent();

            CrossNFC.Current.StartListening();
            // Subscribe to the NFC events
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
            CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
            CrossNFC.Current.OnTagConnected += Current_OnTagConnected;
            CrossNFC.Current.OnTagDisconnected += Current_OnTagDisconnected;
        }

        private void StartNfcScanning(object sender, EventArgs e)
        {
            CrossNFC.Current.StartListening();
        }

        private void Current_OnNfcStatusChanged(bool isEnabled)
        {
            Debug.WriteLine($"NFC Status Changed: {(isEnabled ? "Enabled" : "Disabled")}");
        }

        private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
        {
            if (tagInfo == null) return;

            if (!tagInfo.IsSupported)
            {
                Debug.WriteLine("Unsupported tag type.");
                return;
            }

            if (tagInfo.Records?.Length > 0)
            {
                foreach (var record in tagInfo.Records)
                {
                    Debug.WriteLine($"Record: {record.Message}");
                }
            }
            else
            {
                Debug.WriteLine("No records found on tag.");
            }
        }

        private async void Current_OnMessageReceived(ITagInfo tagInfo)
        {
            // 1. Debug na samom početku event-a
            Debug.WriteLine("▶▶ Current_OnMessageReceived pokrenut");
            Console.WriteLine("▶▶ Current_OnMessageReceived pokrenut");

            if (tagInfo == null)
            {
                Debug.WriteLine("‼ tagInfo je null, izlazim.");
                return;
            }

            if (tagInfo.Records != null && tagInfo.Records.Length > 0)
            {
                Debug.WriteLine($"ℹ Records.Length = {tagInfo.Records.Length}");
                foreach (var record in tagInfo.Records)
                {
                    string nfcDataJson = record.Message;
                    Debug.WriteLine($"📄 NDEF raw message (debug): {nfcDataJson}");
                    Console.WriteLine($"NDEF Record Message: {nfcDataJson}");

                    var nfcDataHandler = new NFCDataHandler(Navigation);
                    await nfcDataHandler.HandleNFCData(nfcDataJson);
                }
            }
            else
            {
                Debug.WriteLine("ℹ Nema Records na tagu.");
            }

            Debug.WriteLine($"⚙ App.IsRepeatOrder = {App.IsRepeatOrder}");
            Console.WriteLine($"IsRepeatOrder value: {App.IsRepeatOrder}");

            if (App.IsRepeatOrder)
            {
                App.IsRepeatOrder = false;

                // 2. Debug pre poziva PushAsync
                Debug.WriteLine("▶ Priprema za navigaciju na Warenkorb");
                Console.WriteLine("Navigating to Warenkorb...");

                Device.BeginInvokeOnMainThread(async () =>
                {
                    Debug.WriteLine("▶ BEGIN InvokeOnMainThread");
                    await Navigation.PushAsync(new Warenkorb());
                    Debug.WriteLine("▶ PushAsync završen");
                });
            }
        }

        private void Current_OnTagConnected(object sender, EventArgs e)
        {
            Debug.WriteLine("Tag connected.");
        }

        private void Current_OnTagDisconnected(object sender, EventArgs e)
        {
            Debug.WriteLine("Tag disconnected.");
        }
    }
}
