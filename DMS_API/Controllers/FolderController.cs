using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        #region Properteis
        private readonly FolderService Folder_S;
        private readonly LinkParentChildService LinkParentChild_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public FolderController()
        {
            Folder_S = new FolderService();
            LinkParentChild_S = new LinkParentChildService();
        }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost]
        [Route("GetFoldersList")]
        public async Task<IActionResult> GetFoldersList([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.GetFoldersList(Pagination_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetFoldersByID/{FolderId}")]
        public async Task<IActionResult> GetFoldersByID([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.GetFoldersByID(FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddFolder")]
        public async Task<IActionResult> AddFolder([FromBody] FolderModelView Folder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.AddFolder(Folder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("EditFolder")]
        public async Task<IActionResult> EditGroup([FromBody] FolderModelView Folder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.EditFolder(Folder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SearchFolderByName/{Name}")]
        public async Task<IActionResult> SearchFolderByName([FromRoute] string Name, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Folder_S.SearchFolderByName(Name, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetChildsInFolderByID/{FolderId}")]
        public async Task<IActionResult> GetChildsInFolderByID([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildInParentByID((int)HelpService.ClassType.Folder, FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetChildsInFolderByID_Search")]
        public async Task<IActionResult> GetChildsInFolderByID_Search([FromBody] SearchChildParentModelView SearchChildFolder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.GetChildInParentByID_Search((int)HelpService.ClassType.Folder, SearchChildFolder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("GetChildsNotInFolderByID/{FolderId}")]
        //public async Task<IActionResult> GetChildsNotInFolderByID([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        //{
        //    Response_MV = await LinkParentChild_S.GetChildNotInGroupByID(FolderId, RequestHeader);
        //    return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //[Route("GetChildsNotInFolderByID_Search")]
        //public async Task<IActionResult> GetChildsNotInFolderByID_Search([FromBody] SearchChildParentModelView SearchChildFolder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        //{
        //    Response_MV = await LinkParentChild_S.GetChildNotInGroupByID_Search(SearchChildFolder_MV, RequestHeader);
        //    return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        //}

        [AllowAnonymous]
        [HttpPost]
        [Route("AddChildsIntoFolder")]
        public async Task<IActionResult> AddChildsIntoFolder([FromBody] LinkParentChildModelView LinkParentChild_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.AddChildIntoParent((int)HelpService.ClassType.Folder, LinkParentChild_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("RemoveChildsFromFolder")]
        public async Task<IActionResult> RemoveChildsFromFolder([FromBody] LinkParentChildModelView LinkParentChild_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await LinkParentChild_S.RemoveChildFromParent((int)HelpService.ClassType.Folder, LinkParentChild_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}
