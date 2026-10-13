using Application.Features.UserOperationClaims.Commands.Create;
using Application.Features.UserOperationClaims.Commands.Delete;
using Application.Features.UserOperationClaims.Commands.Update;
using Application.Features.UserOperationClaims.Queries.GetById;
using Application.Features.UserOperationClaims.Queries.GetList;
using AutoMapper;
using Domain.Entities;
using Core.Application.Responses;
using Core.Persistence.Paging;

namespace Application.Features.UserOperationClaims.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreateUserOperationClaimCommand, UserOperationClaim>(MemberList.None);
        CreateMap<UserOperationClaim, CreatedUserOperationClaimResponse>();
        CreateMap<UpdateUserOperationClaimCommand, UserOperationClaim>(MemberList.None);
        CreateMap<UserOperationClaim, UpdatedUserOperationClaimResponse>();
        CreateMap<UserOperationClaim, DeletedUserOperationClaimResponse>();
        CreateMap<UserOperationClaim, GetByIdUserOperationClaimResponse>();
        CreateMap<UserOperationClaim, GetListUserOperationClaimListItemDto>();
        CreateMap<IPaginate<UserOperationClaim>, GetListResponse<GetListUserOperationClaimListItemDto>>();
    }
}
