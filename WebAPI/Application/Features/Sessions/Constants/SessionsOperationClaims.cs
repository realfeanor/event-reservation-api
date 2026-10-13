namespace Application.Features.Sessions.Constants;

public static class SessionsOperationClaims
{
    private const string Section = "Sessions";
    public const string Admin = $"{Section}.Admin";
    public const string Write = $"{Section}.Write";
    public const string Create = $"{Section}.Create";
    public const string Update = $"{Section}.Update";
}
