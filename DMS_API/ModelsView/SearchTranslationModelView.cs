namespace DMS_API.ModelsView
{
    public class SearchTranslationModelView
    {
        public string WordSearch { get; set; }
        public string KeySearch { get; set; } // language or key
        public int PageRows { get; set; }
        public int PageNumber { get; set; }
    }
}