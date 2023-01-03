namespace DMS_API.ModelsView
{
    public class GetDestPerOnObjectModelView
    {
        public int DestObjId { get; set; }
        public string DestObjTitle { get; set; }
        public int DestTypeId { get; set; }
        public string DestTypeName { get; set; }
        public bool IsRead { get; set; }
        public bool IsWrite { get; set; }
        public bool IsManage { get; set; }
        public bool IsQR { get; set; }
    }
}