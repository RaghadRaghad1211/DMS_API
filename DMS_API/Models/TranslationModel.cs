//using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DMS_API.Models
{
    public class TranslationModel
    {
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Trid { get; set; }
        public string TrKey { get; set; }
        public string TrArName { get; set; }
        public string TrEnName { get; set; }
        public string TrKrName { get; set; }
    }
}