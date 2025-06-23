using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloudPhone.Models
{
    public class MessageEnvelope
    {
        public string MessageType { get; set; }
        public string Data { get; set; }
    }
    public class InfoMessage
    {
        public string Message { get; set; }
    }
}
