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
        private ResponseModelView Response_MV { get; set; }
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
            Response_MV = await User_S.Login(Login_MV, Lang);           
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetUsersList")]
        public async Task<IActionResult> GetUsersList([FromBody] PaginationModelView Pagination_MV, [FromHeader] string? Token, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await User_S.GetUsersList(Pagination_MV, Token, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetUsersByID/{id}")]
        public async Task<IActionResult> GetUsersByID([FromRoute] int id, [FromHeader] string? Token, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await User_S.GetUsersByID(id, Token, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
    }
}