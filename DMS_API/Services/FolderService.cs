using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;
namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Folders
    /// </summary>
    public class FolderService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private FolderModel Folder_M { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public FolderService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Everyone To Do:
        /// Add Folder in Folder
        /// </summary>
        /// <param name="Folder_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> AddFolder(FolderModelView Folder_MV, RequestHeaderModelView RequestHeader)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(RequestHeader);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
                {
                    if (ValidationService.IsEmpty(Folder_MV.FolderTitle) == true || string.IsNullOrEmpty(Folder_MV.FolderTitle))
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FolderTitleMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].[V_Folders] WHERE ObjTitle = '{Folder_MV.FolderTitle}' AND " +
                                                                         $"ObjId IN (SELECT LcChildObjId FROM [Main].[GetChildsInParent]({Folder_MV.FolderPerantId},{(int)GlobalService.ClassType.Folder})) AND ObjClsId ={(int)GlobalService.ClassType.Folder} AND ObjIsActive=1 "));
                        if (checkDeblicate == 0)
                        {
                            string exeut = $"EXEC [User].[AddFolderPro] '{(int)GlobalService.ClassType.Folder}','{Folder_MV.FolderTitle}', '{userLoginID}', '{Folder_MV.FolderOrgOwnerID}', '{Folder_MV.FolderDescription}', '{Folder_MV.FolderPerantId}' ";
                            var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));

                            if (outValue == null || outValue.Trim() == "" || outValue == 0.ToString())
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.InsertFaild],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.InsertSuccess],
                                    Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                                };
                                return Response_MV;
                            }
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = Folder_MV.FolderTitle + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }
        /// <summary>
        /// Users Have IsWeite & Admins To Do:
        /// Edit Folder in Folder
        /// </summary>
        /// <param name="Folder_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> EditFolder(FolderModelView Folder_MV, RequestHeaderModelView RequestHeader)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(RequestHeader);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
                {
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, Folder_MV.FolderId).Result;
                    bool checkManagePermission = result == null ? false : result.IsWrite;
                    if (checkManagePermission == true)
                    {
                        if (ValidationService.IsEmpty(Folder_MV.FolderTitle) == true)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FolderTitleMustEnter],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            int isDesktopFolder = Convert.ToInt32(dam.FireSQL("SELECT   COUNT(*)     FROM   [User].[V_Folders] " +
                                                                           $"WHERE    ObjId={Folder_MV.FolderId} AND ObjIsDesktopFolder = 1 "));
                            if (isDesktopFolder > 0)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FolderUnEditable],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT  COUNT(*)     FROM    [User].[V_Folders] " +
                                                                                 $"WHERE   ObjTitle = '{Folder_MV.FolderTitle}' AND " +
                                                                                 $"        ObjId IN (SELECT LcChildObjId FROM [Main].[GetChildsInParent]({Folder_MV.FolderPerantId},{(int)GlobalService.ClassType.Folder})) AND " +
                                                                                 $"        ObjClsId ={(int)GlobalService.ClassType.Folder} AND ObjIsActive=1 "));
                                if (checkDeblicate == 0)
                                {
                                    string exeut = $"EXEC [User].[UpdateFolderPro] '{Folder_MV.FolderId}','{Folder_MV.FolderTitle}', '1', '{Folder_MV.FolderDescription}', '{Folder_MV.IsFavoriteFolder}' ";
                                    var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));

                                    if (outValue == null || outValue.Trim() == "" || outValue == 0.ToString())
                                    {
                                        Response_MV = new ResponseModelView
                                        {
                                            Success = false,
                                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.UpdateFaild],
                                            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                                        };
                                        return Response_MV;
                                    }
                                    else
                                    {
                                        Response_MV = new ResponseModelView
                                        {
                                            Success = true,
                                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.UpdateSuccess],
                                            Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                                        };
                                        return Response_MV;
                                    }
                                }
                                else
                                {
                                    Response_MV = new ResponseModelView
                                    {
                                        Success = false,
                                        Message = Folder_MV.FolderTitle + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
                                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                    };
                                    return Response_MV;
                                }
                            }
                        }
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                }
            }
            catch (Exception ex)
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;

            }
        }
        /// <summary>
        /// Users Have IsRead & Admins To Do:
        /// View Folder info by folder Id
        /// </summary>
        /// <param name="FolderId">ID of Folder that neet to view</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetFolderById(int FolderId, RequestHeaderModelView RequestHeader)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(RequestHeader);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
                {
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, FolderId).Result;
                    bool checkManagePermission = result == null ? false : result.IsRead;
                    if (checkManagePermission == true)
                    {
                        int CheckActivation = int.Parse(dam.FireSQL($"SELECT COUNT(*) FROM [User].V_Folders WHERE ObjId={FolderId} AND ObjIsActive=1"));
                        if (CheckActivation == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            string getFolderInfo = "SELECT   ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, CONVERT(DATE,ObjCreationDate,104) AS ObjCreationDate, " +
                                                   "         ObjDescription, UserOwnerID, OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                                  $"FROM    [User].V_Folders    WHERE   ObjId={FolderId} ";
                            dt = new DataTable();
                            dt = await Task.Run(() => dam.FireDataTable(getFolderInfo));
                            if (dt == null)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError],
                                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                                };
                                return Response_MV;
                            }
                            if (dt.Rows.Count > 0)
                            {
                                Folder_M = new FolderModel
                                {
                                    ObjId = Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()),
                                    ObjTitle = dt.Rows[0]["ObjTitle"].ToString(),
                                    ObjClsId = Convert.ToInt32(dt.Rows[0]["ObjClsId"].ToString()),
                                    ClsName = dt.Rows[0]["ClsName"].ToString(),
                                    ObjIsActive = bool.Parse(dt.Rows[0]["ObjIsActive"].ToString()),
                                    ObjCreationDate = DateTime.Parse(dt.Rows[0]["ObjCreationDate"].ToString()).ToShortDateString(),
                                    ObjDescription = dt.Rows[0]["ObjDescription"].ToString(),
                                    UserOwnerID = Convert.ToInt32(dt.Rows[0]["UserOwnerID"].ToString()),
                                    OwnerFullName = dt.Rows[0]["OwnerFullName"].ToString(),
                                    OwnerUserName = dt.Rows[0]["OwnerUserName"].ToString(),
                                    OrgOwner = dt.Rows[0]["OrgOwner"].ToString(),
                                    OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                                    OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                                    OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                                    IsFavoriteFolder = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].[V_Favourites] WHERE [ObjUserId] ={userLoginID} AND " +
                                                                                   $"[ObjFavId]={FolderId} AND [IsActive] = 1 ")) > 0 ? true : false,
                                };
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                    Data = Folder_M
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                                };
                                return Response_MV;
                            }
                        }
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                }
            }
            catch (Exception ex)
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }
        #endregion
    }
}