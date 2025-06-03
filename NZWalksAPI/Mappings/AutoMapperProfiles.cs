using AutoMapper;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;

namespace NZWalks.API.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            //mapping can be created using the method CreateMap
            //AutoMapper also allows for reverse mapping using the method ReverseMap()
            //property name between the two class need to match for that case we need to do the following
            //use ForMember(x => x.Name, opt => opt.MapFrom(source: x => x.FullName)

            // Region
            CreateMap<Region, RegionDtoV1>().ReverseMap();
            CreateMap<Region, RegionDtoV2>().ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Name));
            CreateMap<AddRegionRequestDTO, Region>().ReverseMap();
            CreateMap<UpdateRegionRequestDto, Region>().ReverseMap();

            // Walk
            CreateMap<Walk, WalkDto>().ReverseMap();
            CreateMap<AddWalkRequestDto, Walk>().ReverseMap();
            CreateMap<UpdateWalkRequestDto, Walk>().ReverseMap();

            // Difficulty
            CreateMap<Difficulty, DifficultyDto>().ReverseMap();

            // Image
            CreateMap<Image, ImageUploadResponseDto>().ReverseMap();
            CreateMap<Image, ImageUploadRequestDto>().ReverseMap();

        }
    }
}
