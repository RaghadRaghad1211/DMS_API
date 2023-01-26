﻿using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        [HttpGet]
        [Route("GetFolderById/{FolderId}")]
        public async Task<IActionResult> GetFolderById([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (FolderId.ToString().IsSqlInjection())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Folder_S.GetFolderById(FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddFolder")]
        public async Task<IActionResult> AddFolder([FromBody] FolderModelView Folder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (Folder_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Folder_S.AddFolder(Folder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditFolder")]
        public async Task<IActionResult> EditGroup([FromBody] FolderModelView Folder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (Folder_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Folder_S.EditFolder(Folder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("RemoveChildsFromFolder")]
        public async Task<IActionResult> RemoveChildsFromFolder([FromBody] LinkFolderChildsModelView LinkFolderChilds_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (LinkFolderChilds_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await LinkParentChild_S.RemoveChildsFromFolder(LinkFolderChilds_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("MoveFolderToNewFolder")]
        public async Task<IActionResult> MoveFolderToNewFolder([FromBody] MoveChildToNewFolderModelView MoveChildToNewFolder_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (MoveChildToNewFolder_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await LinkParentChild_S.MoveChildsToNewFolder(MoveChildToNewFolder_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}