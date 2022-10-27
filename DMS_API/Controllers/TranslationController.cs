using DMS_API.Models;
using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService Translation_S;
        private ResponseModelView Response_MV { get; set; }

        public TranslationController()
        {
            Translation_S = new TranslationService();
        }

        [AllowAnonymous]
        //[Authorize(Roles = "Administrator,User")]
        [HttpPost]
        [Route("GetTranslationList")]
        public async Task<IActionResult> GetTranslationList([FromBody] PaginationModelView Pagination_MV, [FromHeader] string Token, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Translation_S.GetTranslationList(Pagination_MV, Token, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
            //return response_MV.Success == true ? Ok(response_MV) : NotFound(response_MV);
        }

        [AllowAnonymous]
        //[Authorize(Roles = "Administrator,User")]
        [HttpGet]
        [Route("GetTranslationByID/{id}")]
        public async Task<IActionResult> GetTranslationByID([FromRoute] int id, [FromHeader] string Token, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Translation_S.GetTranslationByID(id, Token, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        //[Authorize(Roles = "Administrator,User")]
        [HttpPost]
        [Route("AddTranslationWords")]
        public async Task<IActionResult> AddTranslationWords([FromBody] TranslationModel Translation_M, [FromHeader] string Token, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Translation_S.AddTranslationWords(Translation_M, Token, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        //[Authorize(Roles = "Administrator,User")]
        [HttpPut]
        [Route("EditTranslationWords")]
        public async Task<IActionResult> EditTranslationWords([FromBody] TranslationModel Translation_M, [FromHeader] string Token, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Translation_S.EditTranslationWords(Translation_M, Token, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        //[Authorize(Roles = "Administrator,User")]
        [HttpPost]
        [Route("SearchTranslationWords")]
        public async Task<IActionResult> SearchTranslationWords([FromBody] SearchTranslationModelView SearchTranslation_MV, [FromHeader] string Token, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Translation_S.SearchTranslationWords(SearchTranslation_MV, Token, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        //[Authorize(Roles = "Administrator,User")]
        [HttpGet]
        [Route("GetTranslationPage")]
        public async Task<IActionResult> GetTranslationPage([FromHeader] string Token,[FromHeader] string? Lang = "Ar")
        {
            Response_MV = await Translation_S.GetTranslationPage(Token, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
    }
}
