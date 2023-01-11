namespace DMS_API.Models
{
    public class QRLookupModel
    {
        public int QrDocumentId { get; set; }
        public bool QrIsPraivet { get; set; }
        public DateTime QrExpiry { get; set; }
    }
}