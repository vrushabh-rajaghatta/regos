namespace RegOS.Platform.Domain.ValueObjects;

public static class PasswordErrors
{
    public const string Required = "Password is required.";

    public static readonly string TooShort =
        $"Password must be at least {Password.MinimumLength} characters.";

    public static readonly string TooLong =
        $"Password must be at most {Password.MaximumLength} characters.";
}
