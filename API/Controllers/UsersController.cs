using Microsoft.AspNetCore.Mvc;
using DomainModels;
using API.Data;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;
using System.Text;
using Microsoft.CodeAnalysis.Scripting;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly HotelContext _hotelContext;
        private readonly IConfiguration _configuration;
        public UsersController(HotelContext hotelContext, IConfiguration configuration)
		{
			_hotelContext = hotelContext;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (await _hotelContext.Users.AnyAsync(p => p.Email == registerDto.Email))
            {
                return BadRequest("Email already exists");
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                PhoneNr = registerDto.PhoneNr
            };

            _hotelContext.Users.Add(user);
            await _hotelContext.SaveChangesAsync();

            return Ok("Registration successful");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _hotelContext.Users.FirstOrDefaultAsync(p => p.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized("Invalid email or password");
            }

            var token = GenerateJwtToken(user);

            return Ok(new { Token = token, UserId = user.UserId, IsAdmin = user.Administrator });
        }

        // Get all users.
        [HttpGet("all")]
        // [Authorize(Roles = "Administrator")] // Temporarily comment this out for testing
        public async Task<ActionResult<IEnumerable<User>>> GetAllProfiles()
        {
            try
            {
                return await _hotelContext.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while fetching users");
            }
        }

        // Get a specific user from the UserID.
        [HttpGet("{id}")]
		public async Task<ActionResult<User>> GetAUserById(int id)
		{
			// Finds a user through the user id.
			var user = await _hotelContext.Users.FindAsync(id);

			if (user == null)
			{
				return NotFound();
			}

            return user;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Administrator ? "Administrator" : "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // Update a specific guest user account.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (id != updatedUser.UserId)
            {
                return BadRequest();
            }

            var user = await _hotelContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.PhoneNr = updatedUser.PhoneNr;

            try
            {
                await _hotelContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool UserExists(int id)
        {
            // Gå igennem alle profiler i databasen og tjek, om der findes en med det givne id
            foreach (var user in _hotelContext.Users)
            {
                if (user.UserId == id)
                {
                    return true; // Profilen findes
                }
            }
            return false; // Profilen findes ikke
        }

        [HttpPut("admin/{id}")]
        //[Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AdminUpdateUser(int id, [FromBody] User updatedUser)
        {
            try
            {
                if (id != updatedUser.UserId)
                {
                    return BadRequest("ID mismatch");
                }

                var user = await _hotelContext.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                user.FullName = updatedUser.FullName;
                user.Email = updatedUser.Email;
                user.PhoneNr = updatedUser.PhoneNr;
                user.Administrator = updatedUser.Administrator;

                await _hotelContext.SaveChangesAsync();
                return Ok("User updated successfully");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound($"User with ID {id} no longer exists");
                }
                else
                {
                    return StatusCode(500, "A concurrency error occurred while updating the user");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the user: {ex.Message}");
            }
        }

        // Delete a User.
        [HttpDelete("{id}")]
		public async Task<IActionResult> UserDelete(int id)
		{
			// Find user by user id.
			var user = await _hotelContext.Users.FindAsync(id);

			if (user == null)
			{
				return NotFound();
			}

			// Removes user from Database.
			_hotelContext.Users.Remove(user);
			// Saves changes.
			await _hotelContext.SaveChangesAsync();
			return StatusCode(200);
		}
    }
}

