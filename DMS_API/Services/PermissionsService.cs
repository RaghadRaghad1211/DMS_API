using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;

namespace DMS_API.Services
{
    public class PermissionsService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private PermessionsModel Permessions_M { get; set; }
        private List<PermessionsModel> Permessions_Mlist { get; set; }
        private GetDestPerOnObjectModelView DestNotHavePer_MV { get; set; }
        private List<GetDestPerOnObjectModelView> DestNotHavePer_MVlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public PermissionsService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Functions
        public async Task<ResponseModelView> GetChildsInParentWithPermissions(FolderChildsPermissionsModelView FolderChildsPermissions_MV, RequestHeaderModelView RequestHeader)
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
                    int _PageNumber = FolderChildsPermissions_MV.PageNumber == 0 ? 1 : FolderChildsPermissions_MV.PageNumber;
                    int _PageRows = FolderChildsPermissions_MV.PageRows == 0 ? 1 : FolderChildsPermissions_MV.PageRows;
                    int CurrentPage = _PageNumber;
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    if (FolderChildsPermissions_MV.IsMoveAtion == true)
                    {
                        if (FolderChildsPermissions_MV.ObjectsMovable.Count <= 0)
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
                            bool canOpen = await GlobalService.IsFolderOpenableToMoveInsideIt(FolderChildsPermissions_MV.ParentId, FolderChildsPermissions_MV.ObjectsMovable);
                            if (canOpen == false)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FolderUnOpenable],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                string getPermessions = " SELECT   SourObjId, SourTitle, SourType, SourTypeName," +
                                                        "          DestObjId, DestTitle, DestType, DestTypeName, " +
                                                        "          IsRead, IsWrite, IsManage, IsQR, SourCreationDate, " +
                                                        "          SourUserName, SourOrgArName, SourOrgEnName, SourOrgKuName  " +
                                                       $" FROM     [Document].[GetChildsInParentWithPermissions] ({userLoginID}, {FolderChildsPermissions_MV.ParentId}) " +
                                                        "ORDER BY  SourObjId " +
                                                       $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                       $"FETCH NEXT   {_PageRows} ROWS ONLY ";

                                dt = new DataTable();
                                dt = await Task.Run(() => dam.FireDataTable(getPermessions));
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
                                Permessions_Mlist = new List<PermessionsModel>();
                                if (dt.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        Permessions_M = new PermessionsModel
                                        {
                                            SourObjId = Convert.ToInt32(dt.Rows[i]["SourObjId"].ToString()),
                                            SourTitle = dt.Rows[i]["SourTitle"].ToString(),
                                            SourType = Convert.ToInt32(dt.Rows[i]["SourType"].ToString()),
                                            SourTypeName = dt.Rows[i]["SourTypeName"].ToString(),
                                            DestObjId = Convert.ToInt32(dt.Rows[i]["DestObjId"].ToString()),
                                            DestTitle = dt.Rows[i]["DestTitle"].ToString(),
                                            DestType = Convert.ToInt32(dt.Rows[i]["DestType"].ToString()),
                                            DestTypeName = dt.Rows[i]["DestTypeName"].ToString(),
                                            IsRead = bool.Parse(dt.Rows[i]["IsRead"].ToString()),
                                            IsWrite = bool.Parse(dt.Rows[i]["IsWrite"].ToString()),
                                            IsManage = bool.Parse(dt.Rows[i]["IsManage"].ToString()),
                                            IsQR = bool.Parse(dt.Rows[i]["IsQR"].ToString()),
                                            SourUserName = dt.Rows[i]["SourUserName"].ToString(),
                                            SourOrgArName = dt.Rows[i]["SourOrgArName"].ToString(),
                                            SourOrgEnName = dt.Rows[i]["SourOrgEnName"].ToString(),
                                            SourOrgKuName = dt.Rows[i]["SourOrgKuName"].ToString(),
                                            SourCreationDate = DateTime.Parse(dt.Rows[i]["SourCreationDate"].ToString()).ToShortDateString()
                                        };
                                        Permessions_Mlist.Add(Permessions_M);
                                    }
                                    Response_MV = new ResponseModelView
                                    {
                                        Success = true,
                                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                        Data = new
                                        {
                                            TotalRows = Permessions_Mlist.Count,
                                            MaxPage = Math.Ceiling(Permessions_Mlist.Count / (float)_PageRows),
                                            CurrentPage = _PageNumber,
                                            PageRows = _PageRows,
                                            data = Permessions_Mlist
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
                                            TotalRows = Permessions_Mlist.Count,
                                            MaxPage = Math.Ceiling(Permessions_Mlist.Count / (float)_PageRows),
                                            CurrentPage = _PageNumber,
                                            PageRows = _PageRows,
                                            data = Permessions_Mlist
                                        }
                                    };
                                    return Response_MV;
                                }
                            }
                        }
                    }
                    else
                    {
                        string getPermessions = " SELECT   SourObjId, SourTitle, SourType, SourTypeName," +
                                                "          DestObjId, DestTitle, DestType, DestTypeName, " +
                                                "          IsRead, IsWrite, IsManage, IsQR, SourCreationDate, " +
                                                "          SourUserName, SourOrgArName, SourOrgEnName, SourOrgKuName  " +
                                               $" FROM     [Document].[GetChildsInParentWithPermissions] ({userLoginID}, {FolderChildsPermissions_MV.ParentId}) " +
                                               "ORDER BY   SourObjId " +
                                                       $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                       $"FETCH NEXT   {_PageRows} ROWS ONLY ";

                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getPermessions));
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
                        Permessions_Mlist = new List<PermessionsModel>();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                Permessions_M = new PermessionsModel
                                {
                                    SourObjId = Convert.ToInt32(dt.Rows[i]["SourObjId"].ToString()),
                                    SourTitle = dt.Rows[i]["SourTitle"].ToString(),
                                    SourType = Convert.ToInt32(dt.Rows[i]["SourType"].ToString()),
                                    SourTypeName = dt.Rows[i]["SourTypeName"].ToString(),
                                    DestObjId = Convert.ToInt32(dt.Rows[i]["DestObjId"].ToString()),
                                    DestTitle = dt.Rows[i]["DestTitle"].ToString(),
                                    DestType = Convert.ToInt32(dt.Rows[i]["DestType"].ToString()),
                                    DestTypeName = dt.Rows[i]["DestTypeName"].ToString(),
                                    IsRead = bool.Parse(dt.Rows[i]["IsRead"].ToString()),
                                    IsWrite = bool.Parse(dt.Rows[i]["IsWrite"].ToString()),
                                    IsManage = bool.Parse(dt.Rows[i]["IsManage"].ToString()),
                                    IsQR = bool.Parse(dt.Rows[i]["IsQR"].ToString()),
                                    SourUserName = dt.Rows[i]["SourUserName"].ToString(),
                                    SourOrgArName = dt.Rows[i]["SourOrgArName"].ToString(),
                                    SourOrgEnName = dt.Rows[i]["SourOrgEnName"].ToString(),
                                    SourOrgKuName = dt.Rows[i]["SourOrgKuName"].ToString(),
                                    SourCreationDate = DateTime.Parse(dt.Rows[i]["SourCreationDate"].ToString()).ToShortDateString()
                                };
                                Permessions_Mlist.Add(Permessions_M);
                            }
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = new
                                {
                                    TotalRows = Permessions_Mlist.Count,
                                    MaxPage = Math.Ceiling(Permessions_Mlist.Count / (float)_PageRows),
                                    CurrentPage = _PageNumber,
                                    PageRows = _PageRows,
                                    data = Permessions_Mlist
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
                                    TotalRows = Permessions_Mlist.Count,
                                    MaxPage = Math.Ceiling(Permessions_Mlist.Count / (float)_PageRows),
                                    CurrentPage = _PageNumber,
                                    PageRows = _PageRows,
                                    data = Permessions_Mlist
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
        public async Task<ResponseModelView> GetPermissionsOnObjectByObjectId(int ObjectId, RequestHeaderModelView RequestHeader)
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
                    string getPermessions = " SELECT   SourObjId, SourTitle, SourType, SourTypeName," +
                                            "          DestObjId, DestTitle, DestType, DestTypeName, " +
                                            "          IsRead, IsWrite, IsManage, IsQR " +
                                            " FROM     [User].[V_Permission]" +
                                           $" WHERE    SourObjId= {ObjectId} AND DestObjId= {userLoginID} ";

                    dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getPermessions));
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
                    Permessions_Mlist = new List<PermessionsModel>();
                    if (dt.Rows.Count > 0)
                    {
                        Permessions_M = new PermessionsModel
                        {
                            SourObjId = Convert.ToInt32(dt.Rows[0]["SourObjId"].ToString()),
                            SourTitle = dt.Rows[0]["SourTitle"].ToString(),
                            SourType = Convert.ToInt32(dt.Rows[0]["SourType"].ToString()),
                            SourTypeName = dt.Rows[0]["SourTypeName"].ToString(),
                            DestObjId = Convert.ToInt32(dt.Rows[0]["DestObjId"].ToString()),
                            DestTitle = dt.Rows[0]["DestTitle"].ToString(),
                            DestType = Convert.ToInt32(dt.Rows[0]["DestType"].ToString()),
                            DestTypeName = dt.Rows[0]["DestTypeName"].ToString(),
                            IsRead = bool.Parse(dt.Rows[0]["IsRead"].ToString()),
                            IsWrite = bool.Parse(dt.Rows[0]["IsWrite"].ToString()),
                            IsManage = bool.Parse(dt.Rows[0]["IsManage"].ToString()),
                            IsQR = bool.Parse(dt.Rows[0]["IsQR"].ToString()),
                            SourUserName = dt.Rows[0]["SourUserName"].ToString(),
                            SourOrgArName = dt.Rows[0]["SourOrgArName"].ToString(),
                            SourOrgEnName = dt.Rows[0]["SourOrgEnName"].ToString(),
                            SourOrgKuName = dt.Rows[0]["SourOrgKuName"].ToString(),
                            SourCreationDate = DateTime.Parse(dt.Rows[0]["SourCreationDate"].ToString()).ToShortDateString()
                        };
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                            Data = Permessions_M
                        };
                        return Response_MV;
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
                            Data = Permessions_M
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
        public async Task<ResponseModelView> AddPermissionsOnObject(AddPermissionsModelView AddPermissions_MV, RequestHeaderModelView RequestHeader)
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
                    bool checkManagePermission = GlobalService.CheckUserPermissions(userLoginID, AddPermissions_MV.SourObjId).Result.IsManage;
                    if (checkManagePermission == true)
                    {
                        string exeut = $"EXEC [User].[AddPermissionPro] '{AddPermissions_MV.SourObjId}','{AddPermissions_MV.SourClsId}', '{AddPermissions_MV.DestObjId}', '{AddPermissions_MV.DestClsId}','{AddPermissions_MV.PerRead}', '{AddPermissions_MV.PerWrite}', '{AddPermissions_MV.PerManage}', '{AddPermissions_MV.PerQR}' ";
                        var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));
                        if (outValue == 0.ToString() || outValue == null || outValue.Trim() == "")
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
        public async Task<ResponseModelView> EditPermissionsOnObject(EditPermissionsModelView EditPermissions_MV, RequestHeaderModelView RequestHeader)
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
                    //bool checkManagePermission = bool.Parse(dam.FireSQL($"SELECT [IsManage] FROM [User].[V_Permission] WHERE [SourObjId]={EditPermissions_MV.SourObjId} AND [DestObjId]={userLoginID}"));
                    bool checkManagePermission = GlobalService.CheckUserPermissions(userLoginID, EditPermissions_MV.SourObjId).Result.IsManage;
                    if (checkManagePermission == true)
                    {
                        string exeut = $"EXEC [User].[UpdatePermissionPro]  '{EditPermissions_MV.SourObjId}', '{EditPermissions_MV.DestObjId}', '{EditPermissions_MV.PerRead}', '{EditPermissions_MV.PerWrite}', '{EditPermissions_MV.PerManage}', '{EditPermissions_MV.PerQR}' ";

                        var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));
                        if (outValue == 0.ToString() || outValue == null || outValue.Trim() == "")
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EditFaild],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EditSuccess],
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
        public async Task<ResponseModelView> GetUsersOrGroupsNotHavePermissionOnObject(int ObjectId, PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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
                    string getPermessions = " SELECT   DestObjId, DestTitle, DestTypeId, DestTypeName" +
                                           $" FROM     [User].[GetDestObjsNotHavePerOnSourObj] ({userLoginID},{ObjectId}) " +
                                                        "ORDER BY  DestTypeId " +
                                                       $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                       $"FETCH NEXT   {_PageRows} ROWS ONLY ";




                    dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getPermessions));
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
                    DestNotHavePer_MVlist = new List<GetDestPerOnObjectModelView>();
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DestNotHavePer_MV = new GetDestPerOnObjectModelView
                            {

                                DestObjId = Convert.ToInt32(dt.Rows[i]["DestObjId"].ToString()),
                                DestObjTitle = dt.Rows[i]["DestTitle"].ToString(),
                                DestTypeId = Convert.ToInt32(dt.Rows[i]["DestTypeId"].ToString()),
                                DestTypeName = dt.Rows[i]["DestTypeName"].ToString()
                            };
                            DestNotHavePer_MVlist.Add(DestNotHavePer_MV);
                        }
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                            Data = new
                            {
                                TotalRows = DestNotHavePer_MVlist.Count,
                                MaxPage = Math.Ceiling(DestNotHavePer_MVlist.Count / (float)_PageRows),
                                CurrentPage = _PageNumber,
                                PageRows = _PageRows,
                                data = DestNotHavePer_MVlist
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
                                TotalRows = DestNotHavePer_MVlist.Count,
                                MaxPage = Math.Ceiling(DestNotHavePer_MVlist.Count / (float)_PageRows),
                                CurrentPage = _PageNumber,
                                PageRows = _PageRows,
                                data = DestNotHavePer_MVlist
                            }
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
        public async Task<ResponseModelView> GetUsersOrGroupsHavePermissionOnObject(int ObjectId, PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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
                    string getPermessions = " SELECT   DestObjId, DestTitle, DestTypeId, DestTypeName" +
                                           $" FROM     [User].[GetDestObjsHavePerOnSourObj] ({userLoginID},{ObjectId}) " +
                                                        "ORDER BY  DestTypeId " +
                                                       $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                       $"FETCH NEXT   {_PageRows} ROWS ONLY ";

                    dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getPermessions));
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
                    DestNotHavePer_MVlist = new List<GetDestPerOnObjectModelView>();
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DestNotHavePer_MV = new GetDestPerOnObjectModelView
                            {

                                DestObjId = Convert.ToInt32(dt.Rows[i]["DestObjId"].ToString()),
                                DestObjTitle = dt.Rows[i]["DestTitle"].ToString(),
                                DestTypeId = Convert.ToInt32(dt.Rows[i]["DestTypeId"].ToString()),
                                DestTypeName = dt.Rows[i]["DestTypeName"].ToString()
                            };
                            DestNotHavePer_MVlist.Add(DestNotHavePer_MV);
                        }
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                            Data = new
                            {
                                TotalRows = DestNotHavePer_MVlist.Count,
                                MaxPage = Math.Ceiling(DestNotHavePer_MVlist.Count / (float)_PageRows),
                                CurrentPage = _PageNumber,
                                PageRows = _PageRows,
                                data = DestNotHavePer_MVlist
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
                                TotalRows = DestNotHavePer_MVlist.Count,
                                MaxPage = Math.Ceiling(DestNotHavePer_MVlist.Count / (float)_PageRows),
                                CurrentPage = _PageNumber,
                                PageRows = _PageRows,
                                data = DestNotHavePer_MVlist
                            }
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