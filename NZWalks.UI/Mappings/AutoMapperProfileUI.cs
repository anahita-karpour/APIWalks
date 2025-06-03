using AutoMapper;
using NZWalks.UI.Models.DTO;
using NZWalks.UI.Models.ViewModel;

namespace NZWalks.UI.Mappings
{
    public class AutoMapperProfileUI : Profile
    {
        public AutoMapperProfileUI()
        {

            CreateMap<RegionDto, UpdateRegionViewModel>().ReverseMap();
            CreateMap<RegionDto, AddRegionViewModel>().ReverseMap();
        }
    }
}
