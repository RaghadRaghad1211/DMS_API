namespace DMS_API.Models
{
    public class QRLookupModel
    {
        public int QrId { get; set; }
        public int QrDocumentId { get; set; }
        public bool QrIsPraivet { get; set; }
        public DateTime QrExpiry { get; set; }
        public bool QrIsActive { get; set; }
    }
}