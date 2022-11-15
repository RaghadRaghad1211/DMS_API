namespace DMS_API.Models
{
    public class GroupOrFolderModel
    {
        public int ObjId { get; set; }        
        public string ObjTitle { get; set; }
        public int ObjClsId { get; set; }
        public string ClsName { get; set; }
        public bool ObjIsActive { get; set; }
        public DateTime ObjCreationDate { get; set; }
        public bool ObjIsDesktopFolder { get; set; }
        public string ObjDescription { get; set; }
        public int UserOwnerID { get; set; }
        public string OwnerFullName { get; set; }
        public string OwnerUserName { get; set; }
        public string OrgOwner { get; set; }
        public string OrgEnName { get; set; }
        public string OrgArName { get; set; }
        public string OrgKuName { get; set; }
    }
}