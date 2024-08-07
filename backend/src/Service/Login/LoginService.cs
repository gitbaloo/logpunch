using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Shared;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Service.Login
{
    public class LoginService : ILoginService
    {
        private readonly IConfiguration _configuration;
        private readonly LogpunchDbContext _dbContext;

        public LoginService(IConfiguration configuration, LogpunchDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<string> AuthorizeLogin(string email, string password)
        {
            var user = await _dbContext.Users
                .Where(u => u.Email == email && u.Password == password)
                .Select(u => new LogpunchUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString()
                })
                .FirstOrDefaultAsync();

            if (user is not null)
            {
                var token = GenerateJwtToken(user);
                return token;
            }

            throw new ArgumentException("Invalid Username/Password");
        }

        public async Task<LogpunchUserDto> ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var signingCert = _configuration["JWT_KEY"];
            if (string.IsNullOrEmpty(signingCert))
            {
                throw new Exception("Encryption key was null");
            }

            var key = Encoding.ASCII.GetBytes(signingCert);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "nameid").Value);
                var user = await _dbContext.Users.Where(u => u.Id == userId).Select(u => new LogpunchUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString()
                }).FirstOrDefaultAsync();
                return user ?? throw new AuthenticationException("Error validating user");
            }
            catch
            {
                throw new AuthenticationException("Please login again");
            }
        }

        private string GenerateJwtToken(LogpunchUserDto user)
        {
            var jwtSigningCert = _configuration["JWT_KEY"];
            if (string.IsNullOrEmpty(jwtSigningCert))
            {
                throw new InvalidOperationException("JWT Key is missing in the configuration.");
            }
            var signingCert = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningCert));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT_ISSUER"],
                audience: _configuration["JWT_AUDIENCE"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: new SigningCredentials(signingCert, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
