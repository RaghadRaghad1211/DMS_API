namespace DMS_API.ModelsView
{
    public class RemoveChildFromParentModelView
    {
        public int ParentID { get; set; }
        public List<int> ChildIds { get; set; }
    }
}
