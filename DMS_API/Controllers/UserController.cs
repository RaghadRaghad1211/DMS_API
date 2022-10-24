using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private UserService User_S;
        //private LoginModelView Login_MV { get; set; }
        private ResponseModelView response_MV { get; set; }
        // private TranslationModel Translation_M { get; set; }

        public UserController()
        {
            User_S = new UserService();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody]LoginModelView Login_MV, [FromHeader]string? Lang="Ar")
        {
            response_MV = await User_S.Login(Login_MV, Lang);           
            return response_MV.Success == true ? Ok(response_MV) : StatusCode((int)response_MV.Data, response_MV);
        }
    }
}