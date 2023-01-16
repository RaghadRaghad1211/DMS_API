using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        #region Properteis
        private DocumentService Document_S;
        private ResponseModelView Response_MV { get; set; }
        public IWebHostEnvironment Environment { get; }
        #endregion

        #region Constructor
        public DocumentController(IWebHostEnvironment environment)
        {
            Environment = environment;
            Document_S = new DocumentService(environment);
        }
        #endregion

        #region Actions
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
        public async Task<IActionResult> GetDocumentMetadata([FromRoute] int DocumentId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Document_S.GetDocumentMetadata(DocumentId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}