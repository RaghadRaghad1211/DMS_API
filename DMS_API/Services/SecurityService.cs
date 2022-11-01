//using Microsoft.IdentityModel.Tokens;
using DMS_API.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DMS_API.Services
{
    /// <summary>
    /// Class for set Database Connection 
    /// </summary>
    public static class SecurityService
    {
        #region Properteis
        public static readonly string ConnectionString =
            "Server=10.55.101.20,1433;Database=DMS_DB;Integrated Security=false;User ID=dms; Password=dms;Connection Timeout=60";

        public static readonly string PasswordSaltKey =
            "MSSH24919831610";

        public static readonly string JwtKey =
            "Qig5PmxqgqBbVDtVYRorpC55wm8w3ZrL";
        public static readonly string JwtIssuer =
            "APIsecurity";
        public static readonly string JwtAudience =
            "APIsecurity";
        #endregion

        public static JwtToken GeneratTokenAuthenticate(UserModel User_M)
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
                Expires = DateTime.UtcNow.AddHours(3).AddHours(8),
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserInfo", JsonConvert.SerializeObject(User_M)),
                    new Claim(ClaimTypes.NameIdentifier,User_M.UserID.ToString()),
                    new Claim("Username", User_M.UserName),
                    //new Claim("Password", User_M.Password),
                    new Claim("FullName", User_M.FullName),
                    new Claim(ClaimTypes.Role,User_M.Role),
                    new Claim("Datetime", DateTime.UtcNow.ToString())
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
                TokenExpairy = TokenExTo,
                UserID = User_M.UserID
            };
            return TokenInfo;
            // return TokenID;
        }

        public static string PasswordEnecrypt(string pass)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(pass);//Encoding.UTF8.GetBytes
            byte[] src = Encoding.UTF8.GetBytes(PasswordSaltKey);
            byte[] dst = new byte[src.Length + bytes.Length];
            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(dst);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
        #region Password    
        //public static string PasswordEnecrypt1(string pass)
        //{
        //    byte[] bytes = Encoding.UTF8.GetBytes(pass);
        //    byte[] src = Encoding.UTF8.GetBytes(PasswordSaltKey);
        //    byte[] dst = new byte[src.Length + bytes.Length];
        //    Buffer.BlockCopy(src, 0, dst, 0, src.Length);
        //    Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
        //    HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
        //    byte[] inArray = algorithm.ComputeHash(dst);
        //    return Convert.ToBase64String(inArray);
        //}

        //public static string PasswordEnecrypt(string pass)
        //{
        //    byte[] bytes = Encoding.UTF8.GetBytes(pass);//Encoding.UTF8.GetBytes
        //    SHA256Managed hashstring = new SHA256Managed();
        //    byte[] hash = hashstring.ComputeHash(bytes);
        //    string hashString = string.Empty;
        //    foreach (byte x in hash)
        //    {
        //        hashString += String.Format("{0:x2}", x);
        //    }
        //    return hashString;
        //}

        #endregion
    }

    public class JwtToken
    {
        public string TokenID { get; set; }
        public DateTime TokenExpairy { get; set; }
        public int UserID { get; set; }
    }
}