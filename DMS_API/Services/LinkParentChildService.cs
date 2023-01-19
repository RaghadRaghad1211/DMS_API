using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;
namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Links Between Parents and Childs
    /// </summary>
    public class LinkParentChildService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private LinkParentChildModel LinkParentChild_M { get; set; }
        private GetChildNotInParentModelView ChildNotInParent_M { get; set; }
        private List<LinkParentChildModel> LinkParentChild_Mlist { get; set; }
        private List<GetChildNotInParentModelView> ChildNotInParent_MVlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public LinkParentChildService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Functions
        #region Groups
        /// <summary>
        /// Everyone To Do:
        /// To Get Childs (Groups & Users) In Group by Group Id
        /// </summary>
        /// <param name="GroupId">Group Id</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetChildsInGroupByGroupId(int GroupId, RequestHeaderModelView RequestHeader)
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
                        string getParintChildInfo = $"SELECT LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId, ParentClassType," +
                                                     "       LcChildObjId, ChildTitle, LcChildClsId, ChildClassType, ChildCreationDate, LcIsActive " +
                                                    $" FROM  [User].[GetChildsInGroup]({GroupId}) " +
                                                    $"WHERE  LcIsActive=1 ";

                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getParintChildInfo));
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
                        LinkParentChild_Mlist = new List<LinkParentChildModel>();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                LinkParentChild_M = new LinkParentChildModel
                                {
                                    LcId = Convert.ToInt32(dt.Rows[i]["LcId"].ToString()),
                                    ParentUserOwnerId = Convert.ToInt32(dt.Rows[i]["ParentUserOwnerId"].ToString()),
                                    ParentOrgOwnerId = Convert.ToInt32(dt.Rows[i]["ParentOrgOwnerId"].ToString()),
                                    ParentId = Convert.ToInt32(dt.Rows[i]["LcParentObjId"].ToString()),
                                    ParentTitle = dt.Rows[i]["ObjTitle"].ToString(),
                                    ParentClsId = Convert.ToInt32(dt.Rows[i]["LcParentClsId"].ToString()),
                                    ParentClassType = dt.Rows[i]["ParentClassType"].ToString(),
                                    ChildId = Convert.ToInt32(dt.Rows[i]["LcChildObjId"].ToString()),
                                    ChildTitle = dt.Rows[i]["ChildTitle"].ToString(),
                                    ChildClsId = Convert.ToInt32(dt.Rows[i]["LcChildClsId"].ToString()),
                                    ChildClassType = dt.Rows[i]["ChildClassType"].ToString(),
                                    ChildCreationDate = DateTime.Parse(dt.Rows[i]["ChildCreationDate"].ToString()).ToShortDateString(),
                                    LcIsActive = bool.Parse(dt.Rows[i]["LcIsActive"].ToString()),
                                };
                                LinkParentChild_Mlist.Add(LinkParentChild_M);
                            }
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = new
                                {
                                    Users = LinkParentChild_Mlist.Where(x => x.ChildClsId == 1),
                                    Groups = LinkParentChild_Mlist.Where(x => x.ChildClsId == 2)
                                }
                            };
                            return Response_MV;
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                                Data = new
                                {
                                    Users = LinkParentChild_Mlist.Where(x => x.ChildClsId == 1),
                                    Groups = LinkParentChild_Mlist.Where(x => x.ChildClsId == 2)
                                }
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
        /// Serarch Childs (Groups & Users) In Group by Group Id
        /// </summary>
        /// <param name="SearchChildOfGroup_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetChildsInGroupByGroupId_Search(SearchChildsOfGroupModelView SearchChildOfGroup_MV, RequestHeaderModelView RequestHeader)
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
                        string getGroupChildInfo_Search = "SELECT LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId, ParentClassType, " +
                                                          "       LcChildObjId, ChildTitle, LcChildClsId, ChildClassType, ChildCreationDate, LcIsActive " +
                                                         $"FROM   [User].[GetChildsInGroup_Search]({SearchChildOfGroup_MV.GroupId}, {SearchChildOfGroup_MV.ChildTypeId},'{SearchChildOfGroup_MV.TitleSearch}') " +
                                                         $"WHERE  LcIsActive=1";

                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getGroupChildInfo_Search));
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
                        LinkParentChild_Mlist = new List<LinkParentChildModel>();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                LinkParentChild_M = new LinkParentChildModel
                                {
                                    LcId = Convert.ToInt32(dt.Rows[i]["LcId"].ToString()),
                                    ParentUserOwnerId = Convert.ToInt32(dt.Rows[i]["ParentUserOwnerId"].ToString()),
                                    ParentOrgOwnerId = Convert.ToInt32(dt.Rows[i]["ParentOrgOwnerId"].ToString()),
                                    ParentId = Convert.ToInt32(dt.Rows[i]["LcParentObjId"].ToString()),
                                    ParentTitle = dt.Rows[i]["ObjTitle"].ToString(),
                                    ParentClsId = Convert.ToInt32(dt.Rows[i]["LcParentClsId"].ToString()),
                                    ParentClassType = dt.Rows[i]["ParentClassType"].ToString(),
                                    ChildId = Convert.ToInt32(dt.Rows[i]["LcChildObjId"].ToString()),
                                    ChildTitle = dt.Rows[i]["ChildTitle"].ToString(),
                                    ChildClsId = Convert.ToInt32(dt.Rows[i]["LcChildClsId"].ToString()),
                                    ChildClassType = dt.Rows[i]["ChildClassType"].ToString(),
                                    ChildCreationDate = DateTime.Parse(dt.Rows[i]["ChildCreationDate"].ToString()).ToShortDateString(),
                                    LcIsActive = bool.Parse(dt.Rows[i]["LcIsActive"].ToString()),
                                };
                                LinkParentChild_Mlist.Add(LinkParentChild_M);
                            }
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = new
                                {
                                    Users = LinkParentChild_Mlist.Where(x => x.ChildClsId == 1),
                                    Groups = LinkParentChild_Mlist.Where(x => x.ChildClsId == 2)
                                }
                            };
                            return Response_MV;
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                                Data = new
                                {
                                    Users = LinkParentChild_Mlist.Where(x => x.ChildClsId == 1),
                                    Groups = LinkParentChild_Mlist.Where(x => x.ChildClsId == 2)
                                }
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
        /// Get Childs (Groups & Users) Not In Group by Group Id
        /// </summary>
        /// <param name="GroupId">Group Id</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetChildNotInGroupByGroupId(int GroupId, RequestHeaderModelView RequestHeader)
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
                            string getChildNotinGroup = "SELECT  ID, Title, IsActive, Type " +
                                                       $"FROM   [User].[GetChildsNotInGroup] ({GroupId}, {userLoginID}) " +
                                                        "WHERE  IsActive=1 ";

                            dt = new DataTable();
                            dt = await Task.Run(() => dam.FireDataTable(getChildNotinGroup));
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
                            ChildNotInParent_MVlist = new List<GetChildNotInParentModelView>();
                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    ChildNotInParent_M = new GetChildNotInParentModelView
                                    {
                                        ID = Convert.ToInt32(dt.Rows[i]["ID"].ToString()),
                                        Title = dt.Rows[i]["Title"].ToString(),
                                        Type = dt.Rows[i]["Type"].ToString(),
                                        IsActive = bool.Parse(dt.Rows[i]["IsActive"].ToString()),
                                    };
                                    ChildNotInParent_MVlist.Add(ChildNotInParent_M);
                                }
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new
                                    {
                                        Users = ChildNotInParent_MVlist.Where(x => x.Type == "User"),
                                        Groups = ChildNotInParent_MVlist.Where(x => x.Type == "Group")
                                    }
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                                    Data = new
                                    {
                                        Users = ChildNotInParent_MVlist.Where(x => x.Type == "User"),
                                        Groups = ChildNotInParent_MVlist.Where(x => x.Type == "Group")
                                    }
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
        /// Only Admins To Do:
        /// Serarch Childs (Groups & Users) Not In Group by Group Id
        /// </summary>
        /// <param name="SearchChildParent_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetChildNotInGroupByGroupId_Search(SearchChildsOfGroupModelView SearchChildOfGroup_MV, RequestHeaderModelView RequestHeader)
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
                        string GetChildsNotInGroup_Search = "SELECT  ID, Title, IsActive, Type " +
                                                           $"FROM   [User].[GetChildsNotInGroup_Search] ({SearchChildOfGroup_MV.GroupId} ,{userLoginID}, {SearchChildOfGroup_MV.ChildTypeId},'{SearchChildOfGroup_MV.TitleSearch}')  " +
                                                            "WHERE  IsActive=1 ";

                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(GetChildsNotInGroup_Search));
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
                        ChildNotInParent_MVlist = new List<GetChildNotInParentModelView>();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ChildNotInParent_M = new GetChildNotInParentModelView
                                {
                                    ID = Convert.ToInt32(dt.Rows[i]["ID"].ToString()),
                                    Title = dt.Rows[i]["Title"].ToString(),
                                    Type = dt.Rows[i]["Type"].ToString(),
                                    IsActive = bool.Parse(dt.Rows[i]["IsActive"].ToString()),
                                };
                                ChildNotInParent_MVlist.Add(ChildNotInParent_M);
                            }
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = new
                                {
                                    Users = ChildNotInParent_MVlist.Where(x => x.Type == "User"),
                                    Groups = ChildNotInParent_MVlist.Where(x => x.Type == "Group")
                                }
                            };
                            return Response_MV;
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                                Data = new
                                {
                                    Users = ChildNotInParent_MVlist.Where(x => x.Type == "User"),
                                    Groups = ChildNotInParent_MVlist.Where(x => x.Type == "Group")
                                }
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
        /// Add Childs (Groups & Users) into Group
        /// </summary>
        /// <param name="LinkGroupChilds_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> AddChildsIntoGroup(LinkGroupChildsModelView LinkGroupChilds_MV, RequestHeaderModelView RequestHeader)
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
                        if (LinkGroupChilds_MV.ChildIds.Count == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectedObjects],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }

                        string Query = GlobalService.GetQueryLinkPro(LinkGroupChilds_MV.ChildIds);
                        string exeut = $"EXEC [Main].[AddLinksPro]  '{LinkGroupChilds_MV.GroupId}', '{(int)GlobalService.ClassType.Group}', '{Query}',{1} ";
                        var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));
                        if (outValue == 0.ToString() || (outValue == null || outValue.Trim() == ""))
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.InsertFaild],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
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
        /// Remove Childs (Groups & Users) from Group
        /// </summary>
        /// <param name="LinkGroupChilds_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> RemoveChildsFromGroup(LinkGroupChildsModelView LinkGroupChilds_MV, RequestHeaderModelView RequestHeader)
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
                        if (LinkGroupChilds_MV.ChildIds.Count == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectedObjects],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            string Query = GlobalService.GetQueryLinkPro(LinkGroupChilds_MV.ChildIds);
                            string exeut = $"EXEC [Main].[UpdateLinksPro]  '{LinkGroupChilds_MV.GroupId}', '{(int)GlobalService.ClassType.Group}', '{Query}','{0}' ";
                            var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));
                            if (outValue == 0.ToString() || (outValue == null || outValue.Trim() == ""))
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DeleteFaild],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                                };
                                return Response_MV;
                            }
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DeleteSuccess],
                                Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
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
        #region Folders
        /// <summary>
        /// Users Have IsManage & Admins To Do:
        /// Move Childs (Folders & Documents) from Folder to another
        /// </summary>
        /// <param name="FolderClassID"></param>
        /// <param name="ChildClassID"></param>
        /// <param name="MoveChildToNewFolder_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> MoveChildsToNewFolder(MoveChildToNewFolderModelView MoveChildToNewFolder_MV, RequestHeaderModelView RequestHeader)
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
                    if (MoveChildToNewFolder_MV.ChildIds.Count == 0 || MoveChildToNewFolder_MV.ChildIds == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectedObjects],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        foreach (var item in MoveChildToNewFolder_MV.ChildIds)
                        {
                            var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, item).Result;
                            bool checkManagePermission = result == null ? false : result.IsManage;

                            if (checkManagePermission == false)
                            {
                                MoveChildToNewFolder_MV.ChildIds.Remove(item);
                            }
                            if (MoveChildToNewFolder_MV.ChildIds.Count == 0 || MoveChildToNewFolder_MV.ChildIds == null)
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
                        if (MoveChildToNewFolder_MV.CurrentFolderId == 0 || MoveChildToNewFolder_MV.ChildIds.Count == 0 || MoveChildToNewFolder_MV.NewFolderId == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectedObjects],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            string Query = GlobalService.GetQueryLinkPro(MoveChildToNewFolder_MV.ChildIds);
                            string moveChild2Folder = $"EXEC [Main].[MoveChildToFolderPro] '{MoveChildToNewFolder_MV.CurrentFolderId}','{MoveChildToNewFolder_MV.NewFolderId}', '{(int)GlobalService.ClassType.Folder}', '{Query}' ";
                            var outValue = await Task.Run(() => dam.DoQueryExecProcedure(moveChild2Folder));
                            if (outValue == 0.ToString() || (outValue == null || outValue.Trim() == ""))
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MoveFaild],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MoveSuccess],
                                    Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
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
        /// Users Have IsManage & Admins To Do:
        /// Remove Childs (Folders & Documents) from Folder
        /// </summary>
        /// <param name="LinkFolderChilds_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> RemoveChildsFromFolder(LinkFolderChildsModelView LinkFolderChilds_MV, RequestHeaderModelView RequestHeader)
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
                    if (LinkFolderChilds_MV.ChildIds.Count == 0 || LinkFolderChilds_MV.ChildIds == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectedObjects],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        foreach (var item in LinkFolderChilds_MV.ChildIds)
                        {
                            var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, item).Result;
                            bool checkManagePermission = result == null ? false : result.IsManage;

                            if (checkManagePermission == false)
                            {
                                LinkFolderChilds_MV.ChildIds.Remove(item);
                            }
                            if (LinkFolderChilds_MV.ChildIds.Count == 0 || LinkFolderChilds_MV.ChildIds == null)
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
                        string Query = GlobalService.GetQueryLinkPro(LinkFolderChilds_MV.ChildIds);
                        string exeut = $"EXEC [Main].[UpdateLinksPro]  '{LinkFolderChilds_MV.FolderId}', '{(int)GlobalService.ClassType.Folder}', '{Query}','{0}' ";
                        var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));
                        if (outValue == 0.ToString() || (outValue == null || outValue.Trim() == ""))
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DeleteFaild],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DeleteSuccess],
                            Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
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
        #endregion
    }
}