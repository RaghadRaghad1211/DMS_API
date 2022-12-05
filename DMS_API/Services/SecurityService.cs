using DMS_API.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;

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
        private static readonly string Url =
            "http://109.224.32.124:8003/api/client/mobile_message/send/basic";
        private static readonly string Username =
            "21";
        private static readonly string Password =
            "T8rJnTtmVctNmqZRZOdBFBluu52Ozj6ubd8v28Ni";
        #endregion

        #region Functions
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
        public static bool SendEmail(string EmailTo, string Subject, string Body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("haelrox1989@gmail.com", "NDC-DMS");
                    mail.To.Add(EmailTo);
                    mail.Subject = Subject;
                    mail.Body = Body;
                    mail.IsBodyHtml = true;
                    //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)) // SmtpHost = "smtp.gmail.com"; PortNumber = 587 for Google
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential("haelrox1989@gmail.com", "rfguirfrmutcmbyp");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static async Task<bool> SendOTP(string PhoneNumber, string Message, bool Reliable = false)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"))}");
                var request = new Dictionary<string, string>
            {
                {"content", Message},
                {"phone_num", PhoneNumber},
                {"reliable", Convert.ToInt16(Reliable).ToString()}
            };
                var response = await client.PostAsync(Url, new FormUrlEncodedContent(request));
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject(content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
        public static string RoundomPassword()
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            int length = 8;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();

        }
        #endregion

        #region Test Password    
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