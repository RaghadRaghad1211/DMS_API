using DMS_API.Models;
using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        #region Properteis
        private readonly PermissionsService Permissions_S;
        private ResponseModelView Response_MV { get; set; }
        public IWebHostEnvironment Environment { get; }

        #endregion

        #region Constructor
        public PermissionController(IWebHostEnvironment environment)
        {
            Environment = environment;
            Permissions_S = new PermissionsService(environment);
        }
        #endregion

        #region Actions
        [HttpPost]
        [Route("GetChildInFolderByFolderIdWithPermessions")]
        public async Task<IActionResult> GetChildInFolderByIdWithPermessions([FromBody] ParentChildsPermissionsModelView FolderChildsPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetChildsInParentWithPermissions(FolderChildsPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetChildInFolderByFolderIdWithPermessions_Search")]
        public async Task<IActionResult> GetChildInFolderByIdWithPermessions_Search([FromBody] ParentChildsPermissionsSearchModelView FolderChildsPermissionsSearch_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetChildsInParentWithPermissions_Search(FolderChildsPermissionsSearch_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetPermissionsOnFolderByFolderId/{FolderId}")]
        public async Task<IActionResult> GetPermissionsOnObjectByObjectId([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetPermissionsOnObjectByObjectId(FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddPermissionsOnObject")]
        public async Task<IActionResult> AddPermissionsOnObject([FromBody] List<AddPermissionsModelView> AddPermissions_MVlist, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.AddPermissionsOnObject(AddPermissions_MVlist, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditPermissionsOnObject")]
        public async Task<IActionResult> EditPermissionsOnObject([FromBody] List<EditPermissionsModelView> EditPermissions_MVlist, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.EditPermissionsOnObject(EditPermissions_MVlist, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetUsersOrGroupsHavePermissionOnObject")]
        public async Task<IActionResult> GetUsersOrGroupsHavePermissionOnObject([FromBody] SearchUsersOrGroupsPermissionOnObject SearchUsersOrGroupsPermissionOnObject_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetUsersOrGroupsHavePermissionOnObject(SearchUsersOrGroupsPermissionOnObject_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetUsersOrGroupsNotHavePermissionOnObject")]
        public async Task<IActionResult> GetUsersOrGroupsNotHavePermissionOnObject([FromBody] SearchUsersOrGroupsPermissionOnObject SearchUsersOrGroupsPermissionOnObject_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetUsersOrGroupsNotHavePermissionOnObject(SearchUsersOrGroupsPermissionOnObject_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GenerateQRcodePDFofDocument")]
        public async Task<IActionResult> GenerateQRcodePDFofDocument([FromBody] QRLookupModel QRLookup_M, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GenerateQRcodePDFofDocument(QRLookup_M, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("ReadQRcodePDFofDocument/{QRcode}")]
        public async Task<IActionResult> ReadQRcodePDFofDocument([FromRoute] string QRcode, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Permissions_S.ReadQRcodePDFofDocument(QRcode, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("ReadQRcodePDFofDocumentPrivate/{QRcode}")]
        public async Task<IActionResult> ReadQRcodePDFofDocumentPrivate([FromRoute] string QRcode, [FromBody] LoginModelView Login_MV, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Permissions_S.ReadQRcodePDFofDocumentPrivate(QRcode, Login_MV, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}