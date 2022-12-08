namespace DMS_API.ModelsView
{
    public class DocumentModelView
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; }
        public int DocumentOrgOwnerID { get; set; }
        public bool DocumentIsActive { get; set; }
        public string DocumentDescription { get; set; }
        public int DocumentPerantId { get; set; }
        public List<KeyValueModel> KeysValues { get; set; }
    }
    public class KeyValueModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}