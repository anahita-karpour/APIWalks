using NZWalks.API.Models.DTO;

namespace NZWalks.UI.Services.Members
{
    public interface IMemberService
    {
        // Method to get all members for the list
        Task<IEnumerable<MemberDto>> GetAllMembersAsync();

        // Method to get a single member by ID (for pre-populating the form)
        Task<MemberDto?> GetMemberByIdAsync(Guid id);// the ? means it can be null, so if the member is not found, it will return null instead of throwing an exception

        // Method to update a member
        Task<bool> UpdateMemberAsync(Guid id, UpdateMemberRequestDto memberUpdateRequestDto);

        //Task<MemberDto> AddMemberAsync(AddMemberRequestDto addMemberRequestDto);

        // Method to delete a member
        Task<bool> DeleteMemberAsync(Guid id);
    }
}
