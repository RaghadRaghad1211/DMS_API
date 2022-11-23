namespace DMS_API.Models
{
    public class OrgTableModel
    {
        public int OrgId { get; set; }
        public int OrgUp { get; set; }
        public int OrgLevel { get; set; }
        public string OrgArName { get; set; }
        public string OrgEnName { get; set; }
        public string OrgKuName { get; set; }
        public string OrgArNameUp { get; set; }
        public string OrgEnNameUp { get; set; }
        public string OrgKuNameUp { get; set; }
        public bool OrgIsActive { get; set; }
        public string Note { get; set; }
    }
}