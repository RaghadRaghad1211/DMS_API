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
        [HttpPost]
        [Route("GetGroupsList")]
        public async Task<IActionResult> GetGroupsList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.GetGroupsList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetGroupById/{GroupId}")]
        public async Task<IActionResult> GetGroupById([FromRoute] int GroupId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.GetGroupById(GroupId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddGroup")]
        public async Task<IActionResult> AddGroup([FromBody] GroupModelView Group_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.AddGroup(Group_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditGroup")]
        public async Task<IActionResult> EditGroup([FromBody] GroupModelView Group_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.EditGroup(Group_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("SearchGroupByName/{GroupName}")]
        public async Task<IActionResult> SearchGroupByName([FromRoute] string GroupName, [FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Group_S.SearchGroupByName(GroupName, Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetChildsInGroupByID/{GroupId}")]
        public async Task<IActionResult> GetChildsInGroupByGroupId([FromRoute] int GroupId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildsInGroupByGroupId( GroupId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetChildsInGroupByID_Search")]
        public async Task<IActionResult> GetChildsInGroupByGroupId_Search([FromBody] SearchChildsOfGroupModelView SearchChildGroup_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildsInGroupByGroupId_Search(SearchChildGroup_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetChildsNotInGroupByID/{GroupId}")]
        public async Task<IActionResult> GetChildNotInGroupByGroupId([FromRoute] int GroupId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildNotInGroupByGroupId(GroupId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetChildsNotInGroupByID_Search")]
        public async Task<IActionResult> GetChildNotInGroupByGroupId_Search([FromBody] SearchChildsOfGroupModelView SearchChildGroup_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildNotInGroupByGroupId_Search(SearchChildGroup_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddChildsIntoGroup")]
        public async Task<IActionResult> AddChildsIntoGroup([FromBody] LinkGroupChildsModelView LinkGroupChilds_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.AddChildsIntoGroup(LinkGroupChilds_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("RemoveChildsFromGroup")]
        public async Task<IActionResult> RemoveChildsFromGroup([FromBody] LinkGroupChildsModelView LinkGroupChilds_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.RemoveChildsFromGroup(LinkGroupChilds_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}