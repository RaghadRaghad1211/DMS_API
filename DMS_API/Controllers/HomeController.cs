using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        #region Properteis
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public HomeController()
        {
        }
        #endregion

        #region Actions

        [HttpPost]
        [Route("GetHomeData")]
        public async Task<IActionResult> GetHomeData([FromBody]PaginationHomeModelView PaginationHome_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (PaginationHome_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await GlobalService.GetHomeData(PaginationHome_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GeneralSearchByTitle/{title}")]
        public async Task<IActionResult> GeneralSearchByTitle([FromRoute] string title, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (title.IsSqlInjection())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await GlobalService.GeneralSearchByTitle(title, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}