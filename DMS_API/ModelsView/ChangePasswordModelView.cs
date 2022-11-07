namespace DMS_API.ModelsView
{
    public class ChangePasswordModelView
    {
        public int UserID { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
