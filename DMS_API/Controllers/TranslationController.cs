using DMS_API.Models;
using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        #region Properteis
        private readonly TranslationService Translation_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public TranslationController()
        {
            Translation_S = new TranslationService();
        }
        #endregion

        #region Actions
        [HttpPost]
        [Route("GetTranslationList")]
        public async Task<IActionResult> GetTranslationList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (Pagination_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Translation_S.GetTranslationList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetTranslationByID/{id}")]
        public async Task<IActionResult> GetTranslationByID([FromRoute] int id, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (id.ToString().IsSqlInjection())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Translation_S.GetTranslationByID(id, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddTranslationWords")]
        public async Task<IActionResult> AddTranslationWords([FromBody] TranslationModel Translation_M, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (Translation_M.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Translation_S.AddTranslationWords(Translation_M, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditTranslationWords")]
        public async Task<IActionResult> EditTranslationWords([FromBody] TranslationModel Translation_M, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (Translation_M.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Translation_S.EditTranslationWords(Translation_M, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("SearchTranslationWords")]
        public async Task<IActionResult> SearchTranslationWords([FromBody] SearchTranslationModelView SearchTranslation_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (SearchTranslation_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Translation_S.SearchTranslationWords(SearchTranslation_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetTranslationPage")]
        public async Task<IActionResult> GetTranslationPage([FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Translation_S.GetTranslationPage(Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}