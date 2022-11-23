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
        private DataTable dt { get; set; }
        private LinkParentChildModel LinkParentChild_M { get; set; }
        private GetChildNotInParentModelView ChildNotInParent_M { get; set; }
        private List<LinkParentChildModel> LinkParentChild_Mlist { get; set; } 
        private List<GetChildNotInParentModelView> ChildNotInParent_MVlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        // 2 Group , 4 Folder  , 5 Document
        #endregion

        #region Constructor        
        public LinkParentChildService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Functions
        public async Task<ResponseModelView> GetChildIntoParentByID(int ParentClassID, int ParentID, RequestHeaderModelView RequestHeader)
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
                    //string getParentChildInfo = "SELECT  LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId,  " +
                    //                            "        ParentClassType, LcChildObjId, ChildTitle, LcChildClsId, ChildClassType, LcIsActive " +
                    //                            "FROM    [User].V_Links " +
                    //                           $"WHERE   [ParentOrgOwnerId] IN (SELECT {whereField} FROM [User].GetOrgsbyUserId({userLoginID})) " +
                    //                           $"        AND LcParentObjId={ParentID} AND LcParentClsId ={ParentClassID} AND LcIsActive=1";

                    string getParentChildInfo = "SELECT  LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId,  " +
                                                "        ParentClassType, LcChildObjId, ChildTitle, LcChildClsId, ChildClassType, LcIsActive " +
                                                "FROM    [User].V_Links " +
                                               $"WHERE   LcParentObjId={ParentID} AND LcParentClsId ={ParentClassID} AND LcIsActive=1";

                    dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getParentChildInfo));
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

        public async Task<ResponseModelView> GetChildNotInParentByID(int ParentClassID, int ParentID, RequestHeaderModelView RequestHeader)
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
                    //int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));

                    //string getParentChildInfo = "SELECT  LcId,ParentUserOwnerId,ParentOrgOwnerId, LcParentObjId, ObjTitle, LcParentClsId,  " +
                    //                            "        ParentClassType, LcChildObjId, ChildTitle, LcChildClsId, ChildClassType, LcIsActive " +
                    //                            "FROM    [User].V_Links " +
                    //                           $"WHERE   LcParentObjId={ParentID} AND LcParentClsId ={ParentClassID} AND LcIsActive=1";
                    string getChildNotinGroup = $"SELECT ID, Title, IsActive, Type FROM [User].[GetChildsNotInGroup] ({ParentID} ,{userLoginID})";

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
                    for (int i = 0; i < LinkParentChild_MV.ChildIds.Count; i++)
                    {
                        string exeut = $"EXEC [Main].[AddLinksPro]  '{LinkParentChild_MV.ParentId}', '{ParentClassID}','{LinkParentChild_MV.ChildIds[i]}',{1} ";
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
                    for (int i = 0; i < LinkParentChild_MV.ChildIds.Count; i++)
                    {
                        string exeut = $"EXEC [Main].[UpdateLinksPro]  '{LinkParentChild_MV.ParentId}','{LinkParentChild_MV.ChildIds[i]}',{0} ";                        
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
