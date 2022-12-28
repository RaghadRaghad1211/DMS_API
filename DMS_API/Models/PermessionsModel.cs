using System.Text.Json.Serialization;

namespace DMS_API.Models
{
    public class PermessionsModel
    {
        [JsonPropertyName("ChildId")]
        public int SourObjId { get; set; }

        [JsonPropertyName("ChildTitle")]
        public string SourTitle { get; set; }

        [JsonPropertyName("ChildClsId")]
        public int SourType { get; set; }

        [JsonPropertyName("ChildClassType")]
        public string SourTypeName { get; set; }

        public int DestObjId { get; set; }
        public string DestTitle { get; set; }
        public int DestType { get; set; }
        public string DestTypeName { get; set; }
        public bool IsRead { get; set; }
        public bool IsWrite { get; set; }
        public bool IsManage { get; set; }
        public bool IsQR { get; set; }
        [JsonPropertyName("ChildUserOwner")]
        public string SourUserName { get; set; }

        [JsonPropertyName("ChildOrgArOwner")]
        public string SourOrgArName { get; set; }

        [JsonPropertyName("ChildOrgEnOwner")]
        public string SourOrgEnName { get; set; }

        [JsonPropertyName("ChildOrgKrOwner")]
        public string SourOrgKuName { get; set; }

        [JsonPropertyName("ChildCreationDate")]
        public string SourCreationDate { get; set; }

        public List<TrackingPathModel> TrackingPath { get; set; }
    }
    public class TrackingPathModel
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; }
    }
    public class PermissionTypeModel
    {
        public int UserId { get; set; }
        public int ObjectId { get; set; }
        public bool IsRead { get; set; }
        public bool IsWrite { get; set; }
        public bool IsManage { get; set; }
        public bool IsQR { get; set; }
    }
}
