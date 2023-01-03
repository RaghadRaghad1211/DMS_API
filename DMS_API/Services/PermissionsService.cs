using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;

namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Permissions
    /// </summary>
    public class PermissionsService
    {
        #region Properteis
        private IWebHostEnvironment Environment { get; }
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
        public PermissionsService(IWebHostEnvironment environment)
        {
            Environment = environment;
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Users Have IsRead & Admins To Do:
        /// Get (Folders & Documents in Folder) Or (Versions in Document) which depends on the User Permissions
        /// </summary>
        /// <param name="FolderChildsPermissions_MV">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, FolderChildsPermissions_MV.ParentId).Result;
                    bool checkManagePermission = result == null ? false : result.IsRead;
                    if (checkManagePermission == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int _PageNumber = FolderChildsPermissions_MV.PageNumber == 0 ? 1 : FolderChildsPermissions_MV.PageNumber;
                        int _PageRows = FolderChildsPermissions_MV.PageRows == 0 ? 1 : FolderChildsPermissions_MV.PageRows;
                        var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [Document].[GetChildsInParentWithPermissions] ({userLoginID}, {FolderChildsPermissions_MV.ParentId}) ");
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
                                        List<TrackingPathModel> TrackingPath_Mlist = new List<TrackingPathModel>();
                                        DataTable DtTracking = new DataTable();
                                        DtTracking = dam.FireDataTable($"SELECT TrackId, TrackName FROM [User].[GetFamilyTreeOfObject]({FolderChildsPermissions_MV.ParentId})");
                                        for (int i = 0; i < DtTracking.Rows.Count; i++)
                                        {
                                            TrackingPathModel TrackingPath_M = new TrackingPathModel
                                            {
                                                TrackId = Convert.ToInt32(DtTracking.Rows[i]["TrackId"].ToString()),
                                                TrackName = DtTracking.Rows[i]["TrackName"].ToString()
                                            };
                                            TrackingPath_Mlist.Add(TrackingPath_M);
                                        }
                                        Permessions_Mlist = new List<PermessionsModel>();
                                        if (dt.Rows.Count > 0)
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                Permessions_M = new PermessionsModel
                                                {
                                                    ParentId = FolderChildsPermissions_MV.ParentId,
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
                                                    TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                                    MaxPage = MaxTotal.Rows[0]["MaxPage"],
                                                    CurrentPage = _PageNumber,
                                                    PageRows = _PageRows,
                                                    TrackingPath = TrackingPath_Mlist,
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
                                                    TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                                    MaxPage = MaxTotal.Rows[0]["MaxPage"],
                                                    CurrentPage = _PageNumber,
                                                    PageRows = _PageRows,
                                                    TrackingPath = TrackingPath_Mlist,
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
                                List<TrackingPathModel> TrackingPath_Mlist = new List<TrackingPathModel>();
                                DataTable DtTracking = new DataTable();
                                DtTracking = dam.FireDataTable($"SELECT TrackId, TrackName FROM [User].[GetFamilyTreeOfObject]({FolderChildsPermissions_MV.ParentId})");
                                for (int i = 0; i < DtTracking.Rows.Count; i++)
                                {
                                    TrackingPathModel TrackingPath_M = new TrackingPathModel
                                    {
                                        TrackId = Convert.ToInt32(DtTracking.Rows[i]["TrackId"].ToString()),
                                        TrackName = DtTracking.Rows[i]["TrackName"].ToString()
                                    };
                                    TrackingPath_Mlist.Add(TrackingPath_M);
                                }

                                Permessions_Mlist = new List<PermessionsModel>();
                                if (dt.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        Permessions_M = new PermessionsModel
                                        {
                                            ParentId = FolderChildsPermissions_MV.ParentId,
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
                                            TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                            MaxPage = MaxTotal.Rows[0]["MaxPage"],
                                            CurrentPage = _PageNumber,
                                            PageRows = _PageRows,
                                            TrackingPath = TrackingPath_Mlist,
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
                                            TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                            MaxPage = MaxTotal.Rows[0]["MaxPage"],
                                            CurrentPage = _PageNumber,
                                            PageRows = _PageRows,
                                            TrackingPath = TrackingPath_Mlist,
                                            data = Permessions_Mlist
                                        }
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
        /// Users Have IsRead & Admins To Do:
        /// Search (Folders & Documents in Folder) Or (Versions in Document) which depends on the User Permissions
        /// </summary>
        /// <param name="FolderChildsPermissionsSearch_MV">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetChildsInParentWithPermissions_Search(FolderChildsPermissionsSearchModelView FolderChildsPermissionsSearch_MV, RequestHeaderModelView RequestHeader)
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
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, FolderChildsPermissionsSearch_MV.ParentId).Result;
                    bool checkManagePermission = result == null ? false : result.IsRead;
                    if (checkManagePermission == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int _PageNumber = FolderChildsPermissionsSearch_MV.PageNumber == 0 ? 1 : FolderChildsPermissionsSearch_MV.PageNumber;
                        int _PageRows = FolderChildsPermissionsSearch_MV.PageRows == 0 ? 1 : FolderChildsPermissionsSearch_MV.PageRows;
                        var MaxTotal = dam.FireDataTable($"SELECT   COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [Document].[GetChildsInParentWithPermissions_Search] ({userLoginID}, {FolderChildsPermissionsSearch_MV.ParentId}) " +
                                                         $"WHERE    SourTitle LIKE '{FolderChildsPermissionsSearch_MV.ChildTitle}%' ");
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
                            string getPermessions = " SELECT   SourObjId, SourTitle, SourType, SourTypeName," +
                                                    "          DestObjId, DestTitle, DestType, DestTypeName, " +
                                                    "          IsRead, IsWrite, IsManage, IsQR, SourCreationDate, " +
                                                    "          SourUserName, SourOrgArName, SourOrgEnName, SourOrgKuName  " +
                                                   $" FROM     [Document].[GetChildsInParentWithPermissions_Search] ({userLoginID}, {FolderChildsPermissionsSearch_MV.ParentId}) " +
                                                   $" WHERE    SourTitle LIKE '{FolderChildsPermissionsSearch_MV.ChildTitle}%' " +
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
                            List<TrackingPathModel> TrackingPath_Mlist = new List<TrackingPathModel>();
                            DataTable DtTracking = new DataTable();
                            DtTracking = dam.FireDataTable($"SELECT TrackId, TrackName FROM [User].[GetFamilyTreeOfObject]({FolderChildsPermissionsSearch_MV.ParentId})");
                            for (int i = 0; i < DtTracking.Rows.Count; i++)
                            {
                                TrackingPathModel TrackingPath_M = new TrackingPathModel
                                {
                                    TrackId = Convert.ToInt32(DtTracking.Rows[i]["TrackId"].ToString()),
                                    TrackName = DtTracking.Rows[i]["TrackName"].ToString()
                                };
                                TrackingPath_Mlist.Add(TrackingPath_M);
                            }
                            Permessions_Mlist = new List<PermessionsModel>();
                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    Permessions_M = new PermessionsModel
                                    {
                                        ParentId = FolderChildsPermissionsSearch_MV.ParentId,
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
                                        TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                        MaxPage = MaxTotal.Rows[0]["MaxPage"],
                                        CurrentPage = _PageNumber,
                                        PageRows = _PageRows,
                                        TrackingPath = TrackingPath_Mlist,
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
                                        TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                        MaxPage = MaxTotal.Rows[0]["MaxPage"],
                                        CurrentPage = _PageNumber,
                                        PageRows = _PageRows,
                                        TrackingPath = TrackingPath_Mlist,
                                        data = Permessions_Mlist
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
        /// Users Have IsRead & Admins To Do:
        /// Get Permissions on Object (Folders & Documents) by object Id 
        /// </summary>
        /// <param name="ObjectId">Id of Object want to get it's permissions</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, ObjectId).Result;
                    bool checkManagePermission = result == null ? false : result.IsRead;
                    if (checkManagePermission == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
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
        /// Add Permissions on object (Folder & Document) for Users & Groups
        /// </summary>
        /// <param name="AddPermissions_MVlist">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> AddPermissionsOnObject(List<AddPermissionsModelView> AddPermissions_MVlist, RequestHeaderModelView RequestHeader)
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
                    if (AddPermissions_MVlist.Count == 0)
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
                        foreach (var item in AddPermissions_MVlist)
                        {
                            var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, item.SourObjId).Result;
                            bool checkManagePermission = result == null ? false : result.IsManage;
                            if (checkManagePermission == false)
                            {
                                AddPermissions_MVlist.Remove(item);
                            }
                            else
                            {
                                string exeut = $"EXEC [User].[AddPermissionPro] '{item.SourObjId}','{item.SourClsId}', '{item.DestObjId}', '{item.DestClsId}','{item.PerRead}', '{item.PerWrite}', '{item.PerManage}', '{item.PerQR}' ";
                                var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));
                                if (outValue == 0.ToString() || outValue == null || outValue.Trim() == "")
                                {
                                    AddPermissions_MVlist.Remove(item);
                                }
                            }
                        }
                        if (AddPermissions_MVlist.Count == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
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
        /// Edit Permissions on object (Folder & Document) for Users & Groups
        /// </summary>
        /// <param name="EditPermissions_MVlist">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> EditPermissionsOnObject(List<EditPermissionsModelView> EditPermissions_MVlist, RequestHeaderModelView RequestHeader)
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
                    if (EditPermissions_MVlist.Count == 0)
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
                        foreach (var item in EditPermissions_MVlist)
                        {
                            var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, item.SourObjId).Result;
                            bool checkManagePermission = result == null ? false : result.IsManage;
                            if (checkManagePermission == false)
                            {
                                EditPermissions_MVlist.Remove(item);
                            }
                            else
                            {
                                string exeut = $"EXEC [User].[UpdatePermissionPro] '{item.SourObjId}', '{item.DestObjId}', '{item.PerRead}', '{item.PerWrite}', '{item.PerManage}', '{item.PerQR}' ";
                                var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));
                                if (outValue == 0.ToString() || outValue == null || outValue.Trim() == "")
                                {
                                    EditPermissions_MVlist.Remove(item);
                                }
                            }
                        }
                        if (EditPermissions_MVlist.Count == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
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
        /// Get (Users & Groups) they -- NOT HAVE -- Permission On Object (Folder & Document) by Object Id
        /// </summary>
        /// <param name="ObjectId">Id of Object want to get the users & groups they -- NOT HAVE -- Permissions on it</param>
        /// <param name="Pagination_MV">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, ObjectId).Result;
                    bool checkManagePermission = result == null ? false : result.IsManage;
                    if (checkManagePermission == true)
                    {
                        int _PageNumber = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                        int _PageRows = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
                        var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [User].[GetDestObjsNotHavePerOnSourObj] ({userLoginID},{ObjectId}) ");
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
                                        DestTypeName = dt.Rows[i]["DestTypeName"].ToString(),
                                        IsRead = false,
                                        IsWrite = false,
                                        IsManage = false,
                                        IsQR = false
                                    };
                                    DestNotHavePer_MVlist.Add(DestNotHavePer_MV);
                                }
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new
                                    {
                                        TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                        MaxPage = MaxTotal.Rows[0]["MaxPage"],
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
                                        TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                        MaxPage = MaxTotal.Rows[0]["MaxPage"],
                                        CurrentPage = _PageNumber,
                                        PageRows = _PageRows,
                                        data = DestNotHavePer_MVlist
                                    }
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
        /// <summary>
        /// Users Have IsManage & Admins To Do:
        /// Get (Users & Groups) they -- HAVE -- Permission On Object (Folder & Document) by Object Id
        /// </summary>
        /// <param name="ObjectId">Id of Object want to get the users & groups they -- HAVE -- Permissions on it</param>
        /// <param name="Pagination_MV">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        /// <returns></returns>
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
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, ObjectId).Result;
                    bool checkManagePermission = result == null ? false : result.IsManage;
                    if (checkManagePermission == true)
                    {
                        int _PageNumber = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                        int _PageRows = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
                        var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [User].[GetDestObjsHavePerOnSourObj] ({userLoginID},{ObjectId}) ");
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
                            string getPermessions = " SELECT   DestObjId, DestTitle, DestTypeId, DestTypeName, " +
                                                    "          IsRead, IsWrite, IsManage, IsQR " +
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
                                        DestTypeName = dt.Rows[i]["DestTypeName"].ToString(),
                                        IsRead = bool.Parse(dt.Rows[i]["IsRead"].ToString()),
                                        IsWrite = bool.Parse(dt.Rows[i]["IsWrite"].ToString()),
                                        IsManage = bool.Parse(dt.Rows[i]["IsManage"].ToString()),
                                        IsQR = bool.Parse(dt.Rows[i]["IsQR"].ToString())
                                    };
                                    DestNotHavePer_MVlist.Add(DestNotHavePer_MV);
                                }
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new
                                    {
                                        TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                        MaxPage = MaxTotal.Rows[0]["MaxPage"],
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
                                        TotalRows = MaxTotal.Rows[0]["TotalRows"],
                                        MaxPage = MaxTotal.Rows[0]["MaxPage"],
                                        CurrentPage = _PageNumber,
                                        PageRows = _PageRows,
                                        data = DestNotHavePer_MVlist
                                    }
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

        public async Task<ResponseModelView> GetQRcodePDFofDocument(QRLookupModel QRLookup_M, RequestHeaderModelView RequestHeader)
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
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, QRLookup_M.QrDocumentId).Result;
                    bool checkManagePermission = result == null ? false : result.IsQR;
                    if (checkManagePermission == true)
                    {
                        return await GlobalService.GenerateQRcodePDF(QRLookup_M, RequestHeader, Environment);
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