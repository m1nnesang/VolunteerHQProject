namespace VolunteerHQ.Core.Exceptions;

public class NotEnoughRightsException : Exception
{
    public NotEnoughRightsException(string message) : base(message) {}
}