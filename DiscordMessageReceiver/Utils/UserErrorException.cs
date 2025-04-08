
namespace DiscordMessageReceiver.Utils;

// Simple exception class meant to detect when callers of the service do not provide
// the correct parameters or otherwise invoke the REST method incorrectly
[Serializable]
internal class UserErrorException : Exception
{
    public UserErrorException()
    {
    }

    public UserErrorException(string message) : base(message)
    {
    }

    public UserErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}