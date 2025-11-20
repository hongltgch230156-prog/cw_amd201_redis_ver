
using System;
namespace Service_Identity.CQRS.Commands
{
    public record CreateUserCommand(string FullName, string Email, DateTime DateOfBirth, string? FirebaseUid);
}