using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {


        #region Properteis
        private readonly PermissionsService Permissions_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public PermissionController()
        {
            Permissions_S = new PermissionsService();
        }
        #endregion

        #region Actions
        [HttpPost]
        [Route("GetChildInFolderByFolderIdWithPermessions")]
        public async Task<IActionResult> GetChildInFolderByIdWithPermessions([FromBody] FolderChildsPermissionsModelView FolderChildsPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetChildsInParentWithPermissions(FolderChildsPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetChildInFolderByFolderIdWithPermessions_Search")]
        public async Task<IActionResult> GetChildInFolderByIdWithPermessions_Search([FromBody] FolderChildsPermissionsSearchModelView FolderChildsPermissionsSearch_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetChildsInParentWithPermissions_Search(FolderChildsPermissionsSearch_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetPermissionsOnFolderByFolderId/{FolderId}")]
        public async Task<IActionResult> GetPermissionsOnFolderByObjectId([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetPermissionsOnObjectByObjectId(FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }


        [HttpPost]
        [Route("AddPermissionsOnObject")]
        public async Task<IActionResult> AddPermissionsOnObject([FromBody] AddPermissionsModelView AddPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.AddPermissionsOnObject(AddPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditPermissionsOnObject")]
        public async Task<IActionResult> EditPermissionsOnObject([FromBody] EditPermissionsModelView EditPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.EditPermissionsOnObject(EditPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }


        [HttpPost]
        [Route("GetUsersOrGroupsHavePermissionOnObject/{ObjectId}")]
        public async Task<IActionResult> GetUsersOrGroupsHavePermissionOnObject([FromRoute] int ObjectId, [FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetUsersOrGroupsHavePermissionOnObject(ObjectId, Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }


        [HttpPost]
        [Route("GetUsersOrGroupsNotHavePermissionOnObject/{ObjectId}")]
        public async Task<IActionResult> GetUsersOrGroupsNotHavePermissionOnObject([FromRoute] int ObjectId, [FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetUsersOrGroupsNotHavePermissionOnObject(ObjectId, Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        #endregion
    }
}