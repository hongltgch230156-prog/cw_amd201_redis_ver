using Service_Identity.Data;
using Service_Identity.Models;
using Service_Identity.CQRS.Queries;
using Microsoft.EntityFrameworkCore;

namespace Service_Identity.CQRS.Handler
{
    public class GetUserByFirebaseUidHandler
    {
        private readonly ApplicationDbContext _context;

        public GetUserByFirebaseUidHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> Handle(GetUserByFirebaseUidQuery query)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == query.FirebaseUid);
        }
    }
}