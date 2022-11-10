using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        #region Properteis
        private UserService User_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public UserController()
        {
            User_S = new UserService();
        }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModelView Login_MV, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await User_S.Login(Login_MV, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ResetPassword/{id}")]
        public async Task<IActionResult> ResetPassword([FromRoute] int id, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.ResetPassword(id, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModelView ChangePassword_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.ChangePassword(ChangePassword_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetUsersList")]
        public async Task<IActionResult> GetUsersList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.GetUsersList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetUsersByID/{id}")]
        public async Task<IActionResult> GetUsersByID([FromRoute] int id, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.GetUsersByID(id, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] AddUserModelView AddUser_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.AddUser(AddUser_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser([FromBody] EditUserModelView EditUser_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.EditUser(EditUser_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("SearchUsersByUserName/{username}")]
        public async Task<IActionResult> SearchUsersByUserName([FromRoute] string username, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.SearchUsersByUserName(username, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SearchUsersAdvance")]
        public async Task<IActionResult> SearchUsersAdvance([FromBody] SearchUserModelView SearchUser_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.SearchUsersAdvance(SearchUser_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetOrgsParentWithChilds")]
        public async Task<IActionResult> GetOrgsParentWithChilds([FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.GetOrgsParentWithChilds(RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}