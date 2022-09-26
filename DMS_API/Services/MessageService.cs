using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_API.Services
{
    public static class MessageService
    {
        #region how use ?
        //var msgA = MessageService.MsgDictionary["Ar"]["InvalidUsername"];
        //var msgE = MessageService.MsgDictionary["En"]["InvalidUsername"];
        #endregion


        public static readonly string InvalidEmail = "InvalidEmail";
        public static readonly string InvalidUsername = "InvalidUsername";
        public static readonly string InvalidPassword = "InvalidPassword";
        public static readonly string InvalidIdentity = "InvalidIdentity";
        public static readonly string InvalidVerificationCode = "InvalidVerificationCode";

        public static readonly string UserNotExist = "UserNotExist";
        public static readonly string UserIsDeleted = "UserIsDeleted";
        public static readonly string UserNotActive = "UserNotActive";
        public static readonly string Password8Characters = "Password8Characters";
        public static readonly string PasswordIsStrength = "PasswordIsStrength";
        public static readonly string PasswordNotStrength = "PasswordNotStrength";
        public static readonly string OldPasswordNotCorrect = "OldPasswordNotCorrect";
        public static readonly string VerificationCodeNotCorrect = "VerificationCodeNotCorrect";
        public static readonly string UsernameOrEmailAlreadyExist = "UsernameOrEmailAlreadyExist";
        public static readonly string UsernameOrPasswordNotCorrect = "UsernameOrPasswordNotCorrect";
        public static readonly string EmailOrVerificationCodeNotCorrect = "EmailOrVerificationCodeNotCorrect";

        public static readonly string LoginFaild = "LoginFaild";
        public static readonly string RegisterFaild = "RegisterFaild";
        public static readonly string VerificationFaild = "VerificationFaild";
        public static readonly string ChangePasswordFaild = "ChangePasswordFaild";
        public static readonly string ForgetPasswordFaild = "ForgetPasswordFaild";
        public static readonly string FindUserProfileFaild = "FindUserProfileFaild";
        public static readonly string GetUserAccountsFaild = "GetUserAccountsFaild";
        public static readonly string GetFaild = "GetFaild";
        public static readonly string InsertFaild = "InsertFaild";
        public static readonly string UpdateFaild = "UpdateFaild";
        public static readonly string DeleteFaild = "DeleteFaild";
        public static readonly string UndoDeleteFaild = "UndoDeleteFaild";
        public static readonly string PermanentlyDeleteFaild = "PermanentlyDeleteFaild";


        public static readonly Dictionary<string, Dictionary<string, string>> MsgDictionary = new()
        {
            {
                "Ar",
                new Dictionary<string, string>()
                {
                    { InvalidUsername, "اسم مستخدم غير صالح" },
                    { InvalidPassword, "كلمة المرور غير صالحة" },
                    { InvalidEmail, "" },
                    { Password8Characters, "يجب ادخال 8 احرف او اكثر" },
                    { PasswordIsStrength, "كلمة المرور قوية" },
                    { PasswordNotStrength, "كلمة المرور ضعيفة" },
                    { UsernameOrPasswordNotCorrect, "" },
                    { LoginFaild, "فشل بالدخول" },
                    { RegisterFaild, "" },
                    { UserIsDeleted, "" },
                    { UsernameOrEmailAlreadyExist, "" },
                    { UserNotActive, "" },
                    { VerificationFaild, "" },
                    { InvalidVerificationCode, "" },
                    { EmailOrVerificationCodeNotCorrect, "" },
                    { InvalidIdentity, "" },
                    { UserNotExist, "" },
                    { ForgetPasswordFaild, "" },
                    { FindUserProfileFaild, "" },
                    { ChangePasswordFaild, "" },
                    { OldPasswordNotCorrect, "" },
                    { VerificationCodeNotCorrect, "" }                    
                }
            },
            {
                "En",
                new Dictionary<string, string>()
                {
                    { InvalidUsername, "Invalid Username" },
                    { InvalidPassword, "Invalid Password" },
                    { InvalidEmail, "" },
                    { Password8Characters, "Must enter 8 characters or more" },
                    { PasswordIsStrength, "Password is strong" },
                    { PasswordNotStrength, "Password is weak" },
                    { UsernameOrPasswordNotCorrect, "" },
                    { LoginFaild, "Login Faild" },
                    { RegisterFaild, "" },
                    { UserIsDeleted, "" },
                    { UsernameOrEmailAlreadyExist, "" },
                    { UserNotActive, "" },
                    { VerificationFaild, "" },
                    { InvalidVerificationCode, "" },
                    { EmailOrVerificationCodeNotCorrect, "" },
                    { InvalidIdentity, "" },
                    { UserNotExist, "" },
                    { ForgetPasswordFaild, "" },
                    { FindUserProfileFaild, "" },
                    { ChangePasswordFaild, "" },
                    { OldPasswordNotCorrect, "" },
                    { VerificationCodeNotCorrect, "" }
                }
            }
        };
    }
}