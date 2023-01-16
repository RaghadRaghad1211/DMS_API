using DMS_API.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;
using ArchiveAPI.Services;
using System.Data;
using System.Runtime.CompilerServices;

namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Security
    /// </summary>
    public static class SecurityService
    {
        #region Properteis
        public static readonly string ConnectionString =
        "Server=10.55.101.20,1433;Database=DMS_DB;Integrated Security=false;User ID=dms; Password=dms;Connection Timeout=60;"; // السيرفر
                                                                                                                               // "Server=HAEL\\SQL2022;Database=DMS_DB;Integrated Security=false;User ID=dms; Password=dms;"; // البيت
                                                                                                                               // "Server=NDC-8RW6WC3\\SQL2014;Database=DMS_DB;Integrated Security=false;User ID=dms; Password=dms;"; // الدائرة


        public static readonly string HostFilesUrl =
            "http://10.55.101.10:90/DMSserver";  // السيرفر
                                                 //  "http://192.168.43.39:90/DMSserver"; //  البيت
                                                 //  "http://10.92.92.239:90/DMSserver"; // الدائرة

        private static string PasswordSalt;
        private static string CrypticSalt;
        private static string DocumentSalt;

        private static string JwtKey;
        private static string JwtIssuer;
        private static string JwtAudience;

        private static string MsgEmail;
        private static string MsgPassword;

        private static string OtpUrl;
        private static string OtpUsername;
        private static string OtpPassword;
        #endregion

        #region Functions
        private static bool GetSecureKeys()
        {
            try
            {
                DataAccessService dam = new DataAccessService(ConnectionString);
                DataTable dtKeys = new DataTable();
                dtKeys = dam.FireDataTable($"SELECT SecKey, SecValue  FROM [Security].[SecureKeys]");
                PasswordSalt = dtKeys.Select("SecKey = 'PasswordSalt' ")[0]["SecValue"].ToString();
                CrypticSalt = dtKeys.Select("SecKey = 'CrypticSalt' ")[0]["SecValue"].ToString();
                DocumentSalt = dtKeys.Select("SecKey = 'DocumentSalt' ")[0]["SecValue"].ToString();
                JwtKey = dtKeys.Select("SecKey = 'JwtKey' ")[0]["SecValue"].ToString();
                JwtIssuer = dtKeys.Select("SecKey = 'JwtIssuer' ")[0]["SecValue"].ToString();
                JwtAudience = dtKeys.Select("SecKey = 'JwtAudience' ")[0]["SecValue"].ToString();
                MsgEmail = dtKeys.Select("SecKey = 'MsgEmail' ")[0]["SecValue"].ToString();
                MsgPassword = dtKeys.Select("SecKey = 'MsgPassword' ")[0]["SecValue"].ToString();
                OtpUrl = dtKeys.Select("SecKey = 'OtpUrl' ")[0]["SecValue"].ToString();
                OtpUsername = dtKeys.Select("SecKey = 'OtpUsername' ")[0]["SecValue"].ToString();
                OtpPassword = dtKeys.Select("SecKey = 'OtpPassword' ")[0]["SecValue"].ToString();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Generate Token for user authenticate,
        /// and return token in string variable.
        /// </summary>
        /// <param name="User_M">Body Parameters</param>
        /// <returns></returns>
        public static JwtToken GeneratTokenAuthenticate(UserModel User_M)
        {
            if (GetSecureKeys() == true)
            {
                var JwtKeyByte = Encoding.ASCII.GetBytes(JwtKey);
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
                    new Claim("FullName", User_M.FullName),
                    new Claim(ClaimTypes.Role,User_M.Role),
                    new Claim("IsOrgAdmin",User_M.IsOrgAdmin.ToString()),
                    new Claim(ClaimTypes.MobilePhone,User_M.PhoneNo),
                    new Claim("CreationDate",User_M.UserCreationDate),
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
            }
            else
            {
                return new JwtToken();
            }
        }
        /// <summary>
        /// Send Email to user by email address
        /// </summary>
        /// <param name="EmailTo"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static bool SendEmail(string EmailTo, string Subject, string Body)
        {
            try
            {
                if (GetSecureKeys() == true)
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(MsgEmail, "NDC-DMS");
                        mail.To.Add(EmailTo);
                        mail.Subject = Subject;
                        mail.Body = Body;
                        mail.IsBodyHtml = true;
                        using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential(MsgEmail, MsgPassword);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Send one time password (OTP) to user by phone number.
        /// </summary>
        /// <param name="PhoneNumber"></param>
        /// <param name="Message"></param>
        /// <param name="Reliable">defult false</param>
        /// <returns></returns>
        public static async Task<bool> SendOTP(string PhoneNumber, string Message, bool Reliable = false)
        {
            try
            {
                if (GetSecureKeys() == true)
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization",
                                                    $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{OtpUsername}:{OtpPassword}"))}");
                    var request = new Dictionary<string, string>
                                    {
                                        {"content", Message},
                                        {"phone_num", PhoneNumber},
                                        {"reliable", Convert.ToInt16(Reliable).ToString()}
                                    };
                    var response = await client.PostAsync(OtpUrl, new FormUrlEncodedContent(request));
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject(content);
                    return true;
                }
                else { return false; }
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Enecrypt Password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string PasswordEnecrypt(string password, string username)
        {
            if (GetSecureKeys() == true)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] src = Encoding.UTF8.GetBytes(PasswordSalt + username);
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
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Enecrypt text
        /// </summary>
        /// <param name="text"> the string want encryption</param>
        /// <returns></returns>
        public static string EnecryptText(string text)
        {
            if (GetSecureKeys() == true)
            {
                byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(text);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(CrypticSalt);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
                byte[] encryptedBytes = null;
                byte[] saltBytes = new byte[] { 2, 1, 1, 2, 1, 9, 8, 9 };
                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);
                        AES.Mode = CipherMode.CBC;
                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }
                return Convert.ToHexString(encryptedBytes).Substring(0, GlobalService.LengthKey);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Decrypt text encrypted
        /// </summary>
        /// <param name="TextEncrypted">the string want decryption</param>
        /// <returns></returns>
        public static string DecryptText(string TextEncrypted)
        {
            if (GetSecureKeys() == true)
            {
                byte[] bytesToBeDecrypted = Convert.FromBase64String(TextEncrypted);
                byte[] passwordBytesdecrypt = Encoding.UTF8.GetBytes(CrypticSalt);
                passwordBytesdecrypt = SHA256.Create().ComputeHash(passwordBytesdecrypt);
                byte[] decryptedBytes = null;
                byte[] saltBytes = new byte[] { 2, 1, 1, 2, 1, 9, 8, 9 };
                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytesdecrypt, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);
                        AES.Mode = CipherMode.CBC;
                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }
                string decryptedResult = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedResult;
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// Roundom Password
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RoundomPassword(int length = 10)
        {
            const string valid = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ123456789!@#$%^&*?";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        /// <summary>
        /// Roundom Key
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RoundomKey(int length = 10)
        {
            const string valid = "abcdefghijkmnopqrstuvwxyzABCDEFGHIJKMNOPQRSTUVWXYZ0123456789";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }








        public static string EncryptDocument(string SourcePdfFile, string DestFilePath, string UserPassword)
        {
            try
            {
                if (GetSecureKeys() == true)
                {
                    string? MasterKey = null;
                    string sEncFile = DestFilePath + Path.GetFileName(SourcePdfFile) + ".enc";
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.GenerateIV();
                        aes.GenerateKey();

                        MasterKey = DocumentSalt + "$" + Convert.ToBase64String(aes.IV) + "$" + Convert.ToBase64String(aes.Key);
                        using (FileStream fsIn = new FileStream(SourcePdfFile, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            using (FileStream fsOut = new FileStream(sEncFile, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                                CryptoStream csEncrypt = new CryptoStream(fsOut, encryptor, CryptoStreamMode.Write);

                                int data;
                                while ((data = fsIn.ReadByte()) != -1)
                                    csEncrypt.WriteByte((byte)data);
                                csEncrypt.FlushFinalBlock();

                                fsOut.Flush();
                                fsOut.Close();
                            }
                            fsIn.Close();
                        }
                        aes.Clear();
                    }
                    if (File.Exists(SourcePdfFile))
                    {
                        File.Delete(SourcePdfFile);
                    }
                    return Encrypt(MasterKey, UserPassword);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static string Encrypt(string MasterKey, string UserPassword)
        {
            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] passwordBytes = utf8.GetBytes(UserPassword);
                byte[] plainBytes = utf8.GetBytes(MasterKey);
                byte[] aesKey = SHA256.Create().ComputeHash(passwordBytes);
                byte[] aesIV = MD5.Create().ComputeHash(passwordBytes);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        byte[] bEnc = memoryStream.ToArray();
                        return Convert.ToBase64String(bEnc);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string DecryptDocument(string SourceFile, string UserKeyRing, string UserId, string UserPassword)
        {
            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] passwordBytes = utf8.GetBytes(UserPassword);
                byte[] encBytes = Convert.FromBase64String(UserKeyRing);
                byte[] aesKey = SHA256.Create().ComputeHash(passwordBytes);
                byte[] aesIV = MD5.Create().ComputeHash(passwordBytes);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

                        cryptoStream.Write(encBytes, 0, encBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        byte[] bClear = memoryStream.ToArray();
                        var MasterKey = utf8.GetString(bClear);
                        return Decrypt(SourceFile, MasterKey, UserId);
                    }

                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static string Decrypt(string SourceFile, string MasterKey, string UserId)
        {
            try
            {
                if (GetSecureKeys() == true)
                {
                    string newSourceFile = Path.Combine(Path.GetDirectoryName(SourceFile), $"{UserId}_" + Path.GetFileName(SourceFile));
                    File.Copy(SourceFile, newSourceFile);

                    byte[] IV = new byte[1];
                    byte[] Key = new byte[1];
                    if (MasterKey.StartsWith(DocumentSalt))
                    {
                        string[] sParts = MasterKey.Split("$");
                        if (sParts.Count() != 3)
                        {
                            throw new Exception("V1 Key provided is not valid does not have 3 parts");
                        }
                        IV = Convert.FromBase64String(sParts[1]);
                        Key = Convert.FromBase64String(sParts[2]);
                    }
                    else
                    {
                        throw new Exception("Key provided is not valid/ not recognised by this system");
                    }

                    string sClearFile = newSourceFile.Substring(0, newSourceFile.Length - 4);
                    if (File.Exists(sClearFile))
                    {
                        File.Delete(sClearFile);
                    }

                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.IV = IV;
                        aes.Key = Key;

                        using (FileStream fsIn = new FileStream(newSourceFile, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            using (FileStream fsOut = new FileStream(sClearFile, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                                CryptoStream csDecrypt = new CryptoStream(fsOut, decryptor, CryptoStreamMode.Write);

                                int data;
                                while ((data = fsIn.ReadByte()) != -1)
                                    csDecrypt.WriteByte((byte)data);
                                csDecrypt.FlushFinalBlock();
                                csDecrypt.Flush();
                                fsOut.Flush();
                                fsOut.Close();
                            }
                            fsIn.Close();
                        }

                        aes.Clear();
                    }
                    if (File.Exists(newSourceFile))
                    {
                        File.Delete(newSourceFile);
                    }
                    return sClearFile;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }






























        #endregion
    }
    public class JwtToken
    {
        public string TokenID { get; set; }
        public DateTime TokenExpairy { get; set; }
        public int UserID { get; set; }
    }
}