using Application.Features.OperationClaims.Commands.Create;
using Application.Features.OperationClaims.Commands.Delete;
using Application.Features.OperationClaims.Commands.Update;
using Application.Features.OperationClaims.Queries.GetById;
using Application.Features.OperationClaims.Queries.GetList;
using AutoMapper;
using Domain.Entities;
using Core.Application.Responses;
using Core.Persistence.Paging;

namespace Application.Features.OperationClaims.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreateOperationClaimCommand, OperationClaim>(MemberList.None);
        CreateMap<OperationClaim, CreatedOperationClaimResponse>();
        CreateMap<UpdateOperationClaimCommand, OperationClaim>(MemberList.None);
        CreateMap<OperationClaim, UpdatedOperationClaimResponse>();
        CreateMap<OperationClaim, DeletedOperationClaimResponse>();
        CreateMap<OperationClaim, GetByIdOperationClaimResponse>();
        CreateMap<OperationClaim, GetListOperationClaimListItemDto>();
        CreateMap<IPaginate<OperationClaim>, GetListResponse<GetListOperationClaimListItemDto>>();
    }
}
