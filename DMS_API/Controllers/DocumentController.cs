using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        #region Properteis
        private DocumentService Document_S;
        private readonly PermissionsService Permissions_S;
        private readonly LinkParentChildService LinkParentChild_S;
        private ResponseModelView Response_MV { get; set; }
        public IWebHostEnvironment Environment { get; }
        #endregion

        #region Constructor
        public DocumentController(IWebHostEnvironment environment)
        {
            Environment = environment;
            Document_S = new DocumentService(environment);
            LinkParentChild_S = new LinkParentChildService();
            Permissions_S = new PermissionsService();

        }
        #endregion

        #region Actions
        [HttpPost]
        [Route("GetDocumentsList")]
        public async Task<IActionResult> GetDocumentsList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.GetDocumentsList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetDocumentsByID/{DocumentId}")]
        public async Task<IActionResult> GetDocumentsByID([FromRoute] int DocumentId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.GetDocumentsByID(DocumentId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }



        [HttpGet]
        [Route("SearchDocumentByName/{Name}")]

        public async Task<IActionResult> SearchDocumentByName([FromRoute] string Name, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.SearchDocumentByName(Name, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddDocument")]
        public async Task<IActionResult> AddDocument([FromForm] DocumentModelView Document_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.AddDocument(Document_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("EditDocument")]
        public async Task<IActionResult> EditDocument([FromForm] DocumentModelView Document_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.EditDocument(Document_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("ViewDocumentMetadata/{DocumentId}")]
        public async Task<IActionResult> ViewDocumentMetadata([FromRoute] int DocumentId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.ViewDocumentMetadata(DocumentId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("MoveDocumentToNewFolder")]
        public async Task<IActionResult> MoveDocumentToNewFolder([FromBody] MoveChildToNewFolderModelView MoveChildToNewFolder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.MoveChildToNewFolder((int)GlobalService.ClassType.Folder, (int)GlobalService.ClassType.Document, MoveChildToNewFolder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetChildInDocumentByDocumentIdWithPermessions")]
        public async Task<IActionResult> GetChildInParentByIdWithPermessions([FromBody] FolderChildsPermissionsModelView FolderChildsPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetChildsInParentWithPermissions((int)GlobalService.ClassType.Document, FolderChildsPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetPermissionsOnDocumentByDocumentId/{DocumentId}")]
        public async Task<IActionResult> GetPermissionsOnObjectByObjectId([FromRoute] int DocumentId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetPermissionsOnObjectByObjectId((int)GlobalService.ClassType.Document, DocumentId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddPermissionsOnDocument")]
        public async Task<IActionResult> AddPermissionsOnObject([FromBody] AddPermissionsModelView AddPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.AddPermissionsOnObject(AddPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditPermissionsOnDocument")]
        public async Task<IActionResult> EditPermissionsOnObject([FromBody] EditPermissionsModelView EditPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.EditPermissionsOnObject(EditPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetUsersOrGroupsHavePermissionOnDocument/{DocumentId}")]
        public async Task<IActionResult> GetUsersOrGroupsHavePermissionOnObject([FromRoute] int DocumentId, [FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetUsersOrGroupsHavePermissionOnObject(DocumentId, Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }


        [HttpPost]
        [Route("GetUsersOrGroupsNotHavePermissionOnDocument/{DocumentId}")]
        public async Task<IActionResult> GetUsersOrGroupsNotHavePermissionOnObject([FromRoute] int DocumentId, [FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Permissions_S.GetUsersOrGroupsNotHavePermissionOnObject(DocumentId, Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}
