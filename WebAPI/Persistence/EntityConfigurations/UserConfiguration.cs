using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users").HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("Id").IsRequired();
        builder.Property(u => u.FirstName).HasColumnName("FirstName").IsRequired();
        builder.Property(u => u.LastName).HasColumnName("LastName").IsRequired();
        builder.Property(u => u.Email).HasColumnName("Email").IsRequired();
        builder.Property(u => u.Status).HasColumnName("Status").IsRequired();
        builder.Property(u => u.PasswordSalt).HasColumnName("PasswordSalt").IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("PasswordHash").IsRequired();
        builder.Property(u => u.AuthenticatorType).HasColumnName("AuthenticatorType").IsRequired();
        builder.Property(u => u.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(u => u.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(u => u.DeletedDate).HasColumnName("DeletedDate");

        builder.HasQueryFilter(u => !u.DeletedDate.HasValue);

        builder.HasMany(u => u.UserOperationClaims);
        builder.HasMany(u => u.RefreshTokens);
        builder.HasMany(u => u.EmailAuthenticators);
        builder.HasMany(u => u.OtpAuthenticators);

        builder.HasData(_seeds);

        builder.HasBaseType((string)null!);
    }

    public static Guid AdminId { get; } = new("64b33286-cde0-48b2-af2b-ed7a160d6484");
    private IEnumerable<User> _seeds
    {
        get
        {
            User adminUser =
                new()
                {
                    Id = AdminId,
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "narch@kodlama.io",
                    Status = true,
                    PasswordHash = Convert.FromBase64String(
                        "nVG1VKSzgkCt0ggO4fIRzWF8PEYtYH74M4aP/rHenpKool0bPjvcKAu0GpzlCToiJA9CoLrvyBpcRcnpDRaepw=="
                    ),
                    PasswordSalt = Convert.FromBase64String(
                        "YiVlqmVUfMtR2LRGDu2SyKRLibzKIk4Fn540cYyBQ3jagJFAgTJkBzAdElLXXemqdljh+h0R5hMCJZQDd4Cqscqd5yeW6GOGT2DfayrIrgEXaIXyx5iwFzfnfHil9J707Z64ptCHd6nw8cAq3h9Mo/RPoU/wCMpBplHBegIpR/0="
                    )
                };
            yield return adminUser;
        }
    }
}
