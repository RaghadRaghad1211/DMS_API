namespace DMS_API.ModelsView
{
    public class FolderChildsPermissionsSearchModelView
    {
        public int ParentId { get; set; }
        public string ChildTitle { get; set; }
        public int PageRows { get; set; }
        public int PageNumber { get; set; }
    }
}
