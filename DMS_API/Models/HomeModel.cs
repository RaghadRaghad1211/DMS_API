namespace DMS_API.Models
{
    public class HomeModel
    {
        public List<MyDesktopFolder> MyDesktopFolder { get; set; }
        public List<MyGroup> MyGroups { get; set; }
        public List<MyFavorite> MyFavorites { get; set; }
    }
    public class MyGroup
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
    public class MyFavorite
    {
        public int FavoriteId { get; set; }
        public string FavoriteName { get; set; }
    }
    public class MyDesktopFolder
    {
        public int FolderDesktopId { get; set; }
        public string FolderDesktopName { get; set; }
    }
}
