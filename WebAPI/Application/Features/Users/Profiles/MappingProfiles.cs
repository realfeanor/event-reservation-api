using Application.Features.Users.Commands.Create;
using Application.Features.Users.Commands.Delete;
using Application.Features.Users.Commands.Update;
using Application.Features.Users.Commands.UpdateFromAuth;
using Application.Features.Users.Queries.GetById;
using Application.Features.Users.Queries.GetList;
using AutoMapper;
using Domain.Entities;
using Core.Application.Responses;
using Core.Persistence.Paging;

namespace Application.Features.Users.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreateUserCommand, User>(MemberList.None);
        CreateMap<User, CreatedUserResponse>();
        CreateMap<UpdateUserCommand, User>(MemberList.None);
        CreateMap<User, UpdatedUserResponse>();
        CreateMap<UpdateUserFromAuthCommand, User>(MemberList.None);
        CreateMap<User, UpdatedUserFromAuthResponse>()
            .ForMember(destination => destination.AccessToken, options => options.Ignore());
        CreateMap<User, DeletedUserResponse>();
        CreateMap<User, GetByIdUserResponse>();
        CreateMap<User, GetListUserListItemDto>();
        CreateMap<IPaginate<User>, GetListResponse<GetListUserListItemDto>>();
    }
}
