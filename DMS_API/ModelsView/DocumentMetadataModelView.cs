namespace DMS_API.ModelsView
{
    public class DocumentMetadataModelView
    {
        public int ObjId { get; set; }
        public int ObjClsId { get; set; }
        // public bool TbMultiSelect { get; set; }
        public string DocumentFilePath { get; set; }
        public List<KeyValueModel> KeysValues { get; set; }
    }

    public class KeyValueModel
    {
        public int ToolId { get; set; }
        public string ToolType { get; set; }
        public int Key { get; set; }
        public string Value { get; set; }
    }
}
