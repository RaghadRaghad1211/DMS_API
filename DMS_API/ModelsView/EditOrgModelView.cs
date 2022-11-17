namespace DMS_API.ModelsView
{
    public class EditOrgModelView
    {
        public int OrgId { get; set; }
        public int OrgUp { get; set; }
        public string OrgArName { get; set; }
        public string OrgEnName { get; set; }
        public string OrgKuName { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }
    }
}