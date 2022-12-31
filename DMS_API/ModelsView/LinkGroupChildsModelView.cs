using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DMS_API.ModelsView
{
    public class LinkGroupChildsModelView
    {
        [JsonPropertyName("ParentId")]
        public int GroupId { get; set; }
        public List<int> ChildIds { get; set; }
    }
}