namespace ODK.Core.Events;

public static class EventResponseTypeExtensions
{
    public static string ToString(this EventResponseType responseType, bool forAction = true)
    {
        switch (responseType)
        {
            case EventResponseType.Yes:
                return forAction ? "Yes" : "Going";
            case EventResponseType.Maybe:
                return "Maybe";
            case EventResponseType.No:
                return forAction ? "No" : "Not going";
            default:
                return string.Empty;
        }
    }
}
