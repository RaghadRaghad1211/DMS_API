namespace DMS_API.Models
{
    public class LinkParentChildModel
    {
        public int LcId { get; set; }
        public int ParentId { get; set; }
        public string ParentTitle { get; set; }
        public int ParentClsId { get; set; }
        public string ParentClassType { get; set; }
        public int ChildId { get; set; }
        public string ChildTitle { get; set; }
        public int ChildClsId { get; set; }
        public string ChildClassType { get; set; }
        public string ChildCreationDate { get; set; }
        public bool LcIsActive { get; set; }
        public int ParentUserOwnerId { get; set; }
        public int ParentOrgOwnerId { get; set; }
    }
}