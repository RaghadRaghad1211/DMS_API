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
        private TranslationService Translation_S; 
        private ResponseModelView response_MV { get; set; }
       // private TranslationModel Translation_M { get; set; }

        public TranslationController()
        {
            Translation_S = new TranslationService();

            var ff = SecurityService.PasswordEnecrypt("$ystemAdm!n");// $ystemAdm!n
            //var ff1 = SecurityService.PasswordEnecrypt1("$ystemAdm!n");// $ystemAdm!n
            //var ff2 = SecurityService.PasswordEnecrypt2("$ystemAdm!n");// $ystemAdm!n
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetTranslationList/{Lang}")]
        public async Task<IActionResult> GetTranslationList([FromRoute]string Lang="Ar")
        {
            response_MV = await Translation_S.GetTranslationList(Lang);
            return response_MV.Success == true ? Ok(response_MV) : NotFound(response_MV);

            //return statistics_Mlist != null ? Ok(statistics_Mlist) :
            //    NotFound(response_S = new ResponseService { ResponseBool = false, ResponseString = "No Found Data" });

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetTranslationByID/{id}/{Lang}")]
        public async Task<IActionResult> GetTranslationByID([FromRoute] int id,[FromRoute] string Lang = "Ar")
        {
            response_MV = await Translation_S.GetTranslationByID(id,Lang);
            return response_MV.Success == true ? Ok(response_MV) : NotFound(response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddTranslationWords/{Lang}")]
        public async Task<IActionResult> AddTranslationWords([FromBody] TranslationModel Translation_M, [FromRoute] string Lang = "Ar")
        {
            response_MV = await Translation_S.AddTranslationWords(Translation_M,Lang);
            return response_MV.Success == true ? Ok(response_MV) : NotFound(response_MV);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("EditTranslationWords/{Lang}")]
        public async Task<IActionResult> EditTranslationWords([FromBody] TranslationModel Translation_M, [FromRoute] string Lang = "Ar")
        {
            response_MV = await Translation_S.EditTranslationWords(Translation_M,Lang);
            return response_MV.Success == true ? Ok(response_MV) : NotFound(response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetTranslationPage/{Lang}")]
        public async Task<IActionResult> GetTranslationPage([FromRoute] string Lang = "Ar")
        {
            response_MV = await Translation_S.GetTranslationPage(Lang);
            return response_MV.Success == true ? Ok(response_MV) : NotFound(response_MV);
        }
    }
}
