namespace DMS_API.Models
{
    public class UserModel
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public bool IsOrgAdmin { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string UserEmpNo { get; set; }
        public string UserIdintNo { get; set; }
        public bool IsOnLine { get; set; }
        public int OrgOwnerID { get; set; }
        public string OrgArName { get; set; }
        public string OrgEnName { get; set; }
        public string OrgKuName { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }
    }
}