using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly MemberDbContext _context;

        public UserRepository(MemberDbContext context)
        {
            _context = context;
        }

        public async Task<Option<User>> Find(string userName)
        {
            return await _context.MemberUserAccounts
                .FirstOrDefaultAsync(x => x.UserName == userName);
        }
        
        public async Task<Option<User>> Find(string referenceNumber, string businessGroup)
        {
            return await _context.MemberUserAccounts
                .FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup);
        }
        
        // public async Task<Option<LinkedMember>> FindLinkedMember(string referenceNumber, string businessGroup)
        // {
        //     return await _context.LinkedMembers
        //         .FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup);
        // }
        //
        // public async Task<Option<LinkedMember>> FindLinkedMember(string referenceNumber, string businessGroup, string linkedReferenceNumber, string linkedBusinessGroup)
        // {
        //     return await _context.LinkedMembers
        //         .FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber && 
        //                                   x.BusinessGroup == businessGroup &&
        //                                   x.LinkedReferenceNumber == linkedReferenceNumber &&
        //                                   x.LinkedBusinessGroup == linkedBusinessGroup);
        // }
    }
}