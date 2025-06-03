namespace NZWalks.UI.Models.DTO
{
    public class RegisterResponseDto
    {
        // This class is used to deserialize the response from the API when a user registers
        // The API returns a JSON object with the following properties
        //public List<string> Roles { get; set; }
        public string Roles { get; set; }
        public string Message { get; set; }
    }
}

//DTO (Data Transfer Object):
//DTOs are specifically designed for transferring data between different layers of an application or between different applications (e.g., between your API and UI).
//They are used to shape the data that is sent across network boundaries.
//In your scenario, RegisterResponse is used to transfer data from your API to your UI, making it a perfect fit for a DTO.