using System.Net;

namespace GCloud.Shared.Exceptions
{
    public interface IGustavException /*: /*_Exception*/
    {
        ExceptionStatusCode ErrorCode { get; set; }
        HttpStatusCode HttpStatusCode { get; set; }
        string HumanReadableMessage { get; set; }
    }
}

