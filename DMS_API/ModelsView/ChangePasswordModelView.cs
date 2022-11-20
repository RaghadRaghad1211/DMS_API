namespace DMS_API.ModelsView
{
    public class ChangePasswordModelView
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }
    }
}