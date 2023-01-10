using ArchiveAPI.Services;
using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;

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
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModelView Login_MV, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await User_S.Login(Login_MV, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("ResetPasswordAdmin/{UserId}")]
        public async Task<IActionResult> ResetPasswordByAdmin([FromRoute] int UserId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.ResetPasswordByAdmin(UserId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("ResetPasswordUser/{UserName}")]
        public async Task<IActionResult> ResetPasswordByUser([FromRoute] string UserName, [FromHeader] string? Lang = "Ar")
        {
            Response_MV = await User_S.ResetPasswordByUser(UserName, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("ChangeMyPassword")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordModelView ChangePassword_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.ChangePassword(ChangePassword_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("EditMyContact")]
        public async Task<IActionResult> EditMyContact([FromBody] EditMyContactModelView EditMyContact_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.EditContact(EditMyContact_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetUsersList")]
        public async Task<IActionResult> GetUsersList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.GetUsersList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetUsersByID/{UserId}")]
        public async Task<IActionResult> GetUsersByID([FromRoute] int UserId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.GetUsersByID(UserId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] AddUserModelView AddUser_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.AddUser(AddUser_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser([FromBody] EditUserModelView EditUser_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.EditUser(EditUser_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("SearchUsersByUserName/{username}")]
        public async Task<IActionResult> SearchUsersByUserName([FromRoute] string username, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.SearchUsersByUserName(username, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("SearchUsersAdvance")]
        public async Task<IActionResult> SearchUsersAdvance([FromBody] SearchUserModelView SearchUser_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await User_S.SearchUsersAdvance(SearchUser_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion

        [HttpGet]
        [Route("TestGet")]
        public async Task<IActionResult> TestGet()
        {
            string GG = "GSCOM-NDC" + "©" + DateTime.Now.Year;
            DataAccessService dam = new DataAccessService(SecurityService.ConnectionString);
            return await Task.FromResult(Ok(dam.FireSQL("SELECT COUNT(*) FROM [User].Users") + $" {GG}"));
        }

    }
}