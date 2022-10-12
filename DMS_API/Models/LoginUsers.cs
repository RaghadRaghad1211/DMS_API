namespace DMS_API.Models
{
    public class LogInUsers
    {
        public  int UsId { get; set; }
        public string UsFirstName { get; set; }
        public string UsSecondName { get; set; }
        public string UsThirdName { get; set; }
        public string UsLastName { get; set; }
        public string UsUserName { get; set; }
        public string UsPaswored { get; set; }
        public int OrgNo { get; set; }
        public string UsPhoneNo { get; set; }
        public string UsEmail { get; set; }
        public bool UsIsOnLine { get; set; }
        public DateTime UsOnOffDate { get; set; }
        public string UsUserEmpNo { get; set; }
        public string UsUserIdintNo { get; set; }

    }
}
