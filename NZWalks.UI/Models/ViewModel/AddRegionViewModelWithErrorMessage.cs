namespace NZWalks.UI.Models.ViewModel
{
    public class AddRegionViewModelWithErrorMessage
    {
        public AddRegionViewModel RegionViewModel { get; set; } // this property will hold the AddRegionViewModel object
        public string? ErrorMessage { get; set; }        // this property will hold the error message
    }
}
