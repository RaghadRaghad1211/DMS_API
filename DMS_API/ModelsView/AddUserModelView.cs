using System.Text.Json.Serialization;

namespace DMS_API.ModelsView
{
    public class AddUserModelView
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }      
        public string Password { get; set; }
        public string Password2 { get; set; }
        public string Role { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string UserEmpNo { get; set; }
        public string UserIdintNo { get; set; }
        public int UserOwner { get; set; }
        public int OrgOwner { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }
    }
}
