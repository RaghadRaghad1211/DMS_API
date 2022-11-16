namespace DMS_API.ModelsView
{
    public class GroupModelView
    {
        public int GroupId { get; set; }
        //public int ObjClsId { get; set; }
        public string GroupTitle { get; set; }
        //public int ObjUserOwnerID { get; set; }
        public int GroupOrgOwnerID { get; set; }
        public bool GroupIsActive { get; set; }
        public string GroupDescription { get; set; }
    }
}