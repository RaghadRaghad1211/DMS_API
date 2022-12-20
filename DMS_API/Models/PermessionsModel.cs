namespace DMS_API.Models
{
    public class PermessionsModel
    {
        public int SourObjId { get; set; }
        public string SourTitle { get; set; }
        public int SourType { get; set; }
        public string SourTypeName { get; set; }
        public int DestObjId { get; set; }
        public string DestTitle { get; set; }
        public int DestType { get; set; }
        public string DestTypeName { get; set; }
        public bool IsRead { get; set; }
        public bool IsWrite { get; set; }
        public bool IsManage { get; set; }
        public bool IsQR { get; set; }
        public string SourUserName { get; set; }
        public string SourOrgArName { get; set; }
        public string SourOrgEnName { get; set; }
        public string SourOrgKuName { get; set; }
        public string SourCreationDate { get; set; }
    }
}
