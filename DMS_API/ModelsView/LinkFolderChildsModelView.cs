using System.Text.Json.Serialization;

namespace DMS_API.ModelsView
{
    public class LinkFolderChildsModelView
    {
        [JsonPropertyName("ParentId")]
        public int FolderId { get; set; }
        public List<int> ChildIds { get; set; }
    }
}