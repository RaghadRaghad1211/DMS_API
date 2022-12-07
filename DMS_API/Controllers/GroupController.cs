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
        [Route("GetGroupsByID/{GroupId}")]
        public async Task<IActionResult> GetGroupsByID([FromRoute] int GroupId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.GetGroupsByID(GroupId, RequestHeader);
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
        [HttpPost]
        [Route("SearchGroupByName/{Name}")]
        public async Task<IActionResult> SearchGroupByName([FromRoute] string Name, [FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.SearchGroupByName(Name, Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetChildsInGroupByID/{GroupId}")]
        public async Task<IActionResult> GetChildsInGroupByID([FromRoute] int GroupId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildInParentByID((int)HelpService.ClassType.Group, GroupId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetChildsInGroupByID_Search")]
        public async Task<IActionResult> GetChildsInGroupByID_Search([FromBody] SearchChildParentModelView SearchChildGroup_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildInParentByID_Search((int)HelpService.ClassType.Group, SearchChildGroup_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }





        [AllowAnonymous]
        [HttpGet]
        [Route("GetChildsNotInGroupByID/{GroupId}")]
        public async Task<IActionResult> GetChildsNotInGroupByID([FromRoute] int GroupId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildNotInGroupByID((int)HelpService.ClassType.Group, GroupId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetChildsNotInGroupByID_Search")]
        public async Task<IActionResult> GetChildsNotInGroupByID_Search([FromBody] SearchChildParentModelView SearchChildGroup_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildNotInGroupByID_Search((int)HelpService.ClassType.Group, SearchChildGroup_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }










        [AllowAnonymous]
        [HttpPost]
        [Route("AddChildsIntoGroup")]
        public async Task<IActionResult> AddChildsIntoGroup([FromBody] LinkParentChildModelView LinkParentChild_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.AddChildIntoParent((int)HelpService.ClassType.Group, LinkParentChild_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("RemoveChildsFromGroup")]
        public async Task<IActionResult> RemoveChildsFromGroup([FromBody] LinkParentChildModelView LinkParentChild_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.RemoveChildFromParent((int)HelpService.ClassType.Group, LinkParentChild_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}
