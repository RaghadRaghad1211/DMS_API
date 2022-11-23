using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;

namespace DMS_API.Services
{
    public class FolderService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private GroupOrFolderModel Folder_M { get; set; }
        private List<GroupOrFolderModel> Folder_Mlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        private const int ClassID = 4; // Folder
        #endregion

        #region Constructor        
        public FolderService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region CURD Functions
        public async Task<ResponseModelView> GetFoldersList(PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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
                    int _PageNumber = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                    int _PageRows = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
                    int CurrentPage = _PageNumber;

                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                    string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                    var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                             $"FROM [User].V_GroupsFolders  WHERE [OrgOwner] IN ({whereField} FROM [User].[GetOrgsbyUserId]({userLoginID})) AND ObjClsId ={ClassID} ");
                    if (MaxTotal == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError],
                            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        if (MaxTotal.Rows.Count == 0)
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
                            string getFolderInfo = "SELECT  ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, ObjCreationDate, ObjIsDesktopFolder, ObjDescription, UserOwnerID, " +
                                                 "            OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                                 "FROM            [User].V_GroupsFolders " +
                                                $"WHERE [OrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} " +
                                                 "ORDER BY ObjId " +
                                                $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                $"FETCH NEXT   {_PageRows} ROWS ONLY ";

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
                            Folder_Mlist = new List<GroupOrFolderModel>();
                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    Folder_M = new GroupOrFolderModel
                                    {
                                        ObjId = Convert.ToInt32(dt.Rows[i]["ObjId"].ToString()),
                                        ObjTitle = dt.Rows[i]["ObjTitle"].ToString(),
                                        ObjClsId = Convert.ToInt32(dt.Rows[i]["ObjClsId"].ToString()),
                                        ClsName = dt.Rows[i]["ClsName"].ToString(),
                                        ObjIsActive = bool.Parse(dt.Rows[i]["ObjIsActive"].ToString()),
                                        ObjCreationDate = DateTime.Parse(dt.Rows[i]["ObjCreationDate"].ToString()),
                                        ObjIsDesktopFolder = bool.Parse(dt.Rows[i]["ObjIsDesktopFolder"].ToString()),
                                        ObjDescription = dt.Rows[i]["ObjDescription"].ToString(),
                                        UserOwnerID = Convert.ToInt32(dt.Rows[i]["UserOwnerID"].ToString()),
                                        OwnerFullName = dt.Rows[i]["OwnerFullName"].ToString(),
                                        OwnerUserName = dt.Rows[i]["OwnerUserName"].ToString(),
                                        OrgOwner = dt.Rows[i]["OrgOwner"].ToString(),
                                        OrgEnName = dt.Rows[i]["OrgEnName"].ToString(),
                                        OrgArName = dt.Rows[i]["OrgArName"].ToString(),
                                        OrgKuName = dt.Rows[i]["OrgKuName"].ToString(),

                                    };
                                    Folder_Mlist.Add(Folder_M);
                                }

                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Folder_Mlist }
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
        public async Task<ResponseModelView> GetFoldersByID(int id, RequestHeaderModelView RequestHeader)
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
                    int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                    //string whereField = orgOwnerID == 0 ? "OrgUp" : "OrgId";
                    string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                    string getFolderInfo = "SELECT  ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, ObjCreationDate, ObjIsDesktopFolder, ObjDescription, UserOwnerID, " +
                                                     "            OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                                     "FROM            [User].V_GroupsFolders " +
                                                    $"WHERE ObjId={id} AND [OrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} ";


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
                    Folder_Mlist = new List<GroupOrFolderModel>();
                    if (dt.Rows.Count > 0)
                    {
                        Folder_M = new GroupOrFolderModel
                        {
                            ObjId = Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()),
                            ObjTitle = dt.Rows[0]["ObjTitle"].ToString(),
                            ObjClsId = Convert.ToInt32(dt.Rows[0]["ObjClsId"].ToString()),
                            ClsName = dt.Rows[0]["ClsName"].ToString(),
                            ObjIsActive = bool.Parse(dt.Rows[0]["ObjIsActive"].ToString()),
                            ObjCreationDate = DateTime.Parse(dt.Rows[0]["ObjCreationDate"].ToString()),
                            ObjIsDesktopFolder = bool.Parse(dt.Rows[0]["ObjIsDesktopFolder"].ToString()),
                            ObjDescription = dt.Rows[0]["ObjDescription"].ToString(),
                            UserOwnerID = Convert.ToInt32(dt.Rows[0]["UserOwnerID"].ToString()),
                            OwnerFullName = dt.Rows[0]["OwnerFullName"].ToString(),
                            OwnerUserName = dt.Rows[0]["OwnerUserName"].ToString(),
                            OrgOwner = dt.Rows[0]["OrgOwner"].ToString(),
                            OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                            OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                            OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
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
                    if (ValidationService.IsEmpty(Folder_MV.FolderTitle) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FolderNameMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [Main].[Objects] WHERE ObjTitle = '{Folder_MV.FolderTitle}' AND " +
                                                                         $"[ObjOrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} "));
                        if (checkDeblicate == 0)
                        {
                            string exeut = $"EXEC [User].[AddGroupOrFolderPro] '{ClassID}','{Folder_MV.FolderTitle}', '{userLoginID}', '{Folder_MV.FolderOrgOwnerID}','{Folder_MV.IsDesktopFolder}', '{Folder_MV.FolderDescription}' ";
                            var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));

                            if (outValue == null || outValue.Trim() == "")
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
                                if (outValue == 0.ToString())
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
                    if (ValidationService.IsEmpty(Folder_MV.FolderTitle) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GroupNameMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        //int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        //int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        //string whereField = orgOwnerID == 0 ? "OrgUp" : "OrgId";

                        //int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [Main].[Objects] WHERE ObjTitle = '{Folder_MV.FolderTitle}' AND " +
                        //                                                 $"[ObjOrgOwner] IN (SELECT {whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} "));
                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].[V_GroupsFolders] WHERE ObjId = {Folder_MV.FolderId} AND " +
                                                                         $"ObjClsId = {ClassID} "));

                        if (checkDeblicate > 0)
                        {
                            string exeut = $"EXEC [User].[UpdateGroupOrFolderPro] '{Folder_MV.FolderId}','{Folder_MV.FolderTitle}', '{Folder_MV.FolderIsActive}', '{Folder_MV.IsDesktopFolder}','{Folder_MV.FolderDescription}' ";
                            var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));

                            if (outValue == null || outValue.Trim() == "")
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
                                if (outValue == 0.ToString())
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
        public async Task<ResponseModelView> SearchFolderByName(string Name, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(Name) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FolderNameMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                        string getfolderInfo = "SELECT  ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, ObjCreationDate, ObjIsDesktopFolder, ObjDescription, UserOwnerID, " +
                                                     "            OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                                     "FROM            [User].V_GroupsFolders " +
                                                  $"WHERE  ObjTitle LIKE '{Name}%' AND [OrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} ";


                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getfolderInfo));
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
                        Folder_Mlist = new List<GroupOrFolderModel>();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                Folder_M = new GroupOrFolderModel
                                {
                                    ObjId = Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()),
                                    ObjTitle = dt.Rows[0]["ObjTitle"].ToString(),
                                    ObjClsId = Convert.ToInt32(dt.Rows[0]["ObjClsId"].ToString()),
                                    ClsName = dt.Rows[0]["ClsName"].ToString(),
                                    ObjIsActive = bool.Parse(dt.Rows[0]["ObjIsActive"].ToString()),
                                    ObjCreationDate = DateTime.Parse(dt.Rows[0]["ObjCreationDate"].ToString()),
                                    ObjIsDesktopFolder = bool.Parse(dt.Rows[0]["ObjIsDesktopFolder"].ToString()),
                                    ObjDescription = dt.Rows[0]["ObjDescription"].ToString(),
                                    UserOwnerID = Convert.ToInt32(dt.Rows[0]["UserOwnerID"].ToString()),
                                    OwnerFullName = dt.Rows[0]["OwnerFullName"].ToString(),
                                    OwnerUserName = dt.Rows[0]["OwnerUserName"].ToString(),
                                    OrgOwner = dt.Rows[0]["OrgOwner"].ToString(),
                                    OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                                    OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                                    OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                                };
                                Folder_Mlist.Add(Folder_M);
                            }

                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = Folder_Mlist
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
