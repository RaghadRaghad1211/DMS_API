using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
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
        /// <param name="ParentChildsPermissions_MV">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetChildsInParentWithPermissions(ParentChildsPermissionsModelView ParentChildsPermissions_MV, RequestHeaderModelView RequestHeader)
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
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, ParentChildsPermissions_MV.ParentId).Result;
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
                        int _PageNumber = ParentChildsPermissions_MV.PageNumber == 0 ? 1 : ParentChildsPermissions_MV.PageNumber;
                        int _PageRows = ParentChildsPermissions_MV.PageRows == 0 ? 1 : ParentChildsPermissions_MV.PageRows;
                        var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [Document].[GetChildsInParentWithPermissions] ({userLoginID}, {ParentChildsPermissions_MV.ParentId}) ");
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
                            if (ParentChildsPermissions_MV.IsMoveAtion == true)
                            {
                                if (ParentChildsPermissions_MV.ObjectsMovable.Count <= 0)
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
                                    bool canOpen = await GlobalService.IsFolderOpenableToMoveInsideIt(ParentChildsPermissions_MV.ParentId, ParentChildsPermissions_MV.ObjectsMovable);
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
                                                                "          IsRead, IsWrite, IsManage, IsQR, SourCreationDate, " +
                                                                "          SourUserName, SourOrgArName, SourOrgEnName, SourOrgKuName  " +
                                                               $" FROM     [Document].[GetChildsInParentWithPermissions] ({userLoginID}, {ParentChildsPermissions_MV.ParentId}) " +
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
                                        DtTracking = dam.FireDataTable($"SELECT TrackId, TrackName FROM [User].[GetFamilyTreeOfObject]({ParentChildsPermissions_MV.ParentId})");
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
                                                    ParentId = ParentChildsPermissions_MV.ParentId,
                                                    SourObjId = Convert.ToInt32(dt.Rows[i]["SourObjId"].ToString()),
                                                    SourTitle = dt.Rows[i]["SourTitle"].ToString(),
                                                    SourType = Convert.ToInt32(dt.Rows[i]["SourType"].ToString()),
                                                    SourTypeName = dt.Rows[i]["SourTypeName"].ToString(),
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
                                                        "          IsRead, IsWrite, IsManage, IsQR, SourCreationDate, " +
                                                        "          SourUserName, SourOrgArName, SourOrgEnName, SourOrgKuName  " +
                                                       $" FROM     [Document].[GetChildsInParentWithPermissions] ({userLoginID}, {ParentChildsPermissions_MV.ParentId}) " +
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
                                DtTracking = dam.FireDataTable($"SELECT TrackId, TrackName FROM [User].[GetFamilyTreeOfObject]({ParentChildsPermissions_MV.ParentId})");
                                if (DtTracking != null)
                                {
                                    for (int i = 0; i < DtTracking.Rows.Count; i++)
                                    {
                                        TrackingPathModel TrackingPath_M = new TrackingPathModel
                                        {
                                            TrackId = Convert.ToInt32(DtTracking.Rows[i]["TrackId"].ToString()),
                                            TrackName = DtTracking.Rows[i]["TrackName"].ToString()
                                        };
                                        TrackingPath_Mlist.Add(TrackingPath_M);
                                    }
                                }

                                Permessions_Mlist = new List<PermessionsModel>();
                                if (dt.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        Permessions_M = new PermessionsModel
                                        {
                                            ParentId = ParentChildsPermissions_MV.ParentId,
                                            SourObjId = Convert.ToInt32(dt.Rows[i]["SourObjId"].ToString()),
                                            SourTitle = dt.Rows[i]["SourTitle"].ToString(),
                                            SourType = Convert.ToInt32(dt.Rows[i]["SourType"].ToString()),
                                            SourTypeName = dt.Rows[i]["SourTypeName"].ToString(),
                                            //DestObjId = Convert.ToInt32(dt.Rows[i]["DestObjId"].ToString()),
                                            //DestTitle = dt.Rows[i]["DestTitle"].ToString(),
                                            //DestType = Convert.ToInt32(dt.Rows[i]["DestType"].ToString()),
                                            //DestTypeName = dt.Rows[i]["DestTypeName"].ToString(),
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
        /// <param name="ParentChildsPermissionsSearch_MV">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetChildsInParentWithPermissions_Search(ParentChildsPermissionsSearchModelView ParentChildsPermissionsSearch_MV, RequestHeaderModelView RequestHeader)
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
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, ParentChildsPermissionsSearch_MV.ParentId).Result;
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
                        int _PageNumber = ParentChildsPermissionsSearch_MV.PageNumber == 0 ? 1 : ParentChildsPermissionsSearch_MV.PageNumber;
                        int _PageRows = ParentChildsPermissionsSearch_MV.PageRows == 0 ? 1 : ParentChildsPermissionsSearch_MV.PageRows;
                        var MaxTotal = dam.FireDataTable($"SELECT   COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [Document].[GetChildsInParentWithPermissions_Search] ({userLoginID}, {ParentChildsPermissionsSearch_MV.ParentId}) " +
                                                         $"WHERE    SourTitle LIKE '{ParentChildsPermissionsSearch_MV.ChildTitle}%' ");
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
                                                    "          IsRead, IsWrite, IsManage, IsQR, SourCreationDate, " +
                                                    "          SourUserName, SourOrgArName, SourOrgEnName, SourOrgKuName  " +
                                                   $" FROM     [Document].[GetChildsInParentWithPermissions_Search] ({userLoginID}, {ParentChildsPermissionsSearch_MV.ParentId}) " +
                                                   $" WHERE    SourTitle LIKE '{ParentChildsPermissionsSearch_MV.ChildTitle}%' " +
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
                            DtTracking = dam.FireDataTable($"SELECT TrackId, TrackName FROM [User].[GetFamilyTreeOfObject]({ParentChildsPermissionsSearch_MV.ParentId})");
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
                                        ParentId = ParentChildsPermissionsSearch_MV.ParentId,
                                        SourObjId = Convert.ToInt32(dt.Rows[i]["SourObjId"].ToString()),
                                        SourTitle = dt.Rows[i]["SourTitle"].ToString(),
                                        SourType = Convert.ToInt32(dt.Rows[i]["SourType"].ToString()),
                                        SourTypeName = dt.Rows[i]["SourTypeName"].ToString(),
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
                if (ObjectId == 0 || ObjectId.ToString().IsInt() == false)
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
                                    //DestObjId = Convert.ToInt32(dt.Rows[0]["DestObjId"].ToString()),
                                    //DestTitle = dt.Rows[0]["DestTitle"].ToString(),
                                    //DestType = Convert.ToInt32(dt.Rows[0]["DestType"].ToString()),
                                    //DestTypeName = dt.Rows[0]["DestTypeName"].ToString(),
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
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectPermissionForObject],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        foreach (var item in AddPermissions_MVlist)
                        {
                            if (item.PerRead == false && item.PerWrite == false && item.PerManage == false && item.PerQR == false)
                            {
                                AddPermissions_MVlist.Remove(item);
                            }
                            var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, item.SourObjId).Result;
                            bool checkManagePermission = result == null ? false : result.IsManage;
                            if (checkManagePermission == false) { AddPermissions_MVlist.Remove(item); }
                            if (AddPermissions_MVlist.Count == 0)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectPermissionForObject],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                string exeut = $"EXEC [User].[AddPermissionPro] '{userLoginID}','{item.SourObjId}','{item.SourClsId}', '{item.DestObjId}', '{item.DestClsId}','{true}', '{item.PerWrite}', '{item.PerManage}', '{item.PerQR}', '{item.PerToAllChilds}' ";
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
                                string exeut = $"EXEC [User].[UpdatePermissionPro] '{item.SourObjId}', '{item.DestObjId}', '{item.PerRead}', '{item.PerWrite}', '{item.PerManage}', '{item.PerQR}', '{false}' ";
                                var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));
                                if (outValue == 0.ToString() || outValue == null || outValue.Trim() == "")
                                {
                                    EditPermissions_MVlist.Remove(item);
                                }
                                else
                                {
                                    if (item.PerRead == false)
                                    {
                                        int checkFav = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].[V_Favourites] WHERE [ObjUserId]={item.DestObjId} AND [ObjFavId]={item.SourObjId} AND [IsActive]=1"));
                                        if (checkFav > 0)
                                        {
                                            dam.DoQuery($"UPDATE [User].[Favourites] SET [IsActive]=0  WHERE [ObjUserId]={item.DestObjId} AND [ObjFavId]={item.SourObjId}");
                                        }
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
        /// Get (Users & Groups) they -- NOT HAVE -- Permission On Object (Folder & Document) by Object Id,
        /// with searchable
        /// </summary>
        /// <param name="SearchUsersOrGroupsPermissionOnObject_MV">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetUsersOrGroupsNotHavePermissionOnObject(SearchUsersOrGroupsPermissionOnObject SearchUsersOrGroupsPermissionOnObject_MV, RequestHeaderModelView RequestHeader)
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
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, SearchUsersOrGroupsPermissionOnObject_MV.ObjectId).Result;
                    bool checkManagePermission = result == null ? false : result.IsManage;
                    if (checkManagePermission == true)
                    {
                        bool IsSearchable = false;
                        DataTable MaxTotal = new DataTable();
                        string? getPermessions;
                        if (SearchUsersOrGroupsPermissionOnObject_MV.TitleSearch.IsEmpty() == false) { IsSearchable = true; }

                        int _PageNumber = SearchUsersOrGroupsPermissionOnObject_MV.PageNumber == 0 ? 1 : SearchUsersOrGroupsPermissionOnObject_MV.PageNumber;
                        int _PageRows = SearchUsersOrGroupsPermissionOnObject_MV.PageRows == 0 ? 1 : SearchUsersOrGroupsPermissionOnObject_MV.PageRows;

                        if (IsSearchable == true)
                        {
                            MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [User].[GetDestObjsNotHavePerOnSourObj] ({userLoginID},{SearchUsersOrGroupsPermissionOnObject_MV.ObjectId}) " +
                                                         $"WHERE    DestTitle LIKE '{SearchUsersOrGroupsPermissionOnObject_MV.TitleSearch}%'  ");
                        }
                        else
                        {
                            MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [User].[GetDestObjsNotHavePerOnSourObj] ({userLoginID},{SearchUsersOrGroupsPermissionOnObject_MV.ObjectId}) ");
                        }

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
                            if (IsSearchable == true)
                            {
                                getPermessions = " SELECT      DestObjId, DestTitle, DestTypeId, DestTypeName" +
                                                 $" FROM       [User].[GetDestObjsNotHavePerOnSourObj] ({userLoginID},{SearchUsersOrGroupsPermissionOnObject_MV.ObjectId}) " +
                                                 $" WHERE      DestTitle LIKE '{SearchUsersOrGroupsPermissionOnObject_MV.TitleSearch}%' " +
                                                  "ORDER BY    DestTypeId " +
                                                 $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                 $"FETCH NEXT   {_PageRows} ROWS ONLY ";
                            }
                            else
                            {
                                getPermessions = " SELECT    DestObjId, DestTitle, DestTypeId, DestTypeName" +
                                               $" FROM       [User].[GetDestObjsNotHavePerOnSourObj] ({userLoginID},{SearchUsersOrGroupsPermissionOnObject_MV.ObjectId}) " +
                                                "ORDER BY    DestTypeId " +
                                               $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                               $"FETCH NEXT   {_PageRows} ROWS ONLY ";
                            }
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
        /// Get (Users & Groups) they -- HAVE -- Permission On Object (Folder & Document) by Object Id,
        /// with searchable
        /// </summary>
        /// <param name="SearchUsersOrGroupsPermissionOnObject_MV">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        /// <returns></returns>
        public async Task<ResponseModelView> GetUsersOrGroupsHavePermissionOnObject(SearchUsersOrGroupsPermissionOnObject SearchUsersOrGroupsPermissionOnObject_MV, RequestHeaderModelView RequestHeader)
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
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, SearchUsersOrGroupsPermissionOnObject_MV.ObjectId).Result;
                    bool checkManagePermission = result == null ? false : result.IsManage;
                    if (checkManagePermission == true)
                    {
                        bool IsSearchable = false;
                        DataTable MaxTotal = new DataTable();
                        string? getPermessions;
                        if (SearchUsersOrGroupsPermissionOnObject_MV.TitleSearch.IsEmpty() == false) { IsSearchable = true; }

                        int _PageNumber = SearchUsersOrGroupsPermissionOnObject_MV.PageNumber == 0 ? 1 : SearchUsersOrGroupsPermissionOnObject_MV.PageNumber;
                        int _PageRows = SearchUsersOrGroupsPermissionOnObject_MV.PageRows == 0 ? 1 : SearchUsersOrGroupsPermissionOnObject_MV.PageRows;
                        if (IsSearchable == true)
                        {
                            MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                        $"FROM     [User].[GetDestObjsHavePerOnSourObj] ({userLoginID},{SearchUsersOrGroupsPermissionOnObject_MV.ObjectId}) " +
                                                        $"WHERE    DestTitle LIKE '{SearchUsersOrGroupsPermissionOnObject_MV.TitleSearch}%'  ");
                        }
                        else
                        {
                            MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                         $"FROM     [User].[GetDestObjsHavePerOnSourObj] ({userLoginID},{SearchUsersOrGroupsPermissionOnObject_MV.ObjectId}) ");
                        }

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
                            if (IsSearchable == true)
                            {
                                getPermessions = " SELECT    DestObjId, DestTitle, DestTypeId, DestTypeName, " +
                                                    "          IsRead, IsWrite, IsManage, IsQR " +
                                                   $" FROM     [User].[GetDestObjsHavePerOnSourObj] ({userLoginID},{SearchUsersOrGroupsPermissionOnObject_MV.ObjectId}) " +
                                                   $" WHERE     DestTitle LIKE '{SearchUsersOrGroupsPermissionOnObject_MV.TitleSearch}%' " +
                                                    "ORDER BY  DestTypeId " +
                                                   $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                   $"FETCH NEXT   {_PageRows} ROWS ONLY ";
                            }
                            else
                            {
                                getPermessions = " SELECT   DestObjId, DestTitle, DestTypeId, DestTypeName, " +
                                                 "          IsRead, IsWrite, IsManage, IsQR " +
                                                $" FROM     [User].[GetDestObjsHavePerOnSourObj] ({userLoginID},{SearchUsersOrGroupsPermissionOnObject_MV.ObjectId}) " +
                                                 "ORDER BY  DestTypeId " +
                                                $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                $"FETCH NEXT   {_PageRows} ROWS ONLY ";
                            }


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
        /// <summary>
        /// Users Have IsQR & Admins To Do:
        /// Generate PFD of document with QR code.
        /// </summary>
        /// <param name="QRLookup_M">Body Parameter</param>
        /// <param name="RequestHeader">Header Parameter</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GenerateQRcodePDFofDocument(QRLookupModel QRLookup_M, RequestHeaderModelView RequestHeader)
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
        /// <summary>
        /// Everyone To Do:
        /// Read public QR code
        /// </summary>
        /// <param name="QRcode">QR code</param>
        /// <param name="Lang">Header Language</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> ReadQRcodePDFofDocument(string QRcode, string Lang)
        {
            try
            {
                if (QRcode.IsEmpty() == true)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.QrCodeIsEmpty],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
                }
                else
                {
                    string getQrInfo = $"SELECT [QrObjId], [QrIsPraivet], [QrIsActive]  FROM [Main].[QRLookup] WHERE [QrId]='{QRcode}'  ";
                    DataTable dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getQrInfo));
                    if (dt == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError],
                            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                        };
                        return Response_MV;

                    }
                    else
                    {
                        if (dt.Rows.Count <= 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.QrCodeIsWrong],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            if (bool.Parse(dt.Rows[0]["QrIsActive"].ToString()) == false)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NotActive],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else if (bool.Parse(dt.Rows[0]["QrIsPraivet"].ToString()) == true)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.QrCodeIsPraivet],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotAcceptable).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new { DocumentFilePath = await GlobalService.GetFullPathOfDocumentNameInServerFolder(Convert.ToInt32(dt.Rows[0]["QrObjId"].ToString()), Environment) }
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
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }
        /// <summary>
        /// Everyone have account in the system To Do:
        /// Read private QR code
        /// </summary>
        /// <param name="QRcode">QR code</param>
        /// <param name="Login_MV">Body Parameters</param>
        /// <param name="Lang">Header Language</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> ReadQRcodePDFofDocumentPrivate(string QRcode, LoginModelView Login_MV, string Lang)
        {
            try
            {
                if (QRcode.IsEmpty() == true)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.QrCodeIsEmpty],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
                }
                else
                {
                    string getQrInfo = $"SELECT [QrObjId], [QrIsPraivet], [QrIsActive]  FROM [Main].[QRLookup] WHERE [QrId]='{QRcode}'  ";
                    DataTable dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getQrInfo));
                    if (dt == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError],
                            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                        };
                        return Response_MV;

                    }
                    else
                    {
                        if (dt.Rows.Count <= 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.QrCodeIsWrong],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            if (bool.Parse(dt.Rows[0]["QrIsActive"].ToString()) == false)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NotActive],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                if (bool.Parse(dt.Rows[0]["QrIsPraivet"].ToString()) == true)
                                {
                                    string validation = ValidationService.IsEmptyList(Login_MV);
                                    if (ValidationService.IsEmpty(validation) == false)
                                    {
                                        Response_MV = new ResponseModelView
                                        {
                                            Success = false,
                                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.MustFillInformation] + "  " + $"({validation})",
                                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                        };
                                        return Response_MV;
                                    }
                                    else
                                    {
                                        int checkUsername = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsUserName = '{Login_MV.Username}' "));
                                        if (checkUsername == 0)
                                        {
                                            Response_MV = new ResponseModelView
                                            {
                                                Success = false,
                                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.InvalidUsername],
                                                Data = new HttpResponseMessage(HttpStatusCode.NotAcceptable).StatusCode
                                            };
                                            return Response_MV;
                                        }
                                        else
                                        {
                                            int checkPassword = Convert.ToInt32(dam.FireSQL("SELECT  COUNT(*)   FROM    [User].Users " +
                                                                                       $"WHERE   UsUserName = '{Login_MV.Username}' AND " +
                                                                                       $"        UsPassword = '{SecurityService.PasswordEnecrypt(Login_MV.Password.Trim(), Login_MV.Username.Trim())}' "));
                                            if (checkPassword == 0)
                                            {
                                                Response_MV = new ResponseModelView
                                                {
                                                    Success = false,
                                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.InvalidPassword],
                                                    Data = new HttpResponseMessage(HttpStatusCode.NotAcceptable).StatusCode
                                                };
                                                return Response_MV;
                                            }
                                            else
                                            {
                                                Response_MV = new ResponseModelView
                                                {
                                                    Success = true,
                                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                                                    Data = new { DocumentFilePath = await GlobalService.GetFullPathOfDocumentNameInServerFolder(Convert.ToInt32(dt.Rows[0]["QrObjId"].ToString()), Environment) }
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
                                        Success = true,
                                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                                        Data = new { DocumentFilePath = await GlobalService.GetFullPathOfDocumentNameInServerFolder(Convert.ToInt32(dt.Rows[0]["QrObjId"].ToString()), Environment) }
                                    };
                                    return Response_MV;
                                }
                            }
                            //else
                            //{
                            //    Response_MV = new ResponseModelView
                            //    {
                            //        Success = true,
                            //        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                            //        Data = new { DocumentFilePath = await GlobalService.GetFullPathOfDocumentNameInServerFolder(Convert.ToInt32(dt.Rows[0]["QrObjId"].ToString()), Environment) }
                            //    };
                            //    return Response_MV;
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }

        #endregion
    }
}