//using Microsoft.IdentityModel.Tokens;
using DMS_API.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DMS_API.Services
{
    /// <summary>
    /// Class for set Database Connection 
    /// </summary>
    public static class SecurityService
    {
        public static readonly string ConnectionString =
            "Server=10.55.101.20,1433;Database=DMS_DB;Integrated Security=false;User ID=dms; Password=dms;Connection Timeout=60";

        public static readonly string PasswordEncryptionKey =
            "MSSH24919831610";

        public static readonly string JwtKey =
            "Qig5PmxqgqBbVDtVYRorpC55wm8w3ZrL";
        public static readonly string JwtIssuer =
            "APIsecurity";
        public static readonly string JwtAudience =
            "APIsecurity";

        public static JwtToken GeneratTokenAuthenticate(LoginModel UserLogin)
        {
            var JwtKeyByte = Encoding.ASCII.GetBytes(JwtKey); // Convert.FromBase64String(JwtKey);
            var SecurityKey = new SymmetricSecurityKey(JwtKeyByte);
            var SecurityAlgorithm = SecurityAlgorithms.HmacSha256Signature;
            var TokenHandler = new JwtSecurityTokenHandler();
            var TokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = JwtIssuer,
                Audience = JwtAudience,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(3).AddDays(30),
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserLogin", JsonConvert.SerializeObject(UserLogin)),
                    new Claim("Name", UserLogin.username),
                    new Claim("Pass", UserLogin.password),
                    new Claim("Role", UserLogin.role),
                    new Claim("id", "101"),
                    new Claim(ClaimTypes.Role,UserLogin.role),
                    new Claim(ClaimTypes.Name,UserLogin.username),
                    new Claim(ClaimTypes.NameIdentifier,"101")
                }),

                SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithm)
            };
            var Token = TokenHandler.CreateToken(TokenDescriptor);
            var TokenID = TokenHandler.WriteToken(Token);
            var TokenExFrom = Token.ValidFrom;
            var TokenExTo = Token.ValidTo;
            JwtToken TokenInfo = new()
            {
                TokenID = TokenID,
                TokenExpireFrom = TokenExFrom,
                TokenExpireTo = TokenExTo
            };
            return TokenInfo;
            // return TokenID;
        }
    }

    public class JwtToken
    {
        public string TokenID { get; set; }
        public DateTime TokenExpireFrom { get; set; }
        public DateTime TokenExpireTo { get; set; }
    }
}