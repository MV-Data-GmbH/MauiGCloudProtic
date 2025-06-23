namespace GCloud.Shared.Exceptions
{
    public class ExceptionHandlerResult
    {
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public ExceptionStatusCode ErrorCode { get; set; }
    }
}