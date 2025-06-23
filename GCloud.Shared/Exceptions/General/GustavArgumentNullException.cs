
namespace GCloud.Shared.Exceptions.General
{
    public class GustavArgumentNullException : GustavArgumentException
    {
        public GustavArgumentNullException(string propertyName) : base(propertyName, null)
        {
            HumanReadableMessage = $"\"{propertyName.ToUpper()}\" darf nicht NULL sein.";
        }
    }
}
