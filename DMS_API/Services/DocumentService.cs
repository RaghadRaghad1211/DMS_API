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
        private DocumentModel Document_M { get; set; }
        private List<DocumentModel> Document_Mlist { get; set; }
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





// get permession when user click on write or manage or QR...
        // SELECT SourObjId, SourName, SourClsId, SourClsType, DestObjId, DestName, DestClsId, DestClsType, PerRead, PerWrite, PerManage, PerQR
        // FROM [Document].[GetPermissionsOnObject](UserId,@ObjectId )
        // WHERE SourObjId =ObjClicked AND PerManage =1



        // to open folder get chilid where user have permession...
        // SELECT DISTINCT OpenObject.SourObjId, OpenObject.SourName, OpenObject.SourClsId, OpenObject.SourClsType FROM (SELECT * FROM [Document].[GetPermissionsOnObject](41,6 )) OpenObject