using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public class SQLMemberRepository : IMemberRepository
    {
        private readonly NZWalksAuthDbContext _authDbContext;
        // Inject UserManager; it will be needed for linking profiles to IdentityUsers
        private readonly UserManager<IdentityUser> _userManager;

        public SQLMemberRepository(
            NZWalksAuthDbContext authDbContext,
            UserManager<IdentityUser> userManager)
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
        }
        public async Task<Member> CreateMemberAsync(Member member)
        {
            // This method should be called *after* IdentityUser is created.
            // It just creates the profile.
            await _authDbContext.Members.AddAsync(member);
            await _authDbContext.SaveChangesAsync();
            return member;
        }

        public async Task<Member?> DeleteMemberAsync(Guid id)
        {
            var existingMember = await _authDbContext.Members.FirstOrDefaultAsync(x => x.Id == id);
            if (existingMember == null)
            {
                return null;
            }
            // IMPORTANT: Delete the associated IdentityUser if you delete the profile
            // Otherwise, you'll have orphaned IdentityUsers.
            var identityUser = await _userManager.FindByIdAsync(existingMember.IdentityUserId);
            if (identityUser != null)
            {
                var deleteResult = await _userManager.DeleteAsync(identityUser);
                if (!deleteResult.Succeeded)
                {
                    // Handle error, maybe log it and throw, or return null/error indicator
                    // This is a critical operation, so failure needs to be robustly handled.
                    throw new InvalidOperationException($"Failed to delete associated IdentityUser for Member ID {id}. Errors: {string.Join("; ", deleteResult.Errors.Select(e => e.Description))}");
                }
            }

            _authDbContext.Members.Remove(existingMember);
            await _authDbContext.SaveChangesAsync();
            return existingMember;
        }

        public async Task<List<Member>> GetAllAsync()
        {
            // This method should return the raw Member profiles.
            // The controller will then enrich them with IdentityUser roles/data if needed.
            return await _authDbContext.Members.ToListAsync();
        }

        public async Task<Member?> GetMemberByIdAsync(Guid id)
        {
            return await _authDbContext.Members.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Member?> GetMemberByIdentityUserIdAsync(string identityUserId)
        {
            return await _authDbContext.Members.FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId);
        }

        public async Task<Member?> UpdateMemberAsync(Guid id, Member member)
        {
            var existingMember = await _authDbContext.Members.FirstOrDefaultAsync(x => x.Id == id);
            if (existingMember == null)
            {
                return null;
            }

            // Update only the properties relevant to the Member profile itself
            existingMember.Email = member.Email;
            existingMember.Role = member.Role; // If Role is denormalized on Member profile


            // IdentityUserId should not be changed via update for an existing profile
            // existingMember.IdentityUserId = member.IdentityUserId; 

            await _authDbContext.SaveChangesAsync();
            return existingMember;
        }
    }
}
