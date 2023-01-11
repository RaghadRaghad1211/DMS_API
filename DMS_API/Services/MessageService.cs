namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Respons Messages
    /// </summary>
    public static class MessageService
    {
        #region how use ?
        //var msgA = MessageService.MsgDictionary["Ar"][MessageService.InvalidUsername];
        //var msgE = MessageService.MsgDictionary["En"][MessageService.InvalidUsername];
        #endregion

        public static readonly string InvalidFileSize = "InvalidFileSize";
        public static readonly string InvalidEmail = "InvalidEmail";
        public static readonly string InvalidUsername = "InvalidUsername";
        public static readonly string InvalidPassword = "InvalidPassword";
        public static readonly string UsernameOrPasswordNotCorrect = "UsernameOrPasswordNotCorrect";
        public static readonly string Password8Characters = "Password8Characters";
        public static readonly string PasswordIsStrength = "PasswordIsStrength";
        public static readonly string PasswordNotStrength = "PasswordNotStrength";
        public static readonly string PasswordMustEnter = "PasswordMustEnter";
        public static readonly string UsernameMustEnter = "UsernameMustEnter";
        public static readonly string NotPermission = "NotPermission";
        public static readonly string Forbidden = "Forbidden";
        public static readonly string Unauthorized = "Unauthorized";
        public static readonly string ExpiredToken = "ExpiredToken";
        public static readonly string ChangePasswordSuccess = "ChangeSuccess";
        public static readonly string ChangePasswordFaild = "ChangePasswordFaild";
        public static readonly string SaveSuccess = "SaveSuccess";
        public static readonly string SaveFaild = "SaveFaild";
        public static readonly string DeleteSuccess = "DeleteSuccess";
        public static readonly string DeleteFaild = "DeleteFaild";
        public static readonly string EditSuccess = "EditSuccess";
        public static readonly string EditFaild = "EditFaild";
        public static readonly string MoveSuccess = "MoveSuccess";
        public static readonly string MoveFaild = "MoveFaild";
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
        public static readonly string PhoneIsExist = "PhoneIsExist";
        public static readonly string IsNotExist = "IsNotExist";
        public static readonly string MustFillInformation = "MustFillInformation";
        public static readonly string ServerError = "Server Error";
        public static readonly string ServiceUnavailable = "ServiceUnavailable";
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
        public static readonly string GroupTitleMustEnter = "GroupTitleMustEnter";
        public static readonly string FolderTitleMustEnter = "FolderTitleMustEnter";
        public static readonly string DocumentTitelMustEnter = "DocumentTitelMustEnter";
        public static readonly string DocumentFileMustUpload = "DocumentFileMustUpload";
        public static readonly string TitelArIsEmpty = "TitelArIsEmpty";
        public static readonly string TitelEnIsEmpty = "TitelEnIsEmpty";
        public static readonly string MustSelectedObjects = "MustSelectedObjects";
        public static readonly string OrgNameMustEnter = "OrgNameMustEnter";
        public static readonly string ExtensionMustBePFD = "ExtensionMustBePFD";
        public static readonly string NoPermission = "NoPermission";
        public static readonly string FolderUnOpenable = "FolderUnOpenable";
        public static readonly string GroupUnEditable = "GroupUnEditable";
        public static readonly string FolderUnEditable = "FolderUnEditable";
        public static readonly string GeneratQR = "GeneratQR";
        public static readonly string MustSelectPermissionForObject = "MustSelectedPermissionForObject";
        public static readonly string FileSize = "MustSelectedPermissionForObject";
       // public static readonly string FileSize = "MustSelectedPermissionForObject";

        /// <summary>
        /// Dictionary of respons messages
        /// </summary>
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
                     { ServiceUnavailable, "حدث خطأ اثناء الاتصال في الخادم" },
                     {IsInt ,"يجب ادخال رقم"},
                     {TokenEmpty ,"رمز الوصول فارغ"},
                     {ConfirmPasswordIsIncorrect ,"تأكيد كلمة المرور غير صحيحة"},
                     {PhoneIncorrect ,"تنسيق رقم الهاتف غير صحيح ، يجب ان يكون 13 رقم"},
                     {EmailIncorrect ,"تنسيق البريد الالكتروني غير صحيح"},
                     {FSTname ,"يجب ملئ على الاقل الاسم الثلاثي للمستخدم"},
                     {IsReset ,"تم اعادة تعيين كلمة المرور"},
                     {EmailSubjectPasswordIsReset, "إعادة تعيين كلمة المرور" },//
                     {EmailBodyPasswordIsReset ,"تم إعادة تعيين كلمة المرور لنظام ادارة الوثائق .. كلمة المرور الجديدة : "},//
                     {PhonePasswordIsReset ,"تم إعادة تعيين كلمة المرور  لنظام ادارة الوثائق : "},//
                     {OldPasswordNotCorrect ,"كلمة المرور القديمة غير صحيحة"},
                     {ChangePasswordSuccess ,"تم تغيير كلمة المرور"},
                     {DeblicateKey ,"الكلمة المفتاحية موجودة في قاعدة البيانات"},
                     {GroupTitleMustEnter ,"يجب ادخال عنوان المجموعة"},
                     {FolderTitleMustEnter ,"يجب ادخال عنوان المجلد"},
                     {TitelArIsEmpty ,"يجب ادخال العنوان العربي"},
                     {TitelEnIsEmpty ,"يجب ادخال العنوان الانكليزي"},
                     {MustSelectedObjects ,"يجب اختيار عناصر"},
                     {DeleteSuccess, "تمت عملية الحذف" },
                     {DeleteFaild, "فشل بعملية الحذف" },
                     {OrgNameMustEnter, "يجب اضافة اسم الموؤسسة" },
                     { UsernameIsExist, "أسم المستخدم موجود" },
                     { PhoneIsExist, "رقم الهاتف موجود" },
                     { MoveSuccess, "تمت عملية النقل" },
                     { MoveFaild, "فشل بعملية النقل" },
                     {DocumentTitelMustEnter ,"يجب ادخال عنوان الوثيقة"},
                     {DocumentFileMustUpload ,"يجب اختيار صورة الوثيقة"},
                     {ExtensionMustBePFD ,"يجب اختيار وثيقة بصيغة "+"PDF"},
                     { NoPermission, "ليس لديك صلاحية لعمل هذا الاجراء" },
                     { FolderUnOpenable, "لايمكن نقل داخل هذا المجلد" },
                     { GroupUnEditable, "لايمكن تعديل هذه المجموعة" },
                     { FolderUnEditable, "لايمكن تعديل هذا المجلد" },
                     { InvalidUsername, "اسم مستخدم غير صالح" },
                     { InvalidPassword, "كلمة المرور غير صالحة" },
                     { GeneratQR, "تم توليد رمز الوثيقة" },
                     { MustSelectPermissionForObject, "يجب اختيار صلاحية واحدة على الاقل للعنصر " },
                     { InvalidFileSize, $"يجب ان يكون حجم الملف اقل من {GlobalService.MaxFileSize}" },


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
                     { EditFaild, "Edit Faild" },
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
                     { ServiceUnavailable, "An error occurred while connecting to the server" },
                     {IsInt ,"Must enter Number"},
                     {TokenEmpty ,"Token Is Empty"},
                     {ConfirmPasswordIsIncorrect ,"Confirm Password Is Incorrect"},
                     {PhoneIncorrect ,"The phone number is formatted incorrectly, it must be 13 digits"},
                     {EmailIncorrect ,"The email is formatted incorrectly"},
                     {FSTname ,"You must fill in at least the full name of the user"},
                     {IsReset ,"Password is reset"},
                     {EmailSubjectPasswordIsReset ,"Password Reset"},//
                     {EmailBodyPasswordIsReset ,"The password has been reset for Documents Management System.. The new password: "},//
                     {PhonePasswordIsReset ,"The password has been reset for Documents Management System: "},//
                     {OldPasswordNotCorrect ,"The old password is incorrectly"},
                     {ChangePasswordSuccess ,"Password changed"},
                     {DeblicateKey ,"The keyword is exist in database"},
                     {GroupTitleMustEnter ,"Group Title Must Enter"},
                     {FolderTitleMustEnter ,"Folder Title Must Enter"},
                     {TitelArIsEmpty ,"The Arabic titel must be entered"},
                     {TitelEnIsEmpty ,"The English titel must be entered"},
                     {MustSelectedObjects ,"Must Select Objects"},
                     {DeleteSuccess, "Delete Success" },
                     {DeleteFaild, "Delete Faild" },
                     {OrgNameMustEnter, "OrgName Must Enter" },
                     {UsernameIsExist, "Username is exist" },
                      { PhoneIsExist, "Phone Is Exist" },
                     { MoveSuccess, "Move Success" },
                     { MoveFaild, "Move Faild" },
                     {DocumentTitelMustEnter ,"Document Titel Must Enter"},
                     {DocumentFileMustUpload ,"Document File Must Upload"},
                      {ExtensionMustBePFD ,"Extension of Document Must be "+"PDF"},
                      { NoPermission, "You don't have permission to do this action" },
                      { FolderUnOpenable, "It is not possible to move inside this folder" },
                     { GroupUnEditable, "It is not possible to edit this group" },
                     { FolderUnEditable, "It is not possible to edit this folder" },
                     { InvalidUsername, "Invalid Username" },
                     { InvalidPassword, "Invalid Password" },
                     { GeneratQR, "Generat QR code" },
                     { MustSelectPermissionForObject, "Must Select Permission For Object " },
                     { InvalidFileSize, $"File size must be less than {GlobalService.MaxFileSize}" },


                }
            }
        };
    }
}