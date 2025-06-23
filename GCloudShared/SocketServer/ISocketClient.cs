using GCloudShared.WebShopDto;

namespace GCloudShared.SocketServer
{
    public interface ISocketClient
    {
        public bool SocketConnectionOK
        {
            get;
            set;
        }
        public string SendMessage(string ServerIP, int ServerPort, string MessageText, int SendTO, int RecTO, OrderKuechenDisplay display);
    }
}
