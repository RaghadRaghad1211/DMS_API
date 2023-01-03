namespace DMS_API.ModelsView
{
    public class SearchUsersOrGroupsPermissionOnObject
    {
        public int ObjectId { get; set; }
        public string TitleSearch { get; set; }
        public int PageRows { get; set; }
        public int PageNumber { get; set; }
    }
}