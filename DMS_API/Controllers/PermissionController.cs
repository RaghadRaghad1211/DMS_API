﻿using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        #region Properteis
        private readonly PermissionsService Permissions_S;
        private ResponseModelView Response_MV { get; set; }
        private IWebHostEnvironment Environment { get; }

        #endregion

        #region Constructor
        public PermissionController(IWebHostEnvironment environment)
        {
            Environment = environment;
            Permissions_S = new PermissionsService(Environment);
        }
        #endregion

        #region Actions
        [HttpPost]
        [Route("GetChildInFolderByFolderIdWithPermessions")]
        public async Task<IActionResult> GetChildInFolderByIdWithPermessions([FromBody] ParentChildsPermissionsModelView FolderChildsPermissions_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (FolderChildsPermissions_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.GetChildsInParentWithPermissions(FolderChildsPermissions_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetChildInFolderByFolderIdWithPermessions_Search")]
        public async Task<IActionResult> GetChildInFolderByIdWithPermessions_Search([FromBody] ParentChildsPermissionsSearchModelView FolderChildsPermissionsSearch_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (FolderChildsPermissionsSearch_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.GetChildsInParentWithPermissions_Search(FolderChildsPermissionsSearch_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetPermissionsOnFolderByFolderId/{FolderId}")]
        public async Task<IActionResult> GetPermissionsOnObjectByObjectId([FromRoute] int FolderId, [FromHeader] RequestHeaderModelView RequestHeader)
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
            Response_MV = await Permissions_S.GetPermissionsOnObjectByObjectId(FolderId, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddPermissionsOnObject")]
        public async Task<IActionResult> AddPermissionsOnObject([FromBody] List<AddPermissionsModelView> AddPermissions_MVlist, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (AddPermissions_MVlist.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.AddPermissionsOnObject(AddPermissions_MVlist, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditPermissionsOnObject")]
        public async Task<IActionResult> EditPermissionsOnObject([FromBody] List<EditPermissionsModelView> EditPermissions_MVlist, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (EditPermissions_MVlist.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.EditPermissionsOnObject(EditPermissions_MVlist, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetUsersOrGroupsHavePermissionOnObject")]
        public async Task<IActionResult> GetUsersOrGroupsHavePermissionOnObject([FromBody] SearchUsersOrGroupsPermissionOnObject SearchUsersOrGroupsPermissionOnObject_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (SearchUsersOrGroupsPermissionOnObject_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.GetUsersOrGroupsHavePermissionOnObject(SearchUsersOrGroupsPermissionOnObject_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetUsersOrGroupsNotHavePermissionOnObject")]
        public async Task<IActionResult> GetUsersOrGroupsNotHavePermissionOnObject([FromBody] SearchUsersOrGroupsPermissionOnObject SearchUsersOrGroupsPermissionOnObject_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (SearchUsersOrGroupsPermissionOnObject_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.GetUsersOrGroupsNotHavePermissionOnObject(SearchUsersOrGroupsPermissionOnObject_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GenerateQRcodePDFofDocument")]
        public async Task<IActionResult> GenerateQRcodePDFofDocument([FromBody] QRLookupModel QRLookup_M, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            if (QRLookup_M.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.GenerateQRcodePDFofDocument(QRLookup_M, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("ReadQRcodePDFofDocument/{QRcode}")]
        public async Task<IActionResult> ReadQRcodePDFofDocument([FromRoute] string QRcode, [FromHeader] string? Lang = "Ar")
        {
            if (QRcode.IsSqlInjection())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.ReadQRcodePDFofDocument(QRcode, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("ReadQRcodePDFofDocumentPrivate/{QRcode}")]
        public async Task<IActionResult> ReadQRcodePDFofDocumentPrivate([FromRoute] string QRcode, [FromBody] LoginModelView Login_MV, [FromHeader] string? Lang = "Ar")
        {
            if (QRcode.IsSqlInjection())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            if (Login_MV.IsSqlInjectionList())
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.SqlInjection],
                    Data = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity).StatusCode
                };
                return UnprocessableEntity(Response_MV);
            }
            Response_MV = await Permissions_S.ReadQRcodePDFofDocumentPrivate(QRcode, Login_MV, Lang);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("CheckConnectionNetwork")]
        public async Task<IActionResult> CheckConnectionNetwork([FromHeader] string? Lang = "Ar")
        {
            string GSCOM = "NDC" + "© " + DateTime.Now;
            DataAccessService dam = new DataAccessService(SecurityService.ConnectionString);
            var connection = await Task.Run(() => dam.CheckConnectionNetwork());
            return connection == true ? Ok(new { Success = true, Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ServiceAvailable] + $" {GSCOM}" }) :
                                        NotFound(new { Success = false, Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ServiceUnavailable] + $" {GSCOM}" });
        }
        #endregion
    }
}