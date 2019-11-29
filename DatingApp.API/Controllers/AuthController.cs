using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Data;
using System.Threading.Tasks;
using DatingApp.API.Models;
using DatingApp.API.DTOS;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository authRepo, IConfiguration config)
        {
            _authRepo = authRepo;
            _config = config;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register(UserForRegisterDTO userForRegisterDTO)
        {
                userForRegisterDTO.Username = userForRegisterDTO.Username.ToLower();

                if(await _authRepo.UserExists(userForRegisterDTO.Username))
                return BadRequest("Username already exists");

                var userToCreate = new User
                {
                    Username = userForRegisterDTO.Username
                };

                var createdUser = await _authRepo.Register(userToCreate, userForRegisterDTO.Password);

                return StatusCode(201);


        }

        [HttpPost("login")]
        public async Task<IActionResult> Login (UserForLoginDTO userForLoginDTO)
        {
             var userFromRepo = await _authRepo.Login(userForLoginDTO.Username.ToLower(), userForLoginDTO.Password);

            if(userFromRepo == null)
                return Unauthorized();

             var claims = new []
             {
                 new Claim (ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                 new Claim (ClaimTypes.Name, userFromRepo.Username)
             };

            var keys = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(keys, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                    token = tokenHandler.WriteToken(token)
            });

        }

    }
}