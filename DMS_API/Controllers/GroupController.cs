﻿using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        #region Properteis
        private GroupService Group_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public GroupController()
        {
            Group_S = new GroupService();
        }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost]
        [Route("GetGroupsList")]
        public async Task<IActionResult> GetGroupsList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.GetGroupsList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetGroupsByID/{id}")]
        public async Task<IActionResult> GetGroupsByID([FromRoute] int id, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.GetGroupsByID(id, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}