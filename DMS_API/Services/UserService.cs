using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using Microsoft.AspNetCore.Http.Headers;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace DMS_API.Services
{
    public class UserService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private UserModel User_M { get; set; }
        private List<UserModel> User_Mlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public UserService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region CURD Functions
        public async Task<ResponseModelView> Login(LoginModelView User_MV, string Lang)
        {
            try
            {
                if (ValidationService.IsEmpty(User_MV.Username) == true)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.UsernameMustEnter],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
                }
                else if (ValidationService.IsEmpty(User_MV.Password) == true)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.PasswordMustEnter],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
                }
                else
                {
                    if (User_MV.Password.Length < 8)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.Password8Characters],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    { // $ystemAdm!n 
                        string getUserInfo = "SELECT  UserID, FullName, UsUserName,  Role, UserIsActive, " +
                                            "         UsPhoneNo, UsEmail, UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgArName, OrgEnName, OrgKuName, Note " +
                                            "FROM     [User].V_Users " +
                                           $"WHERE    [UsUserName] = '{User_MV.Username}' AND UsPassword = '{SecurityService.PasswordEnecrypt(User_MV.Password)}' ";

                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getUserInfo));
                        if (dt == null)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.LoginFaild],
                                Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                            };
                            return Response_MV;
                        }
                        if (dt.Rows.Count > 0)
                        {
                            if (bool.Parse(dt.Rows[0]["UserIsActive"].ToString()) == false)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.UserNotActive],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotAcceptable).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                User_M = new UserModel
                                {
                                    UserID = Convert.ToInt32(dt.Rows[0]["UserID"].ToString()),
                                    FullName = dt.Rows[0]["FullName"].ToString(),
                                    UserName = dt.Rows[0]["UsUserName"].ToString(),
                                    Role = dt.Rows[0]["Role"].ToString(),
                                    IsActive = bool.Parse(dt.Rows[0]["UserIsActive"].ToString()),
                                    PhoneNo = dt.Rows[0]["UsPhoneNo"].ToString(),
                                    Email = dt.Rows[0]["UsEmail"].ToString(),
                                    UserEmpNo = dt.Rows[0]["UsUserEmpNo"].ToString(),
                                    UserIdintNo = dt.Rows[0]["UsUserIdintNo"].ToString(),
                                    IsOnLine = bool.Parse(dt.Rows[0]["UsIsOnLine"].ToString()),
                                    OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                                    OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                                    OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                                    Note = dt.Rows[0]["Note"].ToString()
                                };
                                var JWTtoken = SecurityService.GeneratTokenAuthenticate(User_M);
                                SessionService Session_S = new SessionService();
                                bool checkSession = await Session_S.AddSession(JWTtoken);
                                if (checkSession == false)
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
                                    Response_MV = new ResponseModelView
                                    {
                                        Success = true,
                                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                                        Data = new { data = User_M, TokenID = JWTtoken.TokenID }
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
                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NoData],
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
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }
        public async Task<ResponseModelView> GetUsersList(PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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
                    var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage FROM [User].V_Users");
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
                            string getUserInfo = "SELECT   UserID, FullName, UsUserName, Role, UserIsActive, UsPhoneNo, UsEmail, " +
                                                 "         UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgArName, OrgEnName, OrgKuName, Note " +
                                                 "FROM    [User].V_Users ";

                            dt = new DataTable();
                            dt = await Task.Run(() => dam.FireDataTable(getUserInfo));
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
                            User_Mlist = new List<UserModel>();
                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    User_M = new UserModel
                                    {
                                        UserID = Convert.ToInt32(dt.Rows[i]["UserID"].ToString()),
                                        FullName = dt.Rows[i]["FullName"].ToString(),
                                        UserName = dt.Rows[i]["UsUserName"].ToString(),
                                        //Password = dt.Rows[i]["UsPassword"].ToString(), // CONVERT(varchar(MAX), UsPassword) AS UsPassword,
                                        Role = dt.Rows[i]["Role"].ToString(),
                                        IsActive = bool.Parse(dt.Rows[i]["UserIsActive"].ToString()),
                                        PhoneNo = dt.Rows[i]["UsPhoneNo"].ToString(),
                                        Email = dt.Rows[i]["UsEmail"].ToString(),
                                        UserEmpNo = dt.Rows[i]["UsUserEmpNo"].ToString(),
                                        UserIdintNo = dt.Rows[i]["UsUserIdintNo"].ToString(),
                                        IsOnLine = bool.Parse(dt.Rows[i]["UsIsOnLine"].ToString()),
                                        //OrgOwner = Convert.ToInt32(dt.Rows[i]["UsOrgOwner"].ToString()),
                                        OrgArName = dt.Rows[i]["OrgArName"].ToString(),
                                        OrgEnName = dt.Rows[i]["OrgEnName"].ToString(),
                                        OrgKuName = dt.Rows[i]["OrgKuName"].ToString(),
                                        Note = dt.Rows[i]["Note"].ToString()
                                    };
                                    User_Mlist.Add(User_M);
                                }

                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = User_Mlist }
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
        public async Task<ResponseModelView> GetUsersByID(int id, RequestHeaderModelView RequestHeader)
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
                    string getUserInfo = "SELECT   UserID, FullName, UsUserName, Role, UserIsActive, UsPhoneNo, UsEmail, " +
                                         "         UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgArName, OrgEnName, OrgKuName, Note " +
                                        $"FROM     [User].V_Users WHERE UserID={id}";

                    dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getUserInfo));
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
                    User_Mlist = new List<UserModel>();
                    if (dt.Rows.Count > 0)
                    {
                        User_M = new UserModel
                        {
                            UserID = Convert.ToInt32(dt.Rows[0]["UserID"].ToString()),
                            FullName = dt.Rows[0]["FullName"].ToString(),
                            UserName = dt.Rows[0]["UsUserName"].ToString(),
                            Role = dt.Rows[0]["Role"].ToString(),
                            IsActive = bool.Parse(dt.Rows[0]["UserIsActive"].ToString()),
                            PhoneNo = dt.Rows[0]["UsPhoneNo"].ToString(),
                            Email = dt.Rows[0]["UsEmail"].ToString(),
                            UserEmpNo = dt.Rows[0]["UsUserEmpNo"].ToString(),
                            UserIdintNo = dt.Rows[0]["UsUserIdintNo"].ToString(),
                            IsOnLine = bool.Parse(dt.Rows[0]["UsIsOnLine"].ToString()),
                            OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                            OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                            OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                            Note = dt.Rows[0]["Note"].ToString()
                        };
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                            Data = User_M
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
        public async Task<ResponseModelView> AddUser(AddUserModelView AddUser_MV, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(AddUser_MV.UserName) == true || ValidationService.IsEmpty(AddUser_MV.Password) == true || ValidationService.IsEmpty(AddUser_MV.PhoneNo) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustFillInformation],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsUserName = '{AddUser_MV.UserName}' "));
                        if (checkDeblicate == 0)
                        {
                            string exeut = $"EXEC [User].[AddUserPro] '{AddUser_MV.UserName}', '{AddUser_MV.UserOwner}', '{AddUser_MV.OrgOwner}', '{AddUser_MV.Note}', '{AddUser_MV.FirstName}',  '{AddUser_MV.SecondName}', '{AddUser_MV.ThirdName}', '{AddUser_MV.LastName}', '{SecurityService.PasswordEnecrypt(AddUser_MV.Password)}', '{AddUser_MV.PhoneNo}', '{AddUser_MV.Email}', '{AddUser_MV.IsActive}', '{AddUser_MV.UserEmpNo}', '{AddUser_MV.UserIdintNo}' ";
                            var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut, "NewValue"));

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
                                Message = AddUser_MV.UserName + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
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

        #endregion

    }
}