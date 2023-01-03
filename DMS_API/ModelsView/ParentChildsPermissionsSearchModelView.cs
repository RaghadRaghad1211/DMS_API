namespace DMS_API.ModelsView
{
    public class ParentChildsPermissionsSearchModelView
    {
        public int ParentId { get; set; }
        public string ChildTitle { get; set; }
        public int PageRows { get; set; }
        public int PageNumber { get; set; }
    }
}
