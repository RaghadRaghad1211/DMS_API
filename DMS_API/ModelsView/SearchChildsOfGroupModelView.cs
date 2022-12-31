using System.Text.Json.Serialization;

namespace DMS_API.ModelsView
{
    public class SearchChildsOfGroupModelView
    {
        [JsonPropertyName("ParentId")]
        public int GroupId { get; set; }
        public int ChildTypeId { get; set; }
        public string TitleSearch { get; set; }
    }
}