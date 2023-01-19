namespace DMS_API.Models
{
    public class LogModel
    {
        public int LogId { get; set; }
        public int LogUserId { get; set; }
        public int LogObjId { get; set; }
        public string LogActionDate { get; set; }
        public string LogAction { get; set; }
        public string LogDescription { get; set; }
        public bool LogOldValue { get; set; }
        public bool LogNewValue { get; set; }
    }
}