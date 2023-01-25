namespace DMS_API.Models
{
    public class SessionModel
    {
        public int UserID { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsOrgAdmin { get; set; }
        public bool IsGroupOrgAdmin { get; set; }
        public bool IsExpairy { get; set; }
        public bool IsActive { get; set; }
    }
}