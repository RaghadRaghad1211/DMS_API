using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
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

        #region Functions
        public async Task<ResponseModelView> Login(LoginModelView User_MV, string Lang)
        {
            try
            {
                #region My Test validation
                //if (ValidationService.IsEmpty(User_MV.Username) == true)
                //{
                //    Response_MV = new ResponseModelView
                //    {
                //        Success = false,
                //        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.UsernameMustEnter],
                //        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                //    };
                //    return Response_MV;
                //}
                //else if (ValidationService.IsEmpty(User_MV.Password) == true)
                //{
                //    Response_MV = new ResponseModelView
                //    {
                //        Success = false,
                //        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.PasswordMustEnter],
                //        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                //    };
                //    return Response_MV;
                //}
                #endregion

                string validation = ValidationService.IsEmptyList(User_MV);
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
                    if (User_MV.Password.Trim().Length < 8)
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
                        string getUserInfo = "SELECT  UserID,UsFirstName,UsSecondName,UsThirdName,UsLastName, FullName, UsUserName,  Role, IsOrgAdmin, UserIsActive, UsPhoneNo, " +
                                            "         UsEmail, UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgOwner, OrgArName, OrgEnName, OrgKuName, Note " +
                                            "FROM     [User].V_Users " +
                                           $"WHERE    [UsUserName] = '{User_MV.Username.Trim()}' AND UsPassword = '{SecurityService.PasswordEnecrypt(User_MV.Password.Trim(), User_MV.Username.Trim())}' ";

                        dt = new DataTable();
                        await Task.Delay(2000);
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
                                    FirstName = dt.Rows[0]["UsFirstName"].ToString(),
                                    SecondName = dt.Rows[0]["UsSecondName"].ToString(),
                                    ThirdName = dt.Rows[0]["UsThirdName"].ToString(),
                                    LastName = dt.Rows[0]["UsLastName"].ToString(),
                                    FullName = dt.Rows[0]["FullName"].ToString(),
                                    UserName = dt.Rows[0]["UsUserName"].ToString(),
                                    Role = dt.Rows[0]["Role"].ToString(),
                                    IsOrgAdmin = bool.Parse(dt.Rows[0]["IsOrgAdmin"].ToString()),
                                    IsActive = bool.Parse(dt.Rows[0]["UserIsActive"].ToString()),
                                    PhoneNo = dt.Rows[0]["UsPhoneNo"].ToString(),
                                    Email = dt.Rows[0]["UsEmail"].ToString(),
                                    UserEmpNo = dt.Rows[0]["UsUserEmpNo"].ToString(),
                                    UserIdintNo = dt.Rows[0]["UsUserIdintNo"].ToString(),
                                    IsOnLine = bool.Parse(dt.Rows[0]["UsIsOnLine"].ToString()),
                                    OrgOwnerID = Convert.ToInt32(dt.Rows[0]["OrgOwner"].ToString()),
                                    OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                                    OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                                    OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                                    Note = dt.Rows[0]["Note"].ToString(),
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
        public async Task<ResponseModelView> ResetPasswordAdmin(int id, RequestHeaderModelView RequestHeader)
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
                    DataTable dtEmail = new DataTable();
                    dtEmail = await Task.Run(() => dam.FireDataTable($"SELECT UsUserName, UsPhoneNo, UsEmail FROM [User].Users WHERE UsId ={id} "));
                    if (dtEmail.Rows.Count > 0)
                    {
                        string RounPass = SecurityService.RoundomPassword(10);
                        bool isResetEmail = SecurityService.SendEmail(dtEmail.Rows[0]["UsEmail"].ToString(),
                             MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EmailSubjectPasswordIsReset],
                             MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EmailBodyPasswordIsReset] + RounPass);
                        bool isResetOTP = await SecurityService.SendOTP(dtEmail.Rows[0]["UsPhoneNo"].ToString(), MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.PhonePasswordIsReset] + RounPass);
                        if (isResetEmail == true && isResetOTP == true)
                        {
                            string reset = $"UPDATE [User].Users SET UsPassword='{SecurityService.PasswordEnecrypt(RounPass, dtEmail.Rows[0]["UsUserName"].ToString())}' WHERE UsId ={id} ";
                            await Task.Run(() => dam.DoQuery(reset));
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsReset],
                                Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError],
                                Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                            };
                            return Response_MV;
                        }
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
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
        public async Task<ResponseModelView> ResetPasswordUser(string username, string Lang)
        {
            try
            {
                DataTable dtEmail = new DataTable();
                dtEmail = await Task.Run(() => dam.FireDataTable($"SELECT UsPhoneNo, UsEmail FROM [User].Users WHERE UsUserName ='{username}' "));
                if (dtEmail.Rows.Count > 0)
                {
                    string RounPass = SecurityService.RoundomPassword(10);
                    bool isResetEmail = SecurityService.SendEmail(dtEmail.Rows[0]["UsEmail"].ToString(),
                         MessageService.MsgDictionary[Lang.ToLower()][MessageService.EmailSubjectPasswordIsReset],
                         MessageService.MsgDictionary[Lang.ToLower()][MessageService.EmailBodyPasswordIsReset] + RounPass);
                    bool isResetOTP = await SecurityService.SendOTP(dtEmail.Rows[0]["UsPhoneNo"].ToString(), MessageService.MsgDictionary[Lang.ToLower()][MessageService.PhonePasswordIsReset] + RounPass);
                    if (isResetEmail == true && isResetOTP == true)
                    {
                        string reset = $"UPDATE [User].Users SET UsPassword='{SecurityService.PasswordEnecrypt(RounPass, username)}' WHERE UsUserName ='{username}' ";
                        await Task.Run(() => dam.DoQuery(reset));
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.IsReset],
                            Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError],
                            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                        };
                        return Response_MV;
                    }
                }
                else
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.IsExist],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
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
        public async Task<ResponseModelView> ChangePassword(ChangePasswordModelView ChangePassword_MV, RequestHeaderModelView RequestHeader)
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
                    string validation = ValidationService.IsEmptyList(ChangePassword_MV);
                    if (ValidationService.IsEmpty(validation) == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustFillInformation] + "  " + $"({validation})",
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ChangePassword_MV.NewPassword.Length < 8)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.Password8Characters],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ChangePassword_MV.NewPasswordConfirm.Trim() != ChangePassword_MV.NewPassword.Trim())
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ConfirmPasswordIsIncorrect],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int checkExist = Convert.ToInt16(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsId ={userLoginID} AND  UsPassword ='{SecurityService.PasswordEnecrypt(ChangePassword_MV.OldPassword, dam.FireSQL($"SELECT UsUserName FROM [User].Users WHERE UsId ={userLoginID}"))}' "));
                        if (checkExist > 0)
                        {
                            string change = $"UPDATE [User].Users SET UsPassword='{SecurityService.PasswordEnecrypt(ChangePassword_MV.NewPassword, dam.FireSQL($"SELECT UsUserName FROM [User].Users WHERE UsId ={userLoginID}"))}' WHERE UsId ={userLoginID} ";
                            await Task.Run(() => dam.DoQuery(change));
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ChangePasswordSuccess],
                                Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.OldPasswordNotCorrect],
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
                    int CurrentPage = _PageNumber; int PageRows = _PageRows;

                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                     $"FROM [User].V_Users  WHERE [OrgOwner] IN (SELECT [OrgId] FROM [User].[GetOrgsbyUserId]({userLoginID})) AND [USERID] !={userLoginID} ");
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
                            string getUserInfo = "SELECT      UserID, UsFirstName, UsSecondName, UsThirdName, UsLastName, FullName, UsUserName, Role, IsOrgAdmin, UserIsActive, UsPhoneNo, UsEmail, " +
                                                 "            UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgOwner, [User].V_Users.OrgArName, [User].V_Users.OrgEnName, [User].V_Users.OrgKuName, Note " +
                                                 "FROM        [User].V_Users " +
                                                $"INNER JOIN  [User].GetOrgsbyUserId({userLoginID}) AS GetOrgsbyUserId ON[User].V_Users.OrgOwner = GetOrgsbyUserId.OrgId " +
                                                 "ORDER BY    UserID " +
                                                $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                $"FETCH NEXT   {_PageRows} ROWS ONLY ";

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
                                        FirstName = dt.Rows[i]["UsFirstName"].ToString(),
                                        SecondName = dt.Rows[i]["UsSecondName"].ToString(),
                                        ThirdName = dt.Rows[i]["UsThirdName"].ToString(),
                                        LastName = dt.Rows[i]["UsLastName"].ToString(),
                                        FullName = dt.Rows[i]["FullName"].ToString(),
                                        UserName = dt.Rows[i]["UsUserName"].ToString(),
                                        Role = dt.Rows[i]["Role"].ToString(),
                                        IsOrgAdmin = bool.Parse(dt.Rows[i]["IsOrgAdmin"].ToString()),
                                        IsActive = bool.Parse(dt.Rows[i]["UserIsActive"].ToString()),
                                        PhoneNo = dt.Rows[i]["UsPhoneNo"].ToString(),
                                        Email = dt.Rows[i]["UsEmail"].ToString(),
                                        UserEmpNo = dt.Rows[i]["UsUserEmpNo"].ToString(),
                                        UserIdintNo = dt.Rows[i]["UsUserIdintNo"].ToString(),
                                        IsOnLine = bool.Parse(dt.Rows[i]["UsIsOnLine"].ToString()),
                                        OrgOwnerID = Convert.ToInt32(dt.Rows[i]["OrgOwner"].ToString()),
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
                                    Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, PageRows, data = User_Mlist }
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
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    string getUserInfo = "SELECT   UserID, UsFirstName, UsSecondName, UsThirdName, UsLastName, FullName, UsUserName, Role, IsOrgAdmin, UserIsActive, UsPhoneNo, UsEmail, " +
                                         "         UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgOwner, OrgArName, OrgEnName, OrgKuName, Note " +
                                        $"FROM     [User].V_Users WHERE UserID={id} ";

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
                            FirstName = dt.Rows[0]["UsFirstName"].ToString(),
                            SecondName = dt.Rows[0]["UsSecondName"].ToString(),
                            ThirdName = dt.Rows[0]["UsThirdName"].ToString(),
                            LastName = dt.Rows[0]["UsLastName"].ToString(),
                            FullName = dt.Rows[0]["FullName"].ToString(),
                            UserName = dt.Rows[0]["UsUserName"].ToString(),
                            Role = dt.Rows[0]["Role"].ToString(),
                            IsOrgAdmin = bool.Parse(dt.Rows[0]["IsOrgAdmin"].ToString()),
                            IsActive = bool.Parse(dt.Rows[0]["UserIsActive"].ToString()),
                            PhoneNo = dt.Rows[0]["UsPhoneNo"].ToString(),
                            Email = dt.Rows[0]["UsEmail"].ToString(),
                            UserEmpNo = dt.Rows[0]["UsUserEmpNo"].ToString(),
                            UserIdintNo = dt.Rows[0]["UsUserIdintNo"].ToString(),
                            IsOnLine = bool.Parse(dt.Rows[0]["UsIsOnLine"].ToString()),
                            OrgOwnerID = Convert.ToInt32(dt.Rows[0]["OrgOwner"].ToString()),
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
                    if (ValidationService.IsEmpty(AddUser_MV.FirstName) == true || ValidationService.IsEmpty(AddUser_MV.SecondName) == true || ValidationService.IsEmpty(AddUser_MV.ThirdName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FSTname],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ValidationService.IsEmpty(AddUser_MV.UserName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.UsernameMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ValidationService.IsPhoneNumber(AddUser_MV.PhoneNo.Trim()) == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.PhoneIncorrect],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ValidationService.IsEmail(AddUser_MV.Email.ToLower().Trim()) == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EmailIncorrect],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (AddUser_MV.OrgOwner == 0) // (AddUser_MV.OrgOwner == 0 || AddUser_MV.UserOwner == 0)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustFillInformation] + "OrgOwner , UserOwner",
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ValidationService.IsEmpty(AddUser_MV.Password.Trim()) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.PasswordMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (AddUser_MV.Password.Trim().Length < 8)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.Password8Characters],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (AddUser_MV.PasswordConfirm.Trim() != AddUser_MV.Password.Trim())
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ConfirmPasswordIsIncorrect],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }

                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int checkDeblicateUsername = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsUserName = '{AddUser_MV.UserName}' "));
                        if (checkDeblicateUsername == 0)
                        {
                            int checkDeblicatePhone = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsPhoneNo = '{AddUser_MV.PhoneNo}' "));
                            if (checkDeblicatePhone == 0)
                            {
                                string exeut = $"EXEC [User].[AddUserPro] '{AddUser_MV.UserName.Trim()}', '{userLoginID}', '{AddUser_MV.OrgOwner}', '{AddUser_MV.Note}', '{AddUser_MV.FirstName.Trim()}',  '{AddUser_MV.SecondName.Trim()}', '{AddUser_MV.ThirdName.Trim()}', '{AddUser_MV.LastName.Trim()}', '{SecurityService.PasswordEnecrypt(AddUser_MV.Password.Trim(), AddUser_MV.UserName.Trim())}', '{AddUser_MV.PhoneNo.Trim()}', '{AddUser_MV.Email.ToLower().Trim()}', '{AddUser_MV.IsActive}', '{AddUser_MV.UserEmpNo}', '{AddUser_MV.UserIdintNo}', '{AddUser_MV.IsOrgAdmin}' ";
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
                                    Message = AddUser_MV.UserName + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.PhoneIsExist],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = AddUser_MV.UserName + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.UsernameIsExist],
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
        public async Task<ResponseModelView> EditUser(EditUserModelView EditUser_MV, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(EditUser_MV.FirstName) == true || ValidationService.IsEmpty(EditUser_MV.SecondName) == true || ValidationService.IsEmpty(EditUser_MV.ThirdName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FSTname],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ValidationService.IsPhoneNumber(EditUser_MV.PhoneNo.Trim()) == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.PhoneIncorrect],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ValidationService.IsEmail(EditUser_MV.Email.ToLower().Trim()) == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EmailIncorrect],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (EditUser_MV.OrgOwner == 0)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustFillInformation] + "OrgOwner",
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }

                    else
                    {
                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsId = {EditUser_MV.UserID} "));
                        if (checkDeblicate > 0)
                        {
                            string exeut = $"EXEC [User].[UpdateUserPro] '{EditUser_MV.UserID}', '{EditUser_MV.IsActive}',  '{EditUser_MV.OrgOwner}', '{EditUser_MV.Note}', '{EditUser_MV.FirstName.Trim()}', '{EditUser_MV.SecondName.Trim()}', '{EditUser_MV.ThirdName.Trim()}', '{EditUser_MV.LastName.Trim()}', '{EditUser_MV.PhoneNo.Trim()}', '{EditUser_MV.Email.ToLower().Trim()}', '{EditUser_MV.UserEmpNo}', '{EditUser_MV.UserIdintNo}', '{EditUser_MV.IsOrgAdmin}' ";
                            var outValue = await Task.Run(() => dam.DoQueryExecProcedure(exeut));

                            if (outValue == null || outValue.Trim() == "")
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
                                if (outValue == 0.ToString())
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
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = EditUser_MV.UserID + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsNotExist],
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
        public async Task<ResponseModelView> SearchUsersByUserName(string username, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(username) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.UsernameMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        //int OrgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {ResponseSession.Data} "));
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        string getUserInfo = "SELECT   UserID, UsFirstName, UsSecondName, UsThirdName, UsLastName, FullName, UsUserName, Role, IsOrgAdmin, UserIsActive, UsPhoneNo, UsEmail, " +
                                             "         UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgArName, OrgEnName, OrgKuName, Note " +
                                            $"FROM     [User].V_Users WHERE UsUserName LIKE '{username}%' AND [OrgOwner] IN (SELECT [OrgId] FROM [User].[GetOrgsbyUserId]({userLoginID})) AND [USERID] !={userLoginID} ";

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
                                    FirstName = dt.Rows[i]["UsFirstName"].ToString(),
                                    SecondName = dt.Rows[i]["UsSecondName"].ToString(),
                                    ThirdName = dt.Rows[i]["UsThirdName"].ToString(),
                                    LastName = dt.Rows[i]["UsLastName"].ToString(),
                                    FullName = dt.Rows[i]["FullName"].ToString(),
                                    UserName = dt.Rows[i]["UsUserName"].ToString(),
                                    Role = dt.Rows[i]["Role"].ToString(),
                                    IsOrgAdmin = bool.Parse(dt.Rows[i]["IsOrgAdmin"].ToString()),
                                    IsActive = bool.Parse(dt.Rows[i]["UserIsActive"].ToString()),
                                    PhoneNo = dt.Rows[i]["UsPhoneNo"].ToString(),
                                    Email = dt.Rows[i]["UsEmail"].ToString(),
                                    UserEmpNo = dt.Rows[i]["UsUserEmpNo"].ToString(),
                                    UserIdintNo = dt.Rows[i]["UsUserIdintNo"].ToString(),
                                    IsOnLine = bool.Parse(dt.Rows[i]["UsIsOnLine"].ToString()),
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
                                Data = User_Mlist
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
        public async Task<ResponseModelView> SearchUsersAdvance(SearchUserModelView SearchUser_MV, RequestHeaderModelView RequestHeader)
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
                    int _PageNumber = SearchUser_MV.PageNumber == 0 ? 1 : SearchUser_MV.PageNumber;
                    int _PageRows = SearchUser_MV.PageRows == 0 ? 1 : SearchUser_MV.PageRows;
                    int CurrentPage = _PageNumber; int PageRows = _PageRows;
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    string where = GlobalService.GetUserSearchColumn(SearchUser_MV);
                    var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                      $"FROM [User].V_Users {where}  AND [OrgOwner] IN (SELECT [OrgId] FROM [User].[GetOrgsbyUserId]({userLoginID})) AND [USERID] !={userLoginID} ");
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
                            string getUserInfo = "SELECT * FROM (SELECT   UserID,UsFirstName,UsSecondName,UsThirdName,UsLastName, FullName, UsUserName, Role, IsOrgAdmin, UserIsActive, UsPhoneNo, UsEmail, " +
                                             "         UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgArName, OrgEnName, OrgKuName, Note " +
                                            $"FROM     [User].V_Users  " +
                                            $"{where}  AND [OrgOwner] IN (SELECT [OrgId] FROM [User].[GetOrgsbyUserId]({userLoginID})) AND [USERID] !={userLoginID}) AS TAB  ORDER BY UserID " +
                                            $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                            $"FETCH NEXT   {_PageRows} ROWS ONLY ";

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
                                        FirstName = dt.Rows[i]["UsFirstName"].ToString(),
                                        SecondName = dt.Rows[i]["UsSecondName"].ToString(),
                                        ThirdName = dt.Rows[i]["UsThirdName"].ToString(),
                                        LastName = dt.Rows[i]["UsLastName"].ToString(),

                                        FullName = dt.Rows[i]["FullName"].ToString(),
                                        UserName = dt.Rows[i]["UsUserName"].ToString(),
                                        Role = dt.Rows[i]["Role"].ToString(),
                                        IsOrgAdmin = bool.Parse(dt.Rows[i]["IsOrgAdmin"].ToString()),
                                        IsActive = bool.Parse(dt.Rows[i]["UserIsActive"].ToString()),
                                        PhoneNo = dt.Rows[i]["UsPhoneNo"].ToString(),
                                        Email = dt.Rows[i]["UsEmail"].ToString(),
                                        UserEmpNo = dt.Rows[i]["UsUserEmpNo"].ToString(),
                                        UserIdintNo = dt.Rows[i]["UsUserIdintNo"].ToString(),
                                        IsOnLine = bool.Parse(dt.Rows[i]["UsIsOnLine"].ToString()),
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
                                    Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, PageRows, data = User_Mlist }
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
        public async Task<ResponseModelView> EditContact(EditMyContactModelView EditMyContact_MV, RequestHeaderModelView RequestHeader)
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
                    string validation = ValidationService.IsEmptyList(EditMyContact_MV);
                    if (ValidationService.IsEmpty(validation) == false)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustFillInformation] + "  " + $"({validation})",
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int checkExist = Convert.ToInt16(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsId ={userLoginID} "));
                        if (checkExist > 0)
                        {
                            if (ValidationService.IsPhoneNumber(EditMyContact_MV.MyPhoneNo) == false)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.PhoneIncorrect],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else if (ValidationService.IsEmail(EditMyContact_MV.MyEmail) == false)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EmailIncorrect],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                string edit = $"UPDATE [User].Users SET UsEmail='{EditMyContact_MV.MyEmail}', UsPhoneNo='{EditMyContact_MV.MyPhoneNo}' WHERE UsId ={userLoginID} ";
                                await Task.Run(() => dam.DoQuery(edit));
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
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EditFaild],
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

        #region Test
        //public async Task<ResponseModelView> GetOrgsParentWithChilds(RequestHeaderModelView RequestHeader)
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
        //            int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
        //            List<OrgModel> Org_Mlist = new List<OrgModel>();
        //            Org_Mlist = await GlobalService.GetOrgsParentWithChildsByUserLoginID(userLoginID);
        //            if (Org_Mlist == null)
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
        //                Response_MV = new ResponseModelView
        //                {
        //                    Success = true,
        //                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
        //                    Data = Org_Mlist
        //                };
        //                return Response_MV;
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

        #region My Test Region
        //public async Task<ResponseModelView> GetOrgsChildByParentID(RequestHeaderModelView RequestHeader)
        //{
        //    Session_S = new SessionService();
        //    var ResponseSession = await Session_S.CheckAuthorizationResponse(RequestHeader);
        //    if (ResponseSession.Success == false)
        //    {
        //        return ResponseSession;
        //    }
        //    else
        //    {
        //        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
        //        int OrgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
        //        //string getOrgInfo = $"SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName   FROM [User].[Orgs] WHERE OrgId ={OrgOwnerID} ";
        //        string getOrgInfo = $"SELECT TOP 1 OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName FROM [User].[GetOrgsbyUserId]({userLoginID}) ";

        //        dt = new DataTable();
        //        dt = await Task.Run(() => dam.FireDataTable(getOrgInfo));
        //        if (dt == null)
        //        {
        //            Response_MV = new ResponseModelView
        //            {
        //                Success = false,
        //                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError],
        //                Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
        //            };
        //            return Response_MV;
        //        }
        //        OrgModel Org_M = new OrgModel();
        //        List<OrgModel> Org_Mlist = new List<OrgModel>();
        //        if (dt.Rows.Count > 0)
        //        {
        //            Org_M = new OrgModel
        //            {
        //                OrgId = Convert.ToInt32(dt.Rows[0]["OrgId"].ToString()),
        //                OrgUp = Convert.ToInt32(dt.Rows[0]["OrgUp"].ToString()),
        //                OrgLevel = Convert.ToInt32(dt.Rows[0]["OrgLevel"].ToString()),
        //                OrgArName = dt.Rows[0]["OrgArName"].ToString(),
        //                OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
        //                OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
        //                OrgChild = await GlobalService.GetChildByParentID(Convert.ToInt32(dt.Rows[0]["OrgId"].ToString()))
        //            };
        //            Org_Mlist.Add(Org_M);
        //            Response_MV = new ResponseModelView
        //            {
        //                Success = true,
        //                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
        //                Data = Org_Mlist
        //            };
        //            return Response_MV;
        //        }
        //        else
        //        {
        //            Response_MV = new ResponseModelView
        //            {
        //                Success = false,
        //                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoData],
        //                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
        //            };
        //            return Response_MV;
        //        }
        //    }
        //}


        //public async Task<List<OrgModel>> GetChildByParentID(int OrgId)
        //{
        //    string getOrgInfo = $"SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName   FROM [User].[GetOrgsChildsByParentId]({OrgId})";
        //    DataTable dtChild = new DataTable();
        //    dtChild = await Task.Run(() => dam.FireDataTable(getOrgInfo));
        //    OrgModel Org_M1 = new OrgModel();
        //    List<OrgModel> Org_Mlist1 = new List<OrgModel>();
        //    if (dtChild.Rows.Count > 0)
        //    {
        //        for (int i = 0; i < dtChild.Rows.Count; i++)
        //        {
        //            Org_M1 = new OrgModel
        //            {
        //                OrgId = Convert.ToInt32(dtChild.Rows[i]["OrgId"].ToString()),
        //                OrgUp = Convert.ToInt32(dtChild.Rows[i]["OrgUp"].ToString()),
        //                OrgLevel = Convert.ToInt32(dtChild.Rows[i]["OrgLevel"].ToString()),
        //                OrgArName = dtChild.Rows[i]["OrgArName"].ToString(),
        //                OrgEnName = dtChild.Rows[i]["OrgEnName"].ToString(),
        //                OrgKuName = dtChild.Rows[i]["OrgKuName"].ToString(),
        //                OrgChild = await GetChild(Convert.ToInt32(dtChild.Rows[i]["OrgId"].ToString()))
        //            };


        //            Org_Mlist1.Add(Org_M1);
        //        }
        //    }
        //    return Org_Mlist1;
        //}
        #endregion
    }
}