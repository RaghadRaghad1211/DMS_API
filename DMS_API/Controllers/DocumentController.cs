using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public IWebHostEnvironment Environment { get; }
        #endregion

        #region Constructor
        public DocumentController(IWebHostEnvironment environment)
        {
            Environment = environment;
            Document_S = new DocumentService(environment);
            LinkParentChild_S = new LinkParentChildService();

            //var ff = HelpService.GetDocumentLocationInServerFolder(1259843, Environment).Result;

            //var feef = HelpService.CreateDocumentFolderInServerFolder(1259843, Environment).Result;
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
        public async Task<IActionResult> AddDocument([FromBody] DocumentModelView Document_MV, [FromForm] DocumentFileModelView DocumentFile_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.AddDocument(Document_MV, DocumentFile_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("EditDocument")]
        public async Task<IActionResult> EditDocument([FromBody] DocumentModelView Document_MV, [FromForm] DocumentFileModelView DocumentFile_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.EditDocument(Document_MV, DocumentFile_MV, RequestHeader);
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
