namespace DMS_API.ModelsView
{
    public class FolderChildsPermissionsModelView
    {
        public int ParentId { get; set; }       
        public int PageRows { get; set; }
        public int PageNumber { get; set; }
        public bool IsMoveAtion { get; set; } = false;
        public List<int> ObjectsMovable { get; set; }
    }
}
