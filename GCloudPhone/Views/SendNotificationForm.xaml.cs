using GCloudShared.Interface;
using Newtonsoft.Json;
using System.Text;
using GCloudPhone.Models;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel.Communication;

namespace GCloudPhone.Views;

public partial class SendNotificationForm : ContentPage
{

    private IAuthService _authService;
    private string _deviceToken;
    private readonly HttpClient _client = new HttpClient();

    public SendNotificationForm(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        ReadFireBaseAdminSdk();
        WeakReferenceMessenger.Default.Register<PushNotificationReceived>(this, (r, m) =>
        {
            string msg = m.Value;
        });

    }

    private async void ReadFireBaseAdminSdk()
    {
        var stream = await FileSystem.OpenAppPackageFileAsync("schnitzelweltsecondversion-firebase-adminsdk-3ixbz-0d906abb7a.json");
        var reader = new StreamReader(stream);

        var jsonContent = reader.ReadToEnd();

        try
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(jsonContent)
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FirebaseApp already initialized: {ex.Message}");
        }
    }

   

    private async void OnSendNotificationClicked(object sender, EventArgs e)
    {
        string title = NotificationTitle.Text;
        string body = NotificationBody.Text;
        
        if (Preferences.ContainsKey("DeviceToken"))
        {
            _deviceToken = Preferences.Get("DeviceToken", "");
        }

        bool userConfirmed = await App.Current.MainPage.DisplayAlert("Bestätigung", "Sind Sie sicher, dass Sie eine Benachrichtigung an alle Benutzer senden möchten?", "Ja", "Nein");

        if (userConfirmed)
        {

            bool userConfirmedTwice = await App.Current.MainPage.DisplayAlert("Bestätigung", "Durch das Klicken auf dies senden Sie die Benachrichtigung an alle Benutzer, die diese App installiert haben. Sind Sie sicher, dass Sie fortfahren möchten?", "Ja", "Nein");

            if (userConfirmedTwice)
            {

                try
                {
                    //List<string> userList = await _authService.GetAllUsers();
                    //string baseURL = "https://schnitzelwelttest1.willessen.online/";
                    //string email = "aaaa@gmail.com";
                    //string apiUrl = $"{baseURL}api/HomeApi/GetUserDeviceIds?userEmail={Uri.EscapeDataString(email)}";

                    //string json = await _client.GetStringAsync(apiUrl);

                    //// Deserialize the JSON array into a List<string>
                    //List<string> userDeviceIds = JsonConvert.DeserializeObject<List<string>>(json);
                    //List<string> userList = userDeviceIds.ToList();

                    List<string> userList = new List<string> { _deviceToken };



                    if (userList != null && userList.Any())
                    {
                        userList = userList.Where(s => !string.IsNullOrEmpty(s)).ToList();

                        var androidNotificationObject = new Dictionary<string, string> { { "NavigationID", "1" } };
                        var iosNotificationObject = new Dictionary<string, object> { { "NavigationID", "1" } };

                       
                        int successCount = 0;
                        int failureCount = 0;

                        //FirebaseMessaging is limited to 500 deviceIds in one call
                        int chunkSize = 500;

                        for (int i = 0; i < userList.Count; i += chunkSize)
                        {
                            var chunkedList = userList.Skip(i).Take(chunkSize).ToList();
                            var messageList = chunkedList.Select(deviceToken => new Message
                            {
                                Token = deviceToken,
                                Notification = new Notification
                                {
                                    Title = title,
                                    Body = body,
                                },
                                Data = androidNotificationObject,
                                Apns = new ApnsConfig
                                {
                                    Aps = new Aps
                                    {
                                        //ContentAvailable = true,
                                        CustomData = iosNotificationObject
                                    },
                                    //Headers = new Dictionary<string, string>
                                    //{
                                    //     { "apns-push-type", "alert" },
                                    //     { "apns-priority", "10" }
                                    //}
                                }

                            }).ToList();

                            var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(messageList);

                            foreach (var result in response.Responses)
                            {
                                if (result.IsSuccess)
                                {
                                    successCount++;
                                }
                                else
                                {
                                    failureCount++;
                                }
                            }
                        }
                        // Send one notification per device token
                        //foreach (var deviceToken in userList)
                        //{
                        //    try
                        //    {
                        //        var userItem = new List<Message>();
                        //        var message = new Message
                        //        {
                        //            Token = deviceToken,
                        //            Notification = new Notification
                        //            {
                        //                Title = title,
                        //                Body = body,
                        //            },
                        //            Data = androidNotificationObject,
                        //            Apns = new ApnsConfig
                        //            {
                        //                Aps = new Aps
                        //                {
                        //                    CustomData = iosNotificationObject
                        //                }
                        //            }
                        //        };
                        //        userItem.Add(message);


                        //        // Send notification
                        //        var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(userItem);

                        //        Console.WriteLine($"Message sent to {deviceToken}: {response}");
                        //        successCount++;
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Console.WriteLine($"Failed to send message to {deviceToken}: {ex.Message}");
                        //        failureCount++;
                        //    }
                    //}

                        await App.Current.MainPage.DisplayAlert("Erfolg", $"Erfolgreich gesendet: {successCount}", "OK");
                    }
                    else
                    {
                        Console.WriteLine("Error: Unable to fetch user list.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        else 
        { 
            Console.WriteLine("Notification sending canceled by the user."); 
        }

    
    }

}