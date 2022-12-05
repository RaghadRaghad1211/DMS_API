namespace DMS_API.Services
{
    public static class MessageService
    {
        #region how use ?
        //var msgA = MessageService.MsgDictionary["Ar"][MessageService.InvalidUsername];
        //var msgE = MessageService.MsgDictionary["En"][MessageService.InvalidUsername];
        #endregion

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
        public static readonly string DeleteSuccess = "DeleteSuccess";
        public static readonly string DeleteFaild = "DeleteFaild";
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
        public static readonly string UsernameIsExist = "UsernameIsExist";
        public static readonly string IsNotExist = "IsNotExist";
        public static readonly string MustFillInformation = "MustFillInformation";
        public static readonly string ServerError = "Server Error";
        public static readonly string ExceptionError = "Exception Error";
        public static readonly string TokenEmpty = "Token Empty";
        public static readonly string ConfirmPasswordIsIncorrect = "ConfirmPasswordIsIncorrect";
        public static readonly string PhoneIncorrect = "PhoneIncorrect";
        public static readonly string EmailIncorrect = "EmailIncorrect";
        public static readonly string FSTname = "FSTname";
        public static readonly string IsReset = "IsReset";
        public static readonly string EmailSubjectPasswordIsReset = "EmailSubjectPasswordIsReset";
        public static readonly string EmailBodyPasswordIsReset = "EmailBodyPasswordIsReset";
        public static readonly string PhonePasswordIsReset = "PhonePasswordIsReset";
        public static readonly string OldPasswordNotCorrect = "OldPasswordNotCorrect";
        public static readonly string DeblicateKey = "DeblicateKey";
        public static readonly string GroupNameMustEnter = "GroupNameMustEnter";
        public static readonly string FolderNameMustEnter = "FolderNameMustEnter";
        public static readonly string TitelArIsEmpty = "TitelArIsEmpty";
        public static readonly string TitelEnIsEmpty = "TitelEnIsEmpty";
        public static readonly string MustSelectedObjects = "MustSelectedObjects";
        public static readonly string OrgNameMustEnter = "OrgNameMustEnter";










        public static readonly string InvalidIdentity = "InvalidIdentity";
        public static readonly string InvalidVerificationCode = "InvalidVerificationCode";

        public static readonly string UserNotExist = "UserNotExist";
        public static readonly string UserIsDeleted = "UserIsDeleted";



        public static readonly string VerificationCodeNotCorrect = "VerificationCodeNotCorrect";
        public static readonly string UsernameOrEmailAlreadyExist = "UsernameOrEmailAlreadyExist";
        public static readonly string EmailOrVerificationCodeNotCorrect = "EmailOrVerificationCodeNotCorrect";


        public static readonly string RegisterFaild = "RegisterFaild";
        public static readonly string VerificationFaild = "VerificationFaild";

        public static readonly string ForgetPasswordFaild = "ForgetPasswordFaild";
        public static readonly string FindUserProfileFaild = "FindUserProfileFaild";
        public static readonly string GetUserAccountsFaild = "GetUserAccountsFaild";
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
                     { EditSuccess, "تمت عملية التعديل" },
                     { EditFaild, "فشل بعملية التعديل" },
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
                     {PhoneIncorrect ,"تنسيق رقم الهاتف غير صحيح ، يجب ان يكون 11 رقم"},
                     {EmailIncorrect ,"تنسيق البريد الالكتروني غير صحيح"},
                     {FSTname ,"يجب ملئ على الاقل الاسم الثلاثي للمستخدم"},
                     {IsReset ,"تم اعادة تعيين كلمة المرور"},
                     {EmailSubjectPasswordIsReset, "إعادة تعيين كلمة المرور" },//
                     {EmailBodyPasswordIsReset ,"تم إعادة تعيين كلمة المرور .. كلمة المرور الجديدة : "},//
                     {PhonePasswordIsReset ,"تم إعادة تعيين كلمة المرور : "},//
                     {OldPasswordNotCorrect ,"كلمة المرور القديمة غير صحيحة"},
                     {ChangePasswordSuccess ,"تم تغيير كلمة المرور"},
                     {DeblicateKey ,"الكلمة المفتاحية موجودة في قاعدة البيانات"},
                     {GroupNameMustEnter ,"يجب ادخال اسم المجموعة"},
                     {FolderNameMustEnter ,"يجب ادخال اسم الفايل"},
                     {TitelArIsEmpty ,"يجب ادخال العنوان العربي"},
                     {TitelEnIsEmpty ,"يجب ادخال العنوان الانكليزي"},
                     {MustSelectedObjects ,"يجب اختيار عناصر"},
                     {DeleteSuccess, "تمت عملية الحذف" },
                     {DeleteFaild, "فشل بعملية الحذف" },//OrgNameMustEnter
                     {OrgNameMustEnter, "يجب اضافة اسم الموؤسسة" },
                     { UsernameIsExist, "أسم المستخدم موجود" },








                    { InvalidUsername, "اسم مستخدم غير صالح" },
                    { InvalidPassword, "كلمة المرور غير صالحة" },
                    { InvalidEmail, "" },


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
                     { EditSuccess, "Edit Success" },
                     { EditFaild, "Faild Success" },
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
                     {PhoneIncorrect ,"The phone number is formatted incorrectly, it must be 11 digits"},
                     {EmailIncorrect ,"The email is formatted incorrectly"},
                     {FSTname ,"You must fill in at least the full name of the user"},
                     {IsReset ,"Password is reset"},
                     {EmailSubjectPasswordIsReset ,"Password Reset"},//
                     {EmailBodyPasswordIsReset ,"The password has been reset.. The new password: "},//
                     {PhonePasswordIsReset ,"The password has been reset: "},//
                     {OldPasswordNotCorrect ,"The old password is incorrectly"},
                     {ChangePasswordSuccess ,"Password changed"},
                     {DeblicateKey ,"The keyword is exist in database"},
                     {GroupNameMustEnter ,"Group Name Must Enter"},
                     {FolderNameMustEnter ,"Folder Name Must Enter"},
                     {TitelArIsEmpty ,"The Arabic titel must be entered"},
                     {TitelEnIsEmpty ,"The English titel must be entered"},
                     {MustSelectedObjects ,"Must Select Objects"},
                     {DeleteSuccess, "Delete Success" },
                     {DeleteFaild, "Delete Faild" },
                     {OrgNameMustEnter, "OrgName Must Enter" },
                     {UsernameIsExist, "Username is exist" },














                    { InvalidUsername, "Invalid Username" },
                    { InvalidPassword, "Invalid Password" },
                    { InvalidEmail, "" },


                }
            }
        };
    }
}