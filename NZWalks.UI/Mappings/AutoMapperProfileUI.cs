using AutoMapper;
using NZWalks.API.Models.DTO;
using NZWalks.UI.Models.ViewModel;

namespace NZWalks.UI.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping between UI ViewModels and API request/response DTOs.
    /// </summary>
    public class AutoMapperProfileUI : Profile
    {
        public AutoMapperProfileUI()
        {
            // Maps UI ViewModel to API request DTOs
            //----Region Mappings----
            CreateMap<AddRegionViewModel, AddRegionRequestDto>().ReverseMap();
            CreateMap<UpdateRegionViewModel, UpdateRegionRequestDto>().ReverseMap();

            //----Member Mappings----
            CreateMap<UpdateMemberViewModel, UpdateMemberRequestDto>().ReverseMap();


            // Maps API response DTOs to UI ViewModel(for display/pre-filling forms)
            //----Region Mappings----
            CreateMap<RegionDtoV1, UpdateRegionViewModel>().ReverseMap();
            CreateMap<RegionDtoV2, UpdateRegionViewModel>().ReverseMap();

            CreateMap<RegionDtoV1, AddRegionViewModel>().ReverseMap();
            CreateMap<RegionDtoV2, AddRegionViewModel>().ReverseMap();

            //----Member Mappings----
            CreateMap<MemberDto, UpdateMemberViewModel>().ReverseMap();
        }
    }
}
