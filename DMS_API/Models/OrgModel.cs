namespace DMS_API.Models
{
    public class OrgModel
    {
        public int OrgId { get; set; }
        public int OrgUp { get; set; }
        public int OrgLevel { get; set; }
        public string OrgArName { get; set; }
        public string OrgEnName { get; set; }
        public string OrgKuName { get; set; }
        public List<OrgModel> OrgChild { get; set; }
    }
}