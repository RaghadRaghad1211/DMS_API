using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;
namespace DMS_API.Services
{

    public class LinkParentChildService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private PermissionsService Permissions_S { get; set; }
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

        #region Groups & Folders
        public async Task<ResponseModelView> GetChildInParentByID(int ParentClassID, int ParentID, RequestHeaderModelView RequestHeader)
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
                    // int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                    string getParintChildInfo = $"SELECT LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId," +
                                                 "       ParentClassType, LcChildObjId, ChildTitle, LcChildClsId, ChildClassType, ChildCreationDate, LcIsActive " +
                                                $" FROM  [Main].[GetChildsInParentForAdmin]({ParentID},{ParentClassID})";

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
                            Data = LinkParentChild_Mlist
                        };
                        return Response_MV;
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                            Data = LinkParentChild_Mlist
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
        public async Task<ResponseModelView> GetChildInParentByID_Search(int ParentClassID, SearchChildParentModelView SearchChildParent_MV, RequestHeaderModelView RequestHeader)
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

                    //string getParentChildInfo = $"SELECT LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId," +
                    //                             "       ParentClassType, LcChildObjId, ChildTitle, LcChildClsId, ChildClassType, LcIsActive " +
                    //                            $" FROM  [User].[GetChildsInGroup]({ParentID})";

                    string getGroupChildInfo_Search = $"SELECT LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId," +
                                                 "       ParentClassType, LcChildObjId, ChildTitle, LcChildClsId, ChildClassType, ChildCreationDate, LcIsActive " +
                                                $" FROM  [Main].[GetChildsInParent_Search]({SearchChildParent_MV.ParentId}, {ParentClassID}, {SearchChildParent_MV.ChildTypeId},'{SearchChildParent_MV.TitleSearch}')";

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
                            //Data = LinkParentChild_Mlist
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
                            //Data = LinkParentChild_Mlist
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

        #region Groups
        public async Task<ResponseModelView> GetChildNotInGroupByID(int ParentClassID, int GroupID, RequestHeaderModelView RequestHeader)
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
                    string getChildNotinGroup = $"SELECT ID, Title, IsActive, Type FROM [User].[GetChildsNotInGroup] ({GroupID}, {ParentClassID}, {userLoginID})";

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
                            Data = ChildNotInParent_MVlist
                        };
                        return Response_MV;
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                            Data = ChildNotInParent_MVlist
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
        public async Task<ResponseModelView> GetChildNotInGroupByID_Search(int ParentClassID, SearchChildParentModelView SearchChildParent_MV, RequestHeaderModelView RequestHeader)
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

                    string GetChildsNotInGroup_Search = $"SELECT ID, Title, IsActive, Type FROM [User].[GetChildsNotInGroup_Search] ({SearchChildParent_MV.ParentId}, {ParentClassID} ,{userLoginID}, {SearchChildParent_MV.ChildTypeId},'{SearchChildParent_MV.TitleSearch}')";

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
                            //Data = ChildNotInParent_MVlist
                        };
                        return Response_MV;
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                            Data = ChildNotInParent_MVlist
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

        #region Groups & Folders
        public async Task<ResponseModelView> AddChildIntoParent(int ParentClassID, LinkParentChildModelView LinkParentChild_MV, RequestHeaderModelView RequestHeader)
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
                    if (LinkParentChild_MV.ChildIds.Count == 0)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectedObjects],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }

                    string Query = GlobalService.GetQueryLinkPro(LinkParentChild_MV);
                    string exeut = $"EXEC [Main].[AddLinksPro]  '{LinkParentChild_MV.ParentId}', '{ParentClassID}', '{Query}',{1} ";
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
        public async Task<ResponseModelView> RemoveChildFromParent(int ParentClassID, LinkParentChildModelView LinkParentChild_MV, RequestHeaderModelView RequestHeader)
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
                    if (ParentClassID == (int)GlobalService.ClassType.Folder)
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        foreach (var item in LinkParentChild_MV.ChildIds)
                        {
                            bool checkManagePermission = GlobalService.CheckUserPermissions(userLoginID, item).Result.IsManage;
                            if (checkManagePermission == false)
                            {
                                LinkParentChild_MV.ChildIds.Remove(item);
                            }
                            if (LinkParentChild_MV.ChildIds.Count == 0 || LinkParentChild_MV.ChildIds == null)
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

                    if (LinkParentChild_MV.ChildIds.Count == 0)
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
                        string Query = GlobalService.GetQueryLinkPro(LinkParentChild_MV);
                        string exeut = $"EXEC [Main].[UpdateLinksPro]  '{LinkParentChild_MV.ParentId}', '{ParentClassID}', '{Query}','{0}' ";
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

        #region Folders & Documents
        public async Task<ResponseModelView> MoveChildToNewFolder(int FolderClassID, int ChildClassID, MoveChildToNewFolderModelView MoveChildToNewFolder_MV, RequestHeaderModelView RequestHeader)
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
                    foreach (var item in MoveChildToNewFolder_MV.ChildIds)
                    {
                        bool checkManagePermission = GlobalService.CheckUserPermissions(userLoginID, item).Result.IsManage;
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
                    if (MoveChildToNewFolder_MV.CurrentParentID == 0 || MoveChildToNewFolder_MV.ChildIds.Count == 0 || MoveChildToNewFolder_MV.NewParentID == 0)
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
                        string Query = GlobalService.GetQueryMoveChilds(MoveChildToNewFolder_MV);
                        string moveChild2Folder = $"EXEC [Main].[MoveChildToFolderPro] '{MoveChildToNewFolder_MV.CurrentParentID}','{MoveChildToNewFolder_MV.NewParentID}', '{FolderClassID}', '{Query}', '{ChildClassID}' ";
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

        #region Test
        //public async Task<ResponseModelView> GetLinkParentsChildsList(PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
        //{
        //    try
        //    {
        //        Session_S = new SessionService();
        //        var ResponseSession = await Session_S.CheckAuthorizationResponse(RequestHeader);
        //        if (ResponseSession.Success == false)
        //        {
        //            return ResponseSession;
        //        }
        //        else
        //        {
        //            int _PageNumber = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
        //            int _PageRows = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
        //            int CurrentPage = _PageNumber;

        //            int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
        //            int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
        //            string whereField = orgOwnerID == 0 ? "OrgUp" : "OrgId";
        //            var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
        //                                     $"FROM [User].[V_Links]  WHERE [ParentOrgOwnerId] IN (SELECT {whereField} FROM [User].[GetOrgsbyUserId]({userLoginID})) AND LcParentClsId ={ClassGroupID} ");
        //            if (MaxTotal == null)
        //            {
        //                Response_MV = new ResponseModelView
        //                {
        //                    Success = false,
        //                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError],
        //                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
        //                };
        //                return Response_MV;
        //            }
        //            else
        //            {
        //                if (MaxTotal.Rows.Count == 0)
        //                {
        //                    Response_MV = new ResponseModelView
        //                    {
        //                        Success = false,
        //                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
        //                        Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
        //                    };
        //                    return Response_MV;
        //                }

        //                else
        //                {
        //                    string getLinkParentsChildsInfo = "SELECT  LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId, ParentClassType, LcChildObjId, ChildTitle, LcChildClsId, " +
        //                                         "            ChildClassType, LcIsActive " +
        //                                         "FROM            [User].V_Links " +
        //                                        $"WHERE [ParentOrgOwnerId] IN (SELECT {whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND LcParentClsId ={ClassGroupID} " +
        //                                         "ORDER BY LcId " +
        //                                        $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
        //                                        $"FETCH NEXT   {_PageRows} ROWS ONLY ";

        //                    dt = new DataTable();
        //                    dt = await Task.Run(() => dam.FireDataTable(getLinkParentsChildsInfo));
        //                    if (dt == null)
        //                    {
        //                        Response_MV = new ResponseModelView
        //                        {
        //                            Success = false,
        //                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError],
        //                            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
        //                        };
        //                        return Response_MV;
        //                    }
        //                    LinkParentChild_Mlist = new List<LinkParentChildModel>();
        //                    if (dt.Rows.Count > 0)
        //                    {
        //                        for (int i = 0; i < dt.Rows.Count; i++)
        //                        {
        //                            LinkParentChild_M = new LinkParentChildModel
        //                            {
        //                                LcId = Convert.ToInt32(dt.Rows[i]["LcId"].ToString()),
        //                                ParentUserOwnerId = Convert.ToInt32(dt.Rows[i]["ParentUserOwnerId"].ToString()),
        //                                ParentOrgOwnerId = Convert.ToInt32(dt.Rows[i]["ParentOrgOwnerId"].ToString()),
        //                                ParentId = Convert.ToInt32(dt.Rows[i]["LcParentObjId"].ToString()),
        //                                ParentTitle = dt.Rows[i]["ObjTitle"].ToString(),
        //                                ParentClsId = Convert.ToInt32(dt.Rows[i]["LcParentClsId"].ToString()),
        //                                ParentClassType = dt.Rows[i]["ParentClassType"].ToString(),
        //                                ChildId = Convert.ToInt32(dt.Rows[i]["LcChildObjId"].ToString()),
        //                                ChildTitle = dt.Rows[i]["ChildTitle"].ToString(),
        //                                ChildClsId = Convert.ToInt32(dt.Rows[i]["LcChildClsId"].ToString()),
        //                                ChildClassType = dt.Rows[i]["ChildClassType"].ToString(),
        //                                LcIsActive = bool.Parse(dt.Rows[i]["LcIsActive"].ToString()),


        //                            };
        //                            LinkParentChild_Mlist.Add(LinkParentChild_M);
        //                        }

        //                        Response_MV = new ResponseModelView
        //                        {
        //                            Success = true,
        //                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
        //                            Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = LinkParentChild_Mlist }
        //                        };
        //                        return Response_MV;
        //                    }
        //                    else
        //                    {
        //                        Response_MV = new ResponseModelView
        //                        {
        //                            Success = false,
        //                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
        //                            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
        //                        };
        //                        return Response_MV;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Response_MV = new ResponseModelView
        //        {
        //            Success = false,
        //            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
        //            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
        //        };
        //        return Response_MV;
        //    }
        //}
        #endregion

        #endregion
    }
}
