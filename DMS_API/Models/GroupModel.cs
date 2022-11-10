namespace DMS_API.Models
{
    public class GroupModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public DateTime CreationDate { get; set; }
        public string OwnerUserName { get; set; }
        public string OwnerFullName { get; set; }
        public string OrgArName { get; set; }
        public string OrgEnName { get; set; }
        public string OrgKuName { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }


      //  [GroupID]
      //,[GroupName]
      //,[CreationDate]
      //,[UserOwnerID]
      //,[OwnerUserName]
      //,[OwnerFullName]
      //,[OrgOwner]
      //,[OrgEnName]
      //,[OrgArName]
      //,[OrgKuName]
      //,[IsActive]
      //,[ObjActiveDate]
      //,[Note]
    }
}