
using System.Net;


namespace GCloud.Shared.Exceptions
{
    public abstract class BaseGustavException : Exception, IGustavException
    {
        public ExceptionStatusCode ErrorCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public string HumanReadableMessage { get; set; }

        /// <summary>
        /// Erstellt eine Klasse mit der der Error Handler umgehen kann.
        /// </summary>
        /// <param name="errorCode">Der interne ErrorCode für diesen Fehlzustand</param>
        /// <param name="httpStatusCode">Der Rückgabewert als ErrorCode</param>
        /// <param name="humanReadableMessage">Die Nachricht welche den Fehlerzustand genauer beschreibt</param>
        protected BaseGustavException(ExceptionStatusCode errorCode, HttpStatusCode httpStatusCode, string humanReadableMessage)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
            HumanReadableMessage = humanReadableMessage;
        }

        /// <summary>
        /// Erstellt eine Klasse mit der der Error Handler umgehen kann.
        /// </summary>
        /// <param name="errorCode">Der interne ErrorCode für diesen Fehlzustand</param>
        /// <param name="humanReadableMessage">Die Nachricht welche den Fehlerzustand genauer beschreibt</param>
        protected BaseGustavException(ExceptionStatusCode errorCode, string humanReadableMessage) : this(errorCode, HttpStatusCode.BadRequest, humanReadableMessage)
        {
        }

        protected BaseGustavException(string message, ExceptionStatusCode errorCode, string humanReadableMessage) : base(message)
        {
            ErrorCode = errorCode;
            HumanReadableMessage = humanReadableMessage;
        }

        protected BaseGustavException(string message, ExceptionStatusCode errorCode, HttpStatusCode httpStatusCode, string humanReadableMessage) : this(message, errorCode,humanReadableMessage)
        {
            HttpStatusCode = httpStatusCode;
        }

        protected BaseGustavException(string message, Exception innerException, ExceptionStatusCode errorCode, string humanReadableMessage) : base(message, innerException)
        {
            ErrorCode = errorCode;
            HumanReadableMessage = humanReadableMessage;
        }

        protected BaseGustavException(string message, Exception innerException, ExceptionStatusCode errorCode, HttpStatusCode httpStatusCode, string humanReadableMessage) : this(message, innerException,errorCode,humanReadableMessage)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}
