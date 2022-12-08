namespace DMS_API.ModelsView
{
    public class FolderModelView
    {
        public int FolderId { get; set; }
        public string FolderTitle { get; set; }
        public int FolderOrgOwnerID { get; set; }
        public bool FolderIsActive { get; set; }
        public string FolderDescription { get; set; }
        public bool IsFavoriteFolder { get; set; }
        public int FolderPerantId { get; set; }
    }
}