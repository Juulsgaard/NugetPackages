using AutoMapper;
using Crud.Tests.Entities;
using Crud.Tests.Models;

namespace Crud.Tests.Mapping;

public class DogMapping : Profile
{
	public DogMapping()
	{
		CreateMap<DogEntity, DogViewModel>();
		CreateMap<DogCreateModel, DogEntity>(MemberList.Source);
		CreateMap<DogUpdateModel, DogEntity>(MemberList.Source);

		CreateMap<OwnerEntity, OwnerEntity>();
		CreateMap<OwnerEntity, OwnerViewModel>();
		CreateMap<OwnerCreateModel, OwnerEntity>(MemberList.Source);
		CreateMap<OwnerUpdateModel, OwnerEntity>(MemberList.Source);
	}
}