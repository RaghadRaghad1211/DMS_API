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

        public TranslationController()
        {
            Translation_S = new TranslationService();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetTranslationList/{lang}")]
        public async Task<IActionResult> GetTranslationList(string Lang)
        {
            response_MV = await Translation_S.GetTranslationList(Lang);
            return Ok(response_MV);
            //return statistics_Mlist != null ? Ok(statistics_Mlist) :
            //    NotFound(response_S = new ResponseService { ResponseBool = false, ResponseString = "No Found Data" });

        }
    }
}
