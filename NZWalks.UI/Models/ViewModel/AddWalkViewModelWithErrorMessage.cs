namespace NZWalks.UI.Models.ViewModel
{
    public class AddWalkViewModelWithErrorMessage
    {
        public AddWalkViewModel WalkViewModel { get; set; } // this property will hold the AddWalkViewModel object
        public string? ErrorMessage { get; set; }        // this property will hold the error message
    }
}
