namespace VolunteerHQ.Core.Exceptions;

public class ConflictEmailException : Exception
{
    public ConflictEmailException(string message) : base(message){}
}