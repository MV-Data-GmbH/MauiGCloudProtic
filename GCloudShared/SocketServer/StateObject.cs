
using System.Text;

namespace GCloudShared.SocketServer
{
    public class StateObject
    {
        public const int BufferSize = 2000000;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
        public System.Net.Sockets.Socket workSocket = null;
    }
}
