using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net;

namespace DMS_API.Services
{
    public class DocumentService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public DocumentService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Functions
     
        #endregion
    }
}