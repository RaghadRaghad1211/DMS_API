using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        #region Properteis
        private DocumentService Document_S;
        private readonly LinkParentChildService LinkParentChild_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public DocumentController()
        {
            Document_S = new DocumentService();
            LinkParentChild_S = new LinkParentChildService();
        }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost]
        [Route("GetDocumentsList")]
        public async Task<IActionResult> GetDocumentsList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.GetDocumentsList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDocumentsByID/{DocumentId}")]
        public async Task<IActionResult> GetDocumentsByID([FromRoute] int DocumentId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.GetDocumentsByID(DocumentId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("SearchDocumentByName/{Name}")]

        public async Task<IActionResult> SearchDocumentByName([FromRoute] string Name, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.SearchDocumentByName(Name, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddDocument")]
        public async Task<IActionResult> AddDocument([FromBody] DocumentModelView Document_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.AddDocument(Document_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }











        [AllowAnonymous]
        [HttpPut]
        [Route("MoveDocumentToNewFolder")]
        public async Task<IActionResult> MoveDocumentToNewFolder([FromBody] MoveChildToNewFolderModelView MoveChildToNewFolder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.MoveChildToNewFolder((int)HelpService.ClassType.Folder, (int)HelpService.ClassType.Document, MoveChildToNewFolder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}
