

namespace GCloudShared.SocketServer
{
    public class NetworkConfiguration
    {
        private static string ip = "192.168.5.105";
        private static int iport = Preferences.Get("Port", 11000);
        private static ISocketClient socketClient = new SocketClient();
        internal static bool Status { get; set; }
        public static bool StatusInfo { get => Status; }

        public static SocketClient SocketClient { get => socketClient as SocketClient; }
        public static string Iad { get => Preferences.Get("IPAdresse", ip); }
        public static int Iport { get => iport; }
    }
}
