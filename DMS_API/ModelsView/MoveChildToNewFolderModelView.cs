using System.Text.Json.Serialization;

namespace DMS_API.ModelsView
{
    public class MoveChildToNewFolderModelView
    {
        [JsonPropertyName("CurrentParentID")]
        public int CurrentFolderId { get; set; }
        public List<int> ChildIds { get; set; }

        [JsonPropertyName("NewParentID")]
        public int NewFolderId { get; set; }
    }
}
