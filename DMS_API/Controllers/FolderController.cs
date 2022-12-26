using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        #region Properteis
        private readonly FolderService Folder_S;
        private readonly PermissionsService Permissions_S;
        private readonly LinkParentChildService LinkParentChild_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public FolderController()
        {
            Folder_S = new FolderService();
            LinkParentChild_S = new LinkParentChildService();
            Permissions_S = new PermissionsService();
        }
        #endregion

        #region Actions
        [HttpPost]
        [Route("GetFoldersList")]
        public async Task<IActionResult> GetFoldersList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.GetFoldersList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetFoldersByID/{FolderId}")]
        public async Task<IActionResult> GetFoldersByID([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.GetFoldersByID(FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddFolder")]
        public async Task<IActionResult> AddFolder([FromBody] FolderModelView Folder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.AddFolder(Folder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditFolder")]
        public async Task<IActionResult> EditGroup([FromBody] FolderModelView Folder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.EditFolder(Folder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("SearchFolderByName/{Name}")]
        public async Task<IActionResult> SearchFolderByName([FromRoute] string Name, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.SearchFolderByName(Name, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetChildsInFolderByID/{FolderId}")]
        public async Task<IActionResult> GetChildsInFolderByID([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildInParentByID((int)GlobalService.ClassType.Folder, FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetChildsInFolderByID_Search")]
        public async Task<IActionResult> GetChildsInFolderByID_Search([FromBody] SearchChildParentModelView SearchChildFolder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildInParentByID_Search((int)GlobalService.ClassType.Folder, SearchChildFolder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddChildsIntoFolder")]
        public async Task<IActionResult> AddChildsIntoFolder([FromBody] LinkParentChildModelView LinkParentChild_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.AddChildIntoParent((int)GlobalService.ClassType.Folder, LinkParentChild_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("RemoveChildsFromFolder")]
        public async Task<IActionResult> RemoveChildsFromFolder([FromBody] LinkParentChildModelView LinkParentChild_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.RemoveChildFromParent((int)GlobalService.ClassType.Folder, LinkParentChild_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }


        [HttpPut]
        [Route("MoveFolderToNewFolder")]
        public async Task<IActionResult> MoveFolderToNewFolder([FromBody] MoveChildToNewFolderModelView MoveChildToNewFolder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.MoveChildToNewFolder((int)GlobalService.ClassType.Folder, (int)GlobalService.ClassType.Folder, MoveChildToNewFolder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetChildInFolderByFolderIdWithPermessions")]
        public async Task<IActionResult> GetChildInParentByIdWithPermessions([FromBody] FolderChildsPermissionsModelView FolderChildsPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetChildsInParentWithPermissions((int)GlobalService.ClassType.Folder, FolderChildsPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetPermissionsOnFolderByFolderId/{FolderId}")]
        public async Task<IActionResult> GetPermissionsOnObjectByObjectId([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetPermissionsOnObjectByObjectId((int)GlobalService.ClassType.Folder, FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddPermissionsOnFolder")]
        public async Task<IActionResult> AddPermissionsOnObject([FromBody] AddPermissionsModelView AddPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.AddPermissionsOnObject(AddPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditPermissionsOnFolder")]
        public async Task<IActionResult> EditPermissionsOnObject([FromBody] EditPermissionsModelView EditPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.EditPermissionsOnObject(EditPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }


        [HttpPost]
        [Route("GetUsersOrGroupsHavePermissionOnFolder/{FolderId}")]
        public async Task<IActionResult> GetUsersOrGroupsHavePermissionOnObject([FromRoute] int FolderId, [FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetUsersOrGroupsHavePermissionOnObject(FolderId, Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }


        [HttpPost]
        [Route("GetUsersOrGroupsNotHavePermissionOnFolder/{FolderId}")]
        public async Task<IActionResult> GetUsersOrGroupsNotHavePermissionOnObject([FromRoute] int FolderId, [FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetUsersOrGroupsNotHavePermissionOnObject(FolderId,Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        #endregion
    }
}
