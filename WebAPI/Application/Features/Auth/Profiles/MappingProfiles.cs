using Application.Features.Auth.Commands.RevokeToken;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Auth.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Core.Security.Entities.RefreshToken<Guid, Guid>, RefreshToken>()
            .ForMember(destination => destination.User, options => options.Ignore());
        CreateMap<RefreshToken, RevokedTokenResponse>();
    }
}
