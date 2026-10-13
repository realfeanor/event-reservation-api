namespace Application.Features.Events.Constants;

public static class EventsOperationClaims
{
    private const string Section = "Events";
    public const string Admin = $"{Section}.Admin";
    public const string Write = $"{Section}.Write";
    public const string Create = $"{Section}.Create";
    public const string Update = $"{Section}.Update";
}
