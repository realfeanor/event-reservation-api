namespace Application.Features.Venues.Constants;

public static class VenuesOperationClaims
{
    private const string Section = "Venues";

    public const string Admin = $"{Section}.Admin";
    public const string Read = $"{Section}.Read";
    public const string Write = $"{Section}.Write";
    public const string Create = $"{Section}.Create";
    public const string Update = $"{Section}.Update";
}
