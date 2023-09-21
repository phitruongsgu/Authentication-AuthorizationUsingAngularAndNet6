using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetCoreAuthAPI.Context;
using NetCoreAuthAPI.Helpers;
using NetCoreAuthAPI.Models;
using NetCoreAuthAPI.Models.Dto;
using NetCoreAuthAPI.UltilityService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NetCoreAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _authContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public UserController(ApplicationDbContext authContext, IConfiguration configuration, IEmailService emailService)
        {
            _authContext = authContext;
            _configuration = configuration; 
            _emailService = emailService;   
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var user = await _authContext.Users.Where(x => x.Username == model.Username).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound(new { Message = "User not found!" });
            }

            if (!PasswordHasher.VerifyPassword(model.Password, user.Password))
            {
                return BadRequest(new { Message = "Password is incorrect" });
            }

            user.Token = CreateJwt(user);

            //return Ok(new
            //{
            //    Token = user.Token,
            //    Message = "Login success!"
            //});

            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);
            await _authContext.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User model)
        {
            if (model == null)
            {
                return BadRequest();
            }
            // kiểm tra username
            if (await CheckUserNameExistAsync(model.Username))
            {
                return BadRequest(new
                {
                    Message = "Username already exist!"
                });
            }

            // kiểm tra email
            if (await CheckEmailExistAsync(model.Email))
            {
                return BadRequest(new
                {
                    Message = "Email already exist!"
                });
            }
            // kiểm tra độ mạnh password
            var passMessage = CheckPasswordStrength(model.Password);
            if (!string.IsNullOrEmpty(passMessage))
            {
                return BadRequest(new { Message = passMessage.ToString() });
            }

            model.Password = PasswordHasher.HashPassword(model.Password);
            model.Role = "User";
            model.Token = "";

            await _authContext.Users.AddAsync(model);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "Register user success!"
            });
        }

        private async Task<bool> CheckUserNameExistAsync(string username)
        {
            return await _authContext.Users.AnyAsync(x => x.Username == username);
        }

        private async Task<bool> CheckEmailExistAsync(string email)
        {
            return await _authContext.Users.AnyAsync(x => x.Email == email);
        }

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 9)
            {
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            }
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
            {
                sb.Append("Password must be alphanumeric" + Environment.NewLine);
            }
            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
            {
                sb.Append("Password should contain special charcter" + Environment.NewLine);
            }
            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name,$"{user.Username}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                //Expires = DateTime.Now.AddDays(1),
                Expires = DateTime.Now.AddSeconds(30),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _authContext.Users
                .Any(a => a.RefreshToken == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("This is Invalid Token");
            }    
            return principal;

        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
            {
                return BadRequest("Invalid Client Request");
            }
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = GetPrincipleFromExpiredToken(accessToken);

            var username = principal.Identity.Name;
            var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid Request");
            }    
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authContext.SaveChangesAsync();
            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            });
        }

        [HttpPost("send-reset-email/{email}")]
        public async Task<IActionResult> SendEmail(string email)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(a => a.Email == email);
            if (user is null) { 
                return NotFound(new {
                    StatusCode = 404,
                    Message = "Email does not exist!"
                });  
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            user.ResetPasswordToken = emailToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            string from = _configuration["EmailSettings:From"];
            var emailModel = new EmailModel(email, "Reset Password!!!", EmailBody.EmailStringBody(email, emailToken));
            _emailService.SendEmail(emailModel);
            _authContext.Entry(user).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Email sent!"
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            var user = await _authContext.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email);
            if (user is null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User does not exist!"
                });
            }

            var tokenCode = user.ResetPasswordToken;
            DateTime emailTokenExpiry = user.ResetPasswordExpiry;
            if(tokenCode != resetPasswordDto.EmailToken || emailTokenExpiry < DateTime.Now)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid Reset link!"
                });
            }

            user.Password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            _authContext.Entry(user).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Password Reset Successfully!"
            });
        }
    }
}
