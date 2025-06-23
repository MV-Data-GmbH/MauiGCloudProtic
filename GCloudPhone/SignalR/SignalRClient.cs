using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.ComponentModel;

public class SignalRClient : INotifyPropertyChanged
{
    private Color _indicatorColor;

    public Color IndicatorColor
    {
        get => _indicatorColor;
        set
        {
            _indicatorColor = value;
            OnPropertyChanged(nameof(IndicatorColor));
        }
    }
    public List<string> OnlineUsers { get; private set; }
    private readonly ConcurrentQueue<string> lstMessages = new ConcurrentQueue<string>();
    private HubConnection _hubConnection;
    private readonly string _username;
    private readonly string _password;
    private readonly string _url;

    // Events
    public event Action<SignalRClient, string> MessageReceived;
    public event Action<SignalRClient, string> UserOnline;
    public event Action<SignalRClient, string> UserOffline;
    public event Action<SignalRClient, List<string>> OnlineUsersReceived;
    private Frame _connectionStatusIndicator;
    public SignalRClient(string username, string password, string url)
    {
        _username = username;
        _password = password;
        _url = url;
        OnlineUsers = new List<string>();
        StartMessageProcessing();
        ConnectToHub();
    }

    private async void ConnectToHub()
    {
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));



        _hubConnection = new HubConnectionBuilder()

            .WithUrl(_url, options =>

            {

                options.AccessTokenProvider = () => Task.FromResult(base64Credentials);

            })

            .WithAutomaticReconnect()

            .Build();
        //_hubConnection.ServerTimeout = TimeSpan.FromSeconds(10);
        //_hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(5);

        _hubConnection.On<string>("MessageReceived", message =>
        {
            lstMessages.Enqueue(message);
            MessageReceived?.Invoke(this, message);
        });

        _hubConnection.On<string, string>("MessageReceivedV2", (senderConnectionId, message) =>
        {
            lstMessages.Enqueue(message);
            MessageReceived?.Invoke(this, message);
        });

        _hubConnection.On<string>("UserOnline", userName =>
        {
            UserOnline?.Invoke(this, userName);
        });

        _hubConnection.On<string>("UserOffline", userName =>
        {
            UserOffline?.Invoke(this, userName);
        });

        _hubConnection.On<List<string>>("ReceiveOnlineUsers", users =>
        {
            OnlineUsers = users;
            OnlineUsersReceived?.Invoke(this, users);
        });

        _hubConnection.Closed += async (exception) =>
        {
            await UpdateConnectionStatusColor(HubConnectionState.Disconnected);
        };

        _hubConnection.Reconnecting += async (exception) =>
        {
           await UpdateConnectionStatusColor(HubConnectionState.Reconnecting);
        };

        _hubConnection.Reconnected += async (connectionId) =>
        {
            try
            {
                await UpdateConnectionStatusColor(HubConnectionState.Connected);

                Console.WriteLine("Reconnected successfully. Fetching online users...");

                //var onlineUsers = await _hubConnection.InvokeAsync<List<string>>("GetOnlineUsers");

                //OnlineUsers = onlineUsers;
                //OnlineUsersReceived?.Invoke(this, onlineUsers);
                //OnOnlineUsersReceived(this, onlineUsers);
                await _hubConnection.InvokeAsync("GetOnlineUsers");

                Console.WriteLine("Online users updated after reconnection.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching online users after reconnection: {ex.Message}");
            }
        };
        // Start the connection
        try
        {
            await _hubConnection.StartAsync();
            await UpdateConnectionStatusColor(HubConnectionState.Connected);
            await _hubConnection.InvokeAsync("GetOnlineUsers");
            //MonitorNetworkConnectivity();
            //await MonitorConnectionStatus();
        }
        catch (Exception ex)
        {
            await UpdateConnectionStatusColor(HubConnectionState.Disconnected);
            Console.WriteLine($"Error connecting to hub: {ex.Message}");
        }
    }

    public async void SendMessage(string msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            try
            {
                await _hubConnection.InvokeAsync("SendMessage", msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("No message to send.");
        }
    }

    public async Task<bool> SendMessageToUser(string usern, string msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            try
            {
                await _hubConnection.InvokeAsync("GetOnlineUsers");
                if (OnlineUsers.Contains(usern))
                {
                    await _hubConnection.InvokeAsync("SendMessageToUser", usern, msg);
                    return true;
                }
                else
                {
                    Console.WriteLine($"User not online: {usern}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to user: {ex.Message}");
                return false;
            }
        }
        {
            return false;
        }
    }

    public async void SendMessageToServer(string msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            try
            {
                await _hubConnection.InvokeAsync("SendMessageToServer", msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }

    private void StartMessageProcessing()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                if (lstMessages.TryDequeue(out var message))
                {
                    // Process the message here
                    //SignalRApp.HandleReceivedMessage(message);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        });
    }

    public void OnMessageReceived(SignalRClient sender, string message)
    {
       // SignalRApp.HandleReceivedMessage(message);
    }

    public void OnOnlineUsersReceived(SignalRClient sender, List<string> users)
    {
        sender.OnlineUsers = users;
    }

    public void OnUserOnline(SignalRClient sender, string user)
    {
        if (sender.OnlineUsers.Contains(user))
        {
            return;
        }
        sender.OnlineUsers.Add(user);
    }

    public void OnUserOffline(SignalRClient sender, string user)
    {
        if (sender.OnlineUsers.Contains(user))
        {
            sender.OnlineUsers.Remove(user);
        }
    }
    public bool IsConnected
    {
        get
        {
            return _hubConnection?.State == HubConnectionState.Connected;
        }
    }
    private async Task UpdateConnectionStatusColor(HubConnectionState state)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            switch (state)
            {
                case HubConnectionState.Connected:
                    IndicatorColor = Colors.Green;
                    break;
                case HubConnectionState.Connecting:
                case HubConnectionState.Reconnecting:
                    IndicatorColor = Colors.Yellow;
                    break;
                case HubConnectionState.Disconnected:
                default:
                    IndicatorColor = Colors.Red;
                    break;
            }
        });
    }
 
    private void MonitorNetworkConnectivity()
    {
        Connectivity.ConnectivityChanged += async (sender, args) =>
        {
            if (args.NetworkAccess != NetworkAccess.Internet)
            {
                // Keine Internetverbindung vorhanden
                Console.WriteLine("Network connection lost. Stopping SignalR connection.");
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.StopAsync();
                }
            }
            else
            {
                // Internetverbindung wiederhergestellt
                Console.WriteLine("Network connection restored. Starting SignalR connection.");
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    try
                    {
                        await _hubConnection.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error restarting SignalR connection: " + ex.Message);
                    }
                }
            }
        };
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


}
