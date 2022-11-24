using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        #endregion

        #region Constructor
        public DocumentController()
        {
            Document_S = new DocumentService();
        }
        #endregion

        #region Actions









        [AllowAnonymous]
        [HttpGet]
        [Route("GeneralSearchByTitle/{title}")]
        public async Task<IActionResult> GeneralSearchByTitle([FromRoute] string title, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await HelpService.GeneralSearchByTitle(title, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}
