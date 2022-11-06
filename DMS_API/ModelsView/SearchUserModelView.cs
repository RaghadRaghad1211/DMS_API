using System.ComponentModel;

namespace DMS_API.ModelsView
{
    public class SearchUserModelView
    {
        public string FullName { get; set; }
        public string UsPhoneNo { get; set; }
        public string UsEmail { get; set; }
        public string UsUserEmpNo { get; set; }
        public string UsUserIdintNo { get; set; }
        public int PageRows { get; set; }
        public int PageNumber { get; set; }
    }
}
