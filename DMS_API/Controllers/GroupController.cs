using DMS_API.ModelsView;
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
        private LinkParentChildService LinkParentChild_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public GroupController()
        {
            Group_S = new GroupService();
            LinkParentChild_S = new LinkParentChildService();
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

        [AllowAnonymous]
        [HttpPost]
        [Route("AddGroup")]
        public async Task<IActionResult> AddGroup([FromBody] GroupModelView Group_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.AddGroup(Group_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("EditGroup")]
        public async Task<IActionResult> EditGroup([FromBody] GroupModelView Group_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.EditGroup(Group_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SearchGroupByName/{Name}")]
        public async Task<IActionResult> SearchGroupByName([FromRoute] string Name, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.SearchGroupByName(Name, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetChildsLinkGroupByID/{GroupId}")]
        public async Task<IActionResult> GetChildsLinkGroupByID([FromRoute] int GroupId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildLinkParentByID((int)HelpService.ParentClass.Group, GroupId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddChildsIntoGroup")]
        public async Task<IActionResult> AddChildsIntoGroup([FromBody] LinkParentChildModelView LinkParentChild_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.AddChildIntoParent((int)HelpService.ParentClass.Group, LinkParentChild_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("RemoveChildsFromGroup")]
        public async Task<IActionResult> RemoveChildsFromGroup([FromBody] LinkParentChildModelView LinkParentChild_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.RemoveChildFromParent((int)HelpService.ParentClass.Group, LinkParentChild_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}
