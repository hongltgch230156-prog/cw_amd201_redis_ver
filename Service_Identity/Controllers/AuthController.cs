using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Service_Identity.CQRS.Commands;
using Service_Identity.CQRS.Handler;
using Service_Identity.CQRS.Queries;

namespace Service_Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GetUserByFirebaseUidHandler _getUserHandler;
        private readonly CreateUserHandler _createUserHandler;

        public AuthController(GetUserByFirebaseUidHandler getHandler, CreateUserHandler createHandler)
        {
            _getUserHandler = getHandler;
            _createUserHandler = createHandler;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { Message = "Missing or invalid token" });

            var idToken = authHeader.Substring("Bearer ".Length);

            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email")
                               ? decodedToken.Claims["email"].ToString()!
                               : "No email";

                var existingUser = await _getUserHandler.Handle(new GetUserByFirebaseUidQuery(uid));

                if (existingUser == null)
                {
                    existingUser = await _createUserHandler.Handle(
                        new CreateUserCommand("Unknown", email, DateTime.MinValue, uid)
                    );
                }

                return Ok(new
                {
                    Message = "Login successful",
                    Uid = uid,
                    Email = email,
                    UserId = existingUser.Id
                });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { Message = "Invalid Firebase token", Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Server error", Error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { Message = "Email and Password are required" });

            try
            {
                var userRecordArgs = new UserRecordArgs
                {
                    Email = request.Email,
                    Password = request.Password,
                    DisplayName = request.FullName
                };

                UserRecord firebaseUser = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);

                var newUser = await _createUserHandler.Handle(
                    new CreateUserCommand(request.FullName, request.Email, request.DateOfBirth, firebaseUser.Uid)
                );

                return Ok(new
                {
                    Message = "Register successful",
                    UserId = newUser.Id,
                    Uid = firebaseUser.Uid,
                    Email = newUser.Email
                });
            }
            catch (FirebaseAuthException ex)
            {
                return BadRequest(new { Message = "Firebase error", Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Server error", Error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { Message = "Logout successful" });
        }
    }

    // DTO cho Register
    public class RegisterRequest
    {
        public string FullName { get; set; } = "Unknown";
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } = DateTime.MinValue;
    }
}