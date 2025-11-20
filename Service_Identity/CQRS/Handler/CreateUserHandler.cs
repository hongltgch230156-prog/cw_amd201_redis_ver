using Service_Identity.Data;
using Service_Identity.CQRS.Commands;
using Service_Identity.Models;

namespace Service_Identity.CQRS.Handler
{
    public class CreateUserHandler
    {
        private readonly ApplicationDbContext _context;

        public CreateUserHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Handle(CreateUserCommand command)
        {
            var user = new User
            {
                FullName = command.FullName,
                Email = command.Email,
                DateOfBirth = command.DateOfBirth,
                FirebaseUid = command.FirebaseUid
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}