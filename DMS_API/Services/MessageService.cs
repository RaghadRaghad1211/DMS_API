using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_API.Services
{
    public static class MessageService
    {
        #region how use ?
        //var msgA = MessageService.MsgDictionary["Ar"][MessageService.InvalidUsername];
        //var msgE = MessageService.MsgDictionary["En"][MessageService.InvalidUsername];
        #endregion

        //enum EnglishKey
        //{
        //    InvalidEmail,
        //    InvalidUsername,
        //    InvalidPassword,
        //    UsernameOrPasswordNotCorrect,
        //    Password8Characters,
        //    PasswordIsStrength,
        //    PasswordNotStrength,
        //    PasswordMustEnter,
        //    UsernameMustEnter,
        //    NotPermission, // You do not have permission to do this
        //    Forbidden, // You are Forbidden to do this
        //    Unauthorized, // You do not have authoriz to do this
        //    ExpiredToken, // The access token expired
        //    ChangePasswordSuccess,
        //    ChangePasswordFaild,
        //    SaveSuccess,
        //    SaveFaild,
        //    EditSuccess,
        //    EditFaild,
        //    UserNotActive,
        //    LoginFaild,
        //    InsertFaild,
        //    UpdateFaild,
        //    GetSuccess,
        //    GetFaild,
        //    InsertSuccess,
        //    UpdateSuccess,
        //    Active,
        //    NotActive,
        //    IsEmpty,
        //    IsInt,
        //    IsDate,
        //    NoData,
        //    IsExist,
        //    IsNotExist,
        //    MustFillInformation,
        //}

        public static readonly string InvalidEmail = "InvalidEmail";
        public static readonly string InvalidUsername = "InvalidUsername";
        public static readonly string InvalidPassword = "InvalidPassword";
        public static readonly string UsernameOrPasswordNotCorrect = "UsernameOrPasswordNotCorrect";
        public static readonly string Password8Characters = "Password8Characters";
        public static readonly string PasswordIsStrength = "PasswordIsStrength";
        public static readonly string PasswordNotStrength = "PasswordNotStrength";
        public static readonly string PasswordMustEnter = "PasswordMustEnter";
        public static readonly string UsernameMustEnter = "UsernameMustEnter";
        public static readonly string NotPermission = "NotPermission"; // You do not have permission to do this
        public static readonly string Forbidden = "Forbidden"; // You are Forbidden to do this
        public static readonly string Unauthorized = "Unauthorized"; // You do not have authoriz to do this
        public static readonly string ExpiredToken = "ExpiredToken"; // The access token expired
        public static readonly string ChangePasswordSuccess = "ChangeSuccess";
        public static readonly string ChangePasswordFaild = "ChangePasswordFaild";
        public static readonly string SaveSuccess = "SaveSuccess";
        public static readonly string SaveFaild = "SaveFaild";
        public static readonly string EditSuccess = "EditSuccess";
        public static readonly string EditFaild = "EditFaild";
        public static readonly string UserNotActive = "UserNotActive";
        public static readonly string LoginFaild = "LoginFaild";
        public static readonly string InsertFaild = "InsertFaild";
        public static readonly string UpdateFaild = "UpdateFaild";
        public static readonly string GetSuccess = "GetSuccess";
        public static readonly string GetFaild = "GetFaild";
        public static readonly string InsertSuccess = "InsertSuccess";
        public static readonly string UpdateSuccess = "UpdateSuccess";
        public static readonly string Active = "Active";
        public static readonly string NotActive = "NotActive";
        public static readonly string IsEmpty = "IsEmpty";
        public static readonly string IsRequired = "IsRequired";
        public static readonly string IsInt = "IsInt";
        public static readonly string IsDate = "IsInt";
        public static readonly string NoData = "NoData";
        public static readonly string IsExist = "IsExist";
        public static readonly string IsNotExist = "IsNotExist";
        public static readonly string MustFillInformation = "MustFillInformation";
        public static readonly string ServerError = "Server Error";
        public static readonly string ExceptionError = "Exception Error";
        public static readonly string TokenEmpty = "Token Empty";
        public static readonly string ConfirmPasswordIsIncorrect = "ConfirmPasswordIsIncorrect";










        public static readonly string InvalidIdentity = "InvalidIdentity";
        public static readonly string InvalidVerificationCode = "InvalidVerificationCode";

        public static readonly string UserNotExist = "UserNotExist";
        public static readonly string UserIsDeleted = "UserIsDeleted";


        public static readonly string OldPasswordNotCorrect = "OldPasswordNotCorrect";
        public static readonly string VerificationCodeNotCorrect = "VerificationCodeNotCorrect";
        public static readonly string UsernameOrEmailAlreadyExist = "UsernameOrEmailAlreadyExist";
        public static readonly string EmailOrVerificationCodeNotCorrect = "EmailOrVerificationCodeNotCorrect";


        public static readonly string RegisterFaild = "RegisterFaild";
        public static readonly string VerificationFaild = "VerificationFaild";

        public static readonly string ForgetPasswordFaild = "ForgetPasswordFaild";
        public static readonly string FindUserProfileFaild = "FindUserProfileFaild";
        public static readonly string GetUserAccountsFaild = "GetUserAccountsFaild";
        public static readonly string DeleteFaild = "DeleteFaild";
        public static readonly string UndoDeleteFaild = "UndoDeleteFaild";
        public static readonly string PermanentlyDeleteFaild = "PermanentlyDeleteFaild";


        public static readonly Dictionary<string, Dictionary<string, string>> MsgDictionary = new()
        {
            {
                "ar",
                new Dictionary<string, string>()
                {
                    {NoData,"لا توجد بيانات" },
                    {UsernameMustEnter,"يجب ادخال اسم المستخدم" },
                    {PasswordMustEnter,"يجب ادخال اسم كلمة المرور" },
                    {Password8Characters, "يجب ادخال 8 احرف او اكثر" },
                    {GetSuccess,"تم جلب البيانات بشكل صحيح" },
                    {GetFaild,"فشل في جلب البيانات" },
                    { LoginFaild, "فشل بالدخول" },
                    { UserNotActive, "المستخدم غير فعال" },
                    { InsertSuccess, "تمت عملية الاضافة" },
                    { InsertFaild, "فشل بعملية الادخال" },
                    { IsExist, "موجود" },
                    { IsNotExist, "غير موجود" },
                    { IsEmpty, "فارغ" },
                    { IsRequired, "مطلوب" },
                    { MustFillInformation, "يجب ملئ البيانات" },
                    { UpdateSuccess, "تمت عملية التعديل" },
                    { UpdateFaild, "فشل بعملية التعديل" },
                    { Unauthorized, "المستخدم غير مصرح له بطلب هذا الإجراء" },
                    { Forbidden, "المستخدم ممنوع من طلب هذا الإجراء" },
                    { ExpiredToken, "انتهت صلاحية رمز الوصول" },
                    { ExceptionError, "فشل - استثناء" },
                    {ServerError ,"خطأ في الخادم"},
                    {IsInt ,"يجب ادخال رقم"},
                    {TokenEmpty ,"رمز الوصول فارغ"},
                    {ConfirmPasswordIsIncorrect ,"تأكيد كلمة المرور غير صحيحة"},








                    { InvalidUsername, "اسم مستخدم غير صالح" },
                    { InvalidPassword, "كلمة المرور غير صالحة" },
                    { InvalidEmail, "" },
                    { PasswordIsStrength, "كلمة المرور قوية" },
                    { PasswordNotStrength, "كلمة المرور ضعيفة" },
                    { UsernameOrPasswordNotCorrect, "" },

                    { RegisterFaild, "" },
                    { UserIsDeleted, "" },
                    { UsernameOrEmailAlreadyExist, "" },

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
                "en",
                new Dictionary<string, string>()
                {
                     {NoData,"No Data Found" },
                     {UsernameMustEnter,"Username Must Enter" },
                     {PasswordMustEnter,"Password Must Enter" },
                     {Password8Characters, "Must enter 8 characters or more" },
                     {GetSuccess,"Get Data Success" },
                     {GetFaild,"Get Data Faild" },
                     {LoginFaild,"Login Faild"},
                     { UserNotActive, "User is Not Active" },
                      { InsertSuccess, "Insert Success" },
                    { InsertFaild, "Insert Faild" },
                    { IsExist, "Is Exist" },
                    { IsNotExist, "Is Not Exist" },
                    { IsEmpty, "Is Empty" },
                    { IsRequired, "Is Required" },
                    { MustFillInformation, "Must Fill Information" },
                    { UpdateSuccess, "Update Success" },
                    { UpdateFaild, "Update Faild" },
                    { Unauthorized, "User is Unauthorized to request this action" },
                    { Forbidden, "User is Forbidden from requesting this action" },
                    { ExpiredToken, "The access token expired" },
                    { ExceptionError, "Faild - Exception" },
                    {ServerError ,"Server Error"},
                    {IsInt ,"Must enter Number"},
                    {TokenEmpty ,"Token Is Empty"},
                    {ConfirmPasswordIsIncorrect ,"Confirm Password Is Incorrect"},






                    { InvalidUsername, "Invalid Username" },
                    { InvalidPassword, "Invalid Password" },
                    { InvalidEmail, "" },
                    { PasswordIsStrength, "Password is strong" },
                    { PasswordNotStrength, "Password is weak" },
                    { UsernameOrPasswordNotCorrect, "" },
                    { RegisterFaild, "" },
                    { UserIsDeleted, "" },
                    { UsernameOrEmailAlreadyExist, "" },
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