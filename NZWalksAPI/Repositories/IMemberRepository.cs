using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetAllAsync();
        Task<Member?> GetMemberByIdAsync(Guid id); //Member can be null
        Task<Member> CreateMemberAsync(Member member);
        Task<Member?> UpdateMemberAsync(Guid id, Member member);
        Task<Member?> DeleteMemberAsync(Guid id); // it is nullable since it will try to return the id,
                                                  // if it can it will return it
                                                  // otherwise it will return null
        Task<Member?> GetMemberByIdentityUserIdAsync(string identityUserId);
    }
}
