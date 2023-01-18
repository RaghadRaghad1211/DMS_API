using DMS_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DMS_API.ModelsView
{
    public class DocumentModelView
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; }
        public int DocumentOrgOwnerID { get; set; }
        public string DocumentDescription { get; set; }
        public int DocumentPerantId { get; set; }
        public string KeysValues { get; set; }
        public IFormFile? DocumentFile { get; set; }
    }
}



