using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;
namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Groups
    /// </summary>
    public class GroupService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private GroupModel Group_M { get; set; }
        private List<GroupModel> Group_Mlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public GroupService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Only Admins To Do:
        /// Get all Groups which depends on the Admin Orgnazation
        /// </summary>
        /// <param name="Pagination_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetGroupsList(PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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
                    if (((SessionModel)ResponseSession.Data).IsOrgAdmin == false && ((SessionModel)ResponseSession.Data).IsGroupOrgAdmin == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int _PageNumber = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                        int _PageRows = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
                        int CurrentPage = _PageNumber;

                        int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                        var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                          "FROM   [User].V_Groups  " +
                                                         $"WHERE  [OrgOwner] IN ({whereField} FROM [User].[GetOrgsbyUserId]({userLoginID})) AND " +
                                                         $"       ObjClsId ={(int)GlobalService.ClassType.Group} ");
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
                                string getGroupInfo = "SELECT    ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, CONVERT(DATE,ObjCreationDate,104) AS ObjCreationDate, " +
                                                      "          ObjDescription, UserOwnerID, OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                                      "FROM       [User].V_Groups " +
                                                     $"WHERE      [OrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND " +
                                                     $"           ObjClsId ={(int)GlobalService.ClassType.Group} " +
                                                      "ORDER BY   ObjId " +
                                                     $"OFFSET     ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                     $"FETCH NEXT  {_PageRows} ROWS ONLY ";

                                dt = new DataTable();
                                dt = await Task.Run(() => dam.FireDataTable(getGroupInfo));
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
                                Group_Mlist = new List<GroupModel>();
                                if (dt.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        Group_M = new GroupModel
                                        {
                                            ObjId = Convert.ToInt32(dt.Rows[i]["ObjId"].ToString()),
                                            ObjTitle = dt.Rows[i]["ObjTitle"].ToString(),
                                            ObjClsId = Convert.ToInt32(dt.Rows[i]["ObjClsId"].ToString()),
                                            ClsName = dt.Rows[i]["ClsName"].ToString(),
                                            ObjIsActive = bool.Parse(dt.Rows[i]["ObjIsActive"].ToString()),
                                            ObjCreationDate = DateTime.Parse(dt.Rows[i]["ObjCreationDate"].ToString()).ToShortDateString(),
                                            ObjDescription = dt.Rows[i]["ObjDescription"].ToString(),
                                            UserOwnerID = Convert.ToInt32(dt.Rows[i]["UserOwnerID"].ToString()),
                                            OwnerFullName = dt.Rows[i]["OwnerFullName"].ToString(),
                                            OwnerUserName = dt.Rows[i]["OwnerUserName"].ToString(),
                                            OrgOwner = dt.Rows[i]["OrgOwner"].ToString(),
                                            OrgEnName = dt.Rows[i]["OrgEnName"].ToString(),
                                            OrgArName = dt.Rows[i]["OrgArName"].ToString(),
                                            OrgKuName = dt.Rows[i]["OrgKuName"].ToString(),
                                        };
                                        Group_Mlist.Add(Group_M);
                                    }

                                    Response_MV = new ResponseModelView
                                    {
                                        Success = true,
                                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                        Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Group_Mlist }
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
        /// Everyone To Do:
        /// Get Group info by group Id
        /// </summary>
        /// <param name="GroupId">ID of Group that neet to view</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetGroupById(int GroupId, RequestHeaderModelView RequestHeader)
        {
            try
            {
                if (GroupId == 0 || GroupId.ToString().IsInt() == false)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsInt],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
                }
                else
                {
                    Session_S = new SessionService();
                    var ResponseSession = await Session_S.CheckAuthorizationResponse(RequestHeader);
                    if (ResponseSession.Success == false)
                    {
                        return ResponseSession;
                    }
                    else
                    {
                        string getGroupInfo = "SELECT  ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, CONVERT(DATE,ObjCreationDate,104) AS ObjCreationDate, " +
                                              "        ObjDescription, UserOwnerID, OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                             $"FROM    [User].V_Groups    WHERE   ObjId={GroupId} ";
                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getGroupInfo));
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
                        Group_Mlist = new List<GroupModel>();
                        if (dt.Rows.Count > 0)
                        {
                            Group_M = new GroupModel
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
                            };
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = Group_M
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
        /// <summary>
        /// Only Admins To Do:
        /// Add Group
        /// </summary>
        /// <param name="Group_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> AddGroup(GroupModelView Group_MV, RequestHeaderModelView RequestHeader)
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
                    if (((SessionModel)ResponseSession.Data).IsOrgAdmin == false && ((SessionModel)ResponseSession.Data).IsGroupOrgAdmin == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        if (ValidationService.IsEmpty(Group_MV.GroupTitle) == true)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GroupTitleMustEnter],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                            int checkDeblicate = Convert.ToInt32(dam.FireSQL("SELECT   COUNT(*)   FROM   [User].[V_Groups] " +
                                                                            $"WHERE    ObjTitle = '{Group_MV.GroupTitle}' AND " +
                                                                            $"OrgOwner ={Group_MV.GroupOrgOwnerID} AND " +
                                                                            $"ObjClsId ={(int)GlobalService.ClassType.Group} AND ObjIsActive=1 "));
                            if (checkDeblicate == 0)
                            {
                                string exeut = $"EXEC [User].[AddGroupPro] '{(int)GlobalService.ClassType.Group}','{Group_MV.GroupTitle}', '{userLoginID}', '{Group_MV.GroupOrgOwnerID}', '{Group_MV.GroupDescription}' ";
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
                                    Message = Group_MV.GroupTitle + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
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
        /// <summary>
        /// Only Admins To Do
        /// Edit Group
        /// </summary>
        /// <param name="Group_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> EditGroup(GroupModelView Group_MV, RequestHeaderModelView RequestHeader)
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
                    if (((SessionModel)ResponseSession.Data).IsOrgAdmin == false && ((SessionModel)ResponseSession.Data).IsGroupOrgAdmin == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        if (ValidationService.IsEmpty(Group_MV.GroupTitle) == true)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GroupTitleMustEnter],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            int isGroupAdmins = Convert.ToInt32(dam.FireSQL("SELECT   COUNT(*)     FROM   [User].[V_Groups] " +
                                                                           $"WHERE    ObjId={Group_MV.GroupId} AND ObjTitle = 'GroupOrgAdmins' "));
                            if (isGroupAdmins > 0)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GroupUnEditable],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                int checkDeblicate = Convert.ToInt32(dam.FireSQL("SELECT   COUNT(*)   FROM [User].[V_Groups] " +
                                                                                $"WHERE    ObjTitle = '{Group_MV.GroupTitle}' AND ObjId != {Group_MV.GroupId} AND " +
                                                                                $"OrgOwner ={Group_MV.GroupOrgOwnerID} AND " +
                                                                                $"ObjClsId ={(int)GlobalService.ClassType.Group} AND ObjIsActive=1 "));
                                if (checkDeblicate == 0)
                                {
                                    string exeut = $"EXEC [User].[UpdateGroupPro] '{Group_MV.GroupId}','{Group_MV.GroupTitle}', '{Group_MV.GroupIsActive}','{Group_MV.GroupDescription}' ";
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
                                        Message = Group_MV.GroupTitle + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
                                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                    };
                                    return Response_MV;
                                }
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
        /// <summary>
        /// Everyone To Do:
        /// Search Group by Title
        /// </summary>
        /// <param name="GroupName">Group Name</param>
        /// <param name="Pagination_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> SearchGroupByName(string GroupName, PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
        {
            try
            {
                if (GroupName.IsEmpty() == true)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GroupTitleMustEnter],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
                }
                else
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
                        int CurrentPage = _PageNumber; int PageRows = _PageRows;

                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                        var MaxTotal = dam.FireDataTable($"SELECT  COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM    [User].V_Groups  " +
                                                         $"WHERE   ObjTitle LIKE '{GroupName}%' AND " +
                                                         $"        OrgOwner IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND " +
                                                         $"        ObjClsId ={(int)GlobalService.ClassType.Group}  ");
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
                                string getgroupInfo = "SELECT    ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, CONVERT(DATE,ObjCreationDate,104) AS ObjCreationDate, ObjDescription, " +
                                                      "          UserOwnerID, OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                                      "FROM     [User].V_Groups " +
                                                     $"WHERE    ObjTitle LIKE '{GroupName}%' AND " +
                                                     $"         OrgOwner IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND " +
                                                     $"         ObjClsId ={(int)GlobalService.ClassType.Group} " +
                                                      "ORDER BY ObjId " +
                                                     $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                     $"FETCH NEXT   {_PageRows} ROWS ONLY ";


                                dt = new DataTable();
                                dt = await Task.Run(() => dam.FireDataTable(getgroupInfo));
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
                                Group_Mlist = new List<GroupModel>();
                                if (dt.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        Group_M = new GroupModel
                                        {
                                            ObjId = Convert.ToInt32(dt.Rows[i]["ObjId"].ToString()),
                                            ObjTitle = dt.Rows[i]["ObjTitle"].ToString(),
                                            ObjClsId = Convert.ToInt32(dt.Rows[i]["ObjClsId"].ToString()),
                                            ClsName = dt.Rows[i]["ClsName"].ToString(),
                                            ObjIsActive = bool.Parse(dt.Rows[i]["ObjIsActive"].ToString()),
                                            ObjCreationDate = DateTime.Parse(dt.Rows[0]["ObjCreationDate"].ToString()).ToShortDateString(),
                                            ObjDescription = dt.Rows[i]["ObjDescription"].ToString(),
                                            UserOwnerID = Convert.ToInt32(dt.Rows[0]["UserOwnerID"].ToString()),
                                            OwnerFullName = dt.Rows[i]["OwnerFullName"].ToString(),
                                            OwnerUserName = dt.Rows[i]["OwnerUserName"].ToString(),
                                            OrgOwner = dt.Rows[i]["OrgOwner"].ToString(),
                                            OrgEnName = dt.Rows[i]["OrgEnName"].ToString(),
                                            OrgArName = dt.Rows[i]["OrgArName"].ToString(),
                                            OrgKuName = dt.Rows[i]["OrgKuName"].ToString(),
                                        };
                                        Group_Mlist.Add(Group_M);
                                    }

                                    Response_MV = new ResponseModelView
                                    {
                                        Success = true,
                                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                        Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, PageRows, data = Group_Mlist }
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