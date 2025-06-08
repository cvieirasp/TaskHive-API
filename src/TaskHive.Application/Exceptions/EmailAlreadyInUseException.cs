namespace TaskHive.Application.Exceptions;

public class EmailAlreadyInUseException(string email) : Exception($"Email '{email}' is already in use.")
{
    public string Email { get; } = email;
}