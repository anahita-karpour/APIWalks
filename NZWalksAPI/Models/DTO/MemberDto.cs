namespace NZWalks.API.Models.DTO
{
    public class MemberDto
    {
        // Individual Member Properties that UI project will receive & UI will send UpdateMemberRquestDto to the API
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        // for security reasons, we do not expose the password
    }
}
