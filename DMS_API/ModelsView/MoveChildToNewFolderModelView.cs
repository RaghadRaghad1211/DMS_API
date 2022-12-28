namespace DMS_API.ModelsView
{
    public class MoveChildToNewFolderModelView
    {
        public int CurrentParentID { get; set; }
        public List<int> ChildIds { get; set; }
        public int NewParentID { get; set; }
    }
}
