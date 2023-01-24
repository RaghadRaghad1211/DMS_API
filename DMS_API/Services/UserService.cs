using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using Microsoft.AspNetCore.Http.Headers;
using System.Data;
using System.Net;
namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Users
    /// </summary>
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
        /// <summary>
        /// Everyone To Do:
        /// Login into Documents Management System (DMS)
        /// </summary>
        /// <param name="User_MV">Body Parameters</param>
        /// <param name="Lang">Header Language</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> Login(LoginModelView User_MV, string Lang)
        {
            try
            {
                bool DatabaseConnection = dam.CheckConnectionNetwork();
                if (DatabaseConnection == false)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ServiceUnavailable],
                        Data = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable).StatusCode
                    };
                    return Response_MV;
                }
                else
                {
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
                            int checkUsername = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsUserName = '{User_MV.Username}' "));
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
                                                                               $"WHERE   UsUserName = '{User_MV.Username}' AND " +
                                                                               $"        UsPassword = '{SecurityService.PasswordEnecrypt(User_MV.Password.Trim(), User_MV.Username.Trim())}' "));
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
                                    string getUserInfo = "SELECT  UserID,UsFirstName,UsSecondName,UsThirdName,UsLastName, FullName, UsUserName,  Role, IsOrgAdmin, UserIsActive, UsPhoneNo, " +
                                                        "         UsEmail, UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgOwner, OrgArName, OrgEnName, OrgKuName, Note, UserCreationDate " +
                                                        "FROM     [User].V_Users " +
                                                       $"WHERE    [UsUserName] = '{User_MV.Username.Trim()}' AND UsPassword = '{SecurityService.PasswordEnecrypt(User_MV.Password.Trim(), User_MV.Username.Trim())}' ";

                                    dt = new DataTable();
                                    await Task.Delay(1000);
                                    dt = dam.FireDataTable(getUserInfo);
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
                                                UserCreationDate = DateTime.Parse(dt.Rows[0]["UserCreationDate"].ToString()).ToShortDateString()
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
                                                dam.DoQuery($"UPDATE [User].Users SET UsIsOnLine=1 WHERE UserID={Convert.ToInt32(dt.Rows[0]["UserID"].ToString())}");
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
        /// Only Admins To Do:
        /// Reset Password for Users
        /// </summary>
        /// <param name="userId">Id of user that reset his password</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> ResetPasswordByAdmin(int UserId, RequestHeaderModelView RequestHeader)
        {
            try
            {
                if (UserId.ToString().IsInt() == false || UserId == 0)
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
                            DataTable dtEmail = new DataTable();
                            dtEmail = dam.FireDataTable($"SELECT UsUserName, UsPhoneNo, UsEmail FROM [User].Users WHERE UsId ={UserId} ");
                            if (dtEmail.Rows.Count > 0)
                            {
                                string RounPass = SecurityService.RoundomPassword(10);
                                bool isResetEmail = SecurityService.SendEmail(dtEmail.Rows[0]["UsEmail"].ToString(),
                                     MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EmailSubjectPasswordIsReset],
                                     MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EmailBodyPasswordIsReset] + RounPass);
                                bool isResetOTP = await SecurityService.SendOTP(dtEmail.Rows[0]["UsPhoneNo"].ToString(), MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.PhonePasswordIsReset] + RounPass);
                                if (isResetEmail == true && isResetOTP == true)
                                {
                                    string reset = $"UPDATE [User].Users SET UsPassword='{SecurityService.PasswordEnecrypt(RounPass, dtEmail.Rows[0]["UsUserName"].ToString())}' WHERE UsId ={UserId} ";
                                    dam.DoQuery(reset);
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
        /// Reset Password for User
        /// </summary>
        /// <param name="Username">Username of user that reset his password</param>
        /// <param name="Lang">Header Language</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> ResetPasswordByUser(string Username, string Lang)
        {
            try
            {
                if (Username.IsEmpty() == true)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.UsernameMustEnter],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
                }
                else
                {
                    DataTable dtEmail = new DataTable();
                    dtEmail = dam.FireDataTable($"SELECT UsPhoneNo, UsEmail FROM [User].Users WHERE UsUserName ='{Username}' ");
                    if (dtEmail.Rows.Count > 0)
                    {
                        string RounPass = SecurityService.RoundomPassword(10);
                        bool isResetEmail = SecurityService.SendEmail(dtEmail.Rows[0]["UsEmail"].ToString(),
                             MessageService.MsgDictionary[Lang.ToLower()][MessageService.EmailSubjectPasswordIsReset],
                             MessageService.MsgDictionary[Lang.ToLower()][MessageService.EmailBodyPasswordIsReset] + RounPass);
                        bool isResetOTP = await SecurityService.SendOTP(dtEmail.Rows[0]["UsPhoneNo"].ToString(), MessageService.MsgDictionary[Lang.ToLower()][MessageService.PhonePasswordIsReset] + RounPass);
                        if (isResetEmail == true && isResetOTP == true)
                        {
                            string reset = $"UPDATE [User].Users SET UsPassword='{SecurityService.PasswordEnecrypt(RounPass, Username)}' WHERE UsUserName ='{Username}' ";
                            dam.DoQuery(reset);
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
        /// Everyone To Do:
        /// Change Password for user
        /// </summary>
        /// <param name="ChangePassword_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                            dam.DoQuery(change);
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
        /// <summary>
        /// Only Admins To Do:
        /// Get all Users which depends on the Admin Orgnazation
        /// </summary>
        /// <param name="Pagination_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                        int CurrentPage = _PageNumber; int PageRows = _PageRows;

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
                                                    $"WHERE       [USERID] !={userLoginID} " +
                                                     "ORDER BY    UserID " +
                                                    $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                    $"FETCH NEXT   {_PageRows} ROWS ONLY ";

                                dt = new DataTable();
                                dt = dam.FireDataTable(getUserInfo);
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
        /// Get User info by user Id
        /// </summary>
        /// <param name="UserId">Id of user that get his info</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetUsersByID(int UserId, RequestHeaderModelView RequestHeader)
        {
            try
            {
                if (UserId.ToString().IsInt() == false || UserId == 0)
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
                            string getUserInfo = "SELECT   UserID, UsFirstName, UsSecondName, UsThirdName, UsLastName, FullName, UsUserName, Role, IsOrgAdmin, UserIsActive, UsPhoneNo, UsEmail, " +
                                                 "         UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgOwner, OrgArName, OrgEnName, OrgKuName, Note " +
                                                $"FROM     [User].V_Users WHERE UserID={UserId} ";

                            dt = new DataTable();
                            dt = dam.FireDataTable(getUserInfo);
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
        /// Add Users
        /// </summary>
        /// <param name="AddUser_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                        else if (AddUser_MV.OrgOwner == 0)
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
                                    int checkDeblicateEmail = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Users WHERE UsEmail = '{AddUser_MV.Email}' "));
                                    if (checkDeblicateEmail == 0)
                                    {
                                        string exeut = $"EXEC [User].[AddUserPro] '{AddUser_MV.UserName.Trim()}', '{userLoginID}', '{AddUser_MV.OrgOwner}', '{AddUser_MV.Note}', '{AddUser_MV.FirstName.Trim()}',  '{AddUser_MV.SecondName.Trim()}', '{AddUser_MV.ThirdName.Trim()}', '{AddUser_MV.LastName.Trim()}', '{SecurityService.PasswordEnecrypt(AddUser_MV.Password.Trim(), AddUser_MV.UserName.Trim())}', '{AddUser_MV.PhoneNo.Trim()}', '{AddUser_MV.Email.ToLower().Trim()}', '{AddUser_MV.IsActive}', '{AddUser_MV.UserEmpNo}', '{AddUser_MV.UserIdintNo}', '{AddUser_MV.IsOrgAdmin}' ";
                                        var outValue = dam.DoQueryExecProcedure(exeut);
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
                                            Message = AddUser_MV.UserName + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.EmailIsExist],
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
        /// Edit Users
        /// </summary>
        /// <param name="EditUser_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                                var outValue = dam.DoQueryExecProcedure(exeut);

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
        /// Search User by username
        /// </summary>
        /// <param name="Username">Username of user that want search about it</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> SearchUsersByUserName(string Username, RequestHeaderModelView RequestHeader)
        {
            try
            {
                if (Username.IsEmpty() == true)
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
                                             "         UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgArName, OrgEnName, OrgKuName, Note " +
                                            $"FROM     [User].V_Users WHERE UsUserName LIKE '{Username}%' AND [OrgOwner] IN (SELECT [OrgId] FROM [User].[GetOrgsbyUserId]({userLoginID})) AND [USERID] !={userLoginID} ";

                        dt = new DataTable();
                        dt = dam.FireDataTable(getUserInfo);
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
        /// <summary>
        /// Only Admins To Do:
        /// Advance Search User 
        /// </summary>
        /// <param name="SearchUser_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                        int _PageNumber = SearchUser_MV.PageNumber == 0 ? 1 : SearchUser_MV.PageNumber;
                        int _PageRows = SearchUser_MV.PageRows == 0 ? 1 : SearchUser_MV.PageRows;
                        int CurrentPage = _PageNumber; int PageRows = _PageRows;
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
                                dt = dam.FireDataTable(getUserInfo);
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
        /// Update Contact (Email & Phone)
        /// </summary>
        /// <param name="EditMyContact_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                                dam.DoQuery(edit);
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
        /// <summary>
        /// Only Admins To Do:
        /// Get User's Groups .
        /// </summary>
        /// <param name="UserId">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetGroupsOfUser(int UserId, RequestHeaderModelView RequestHeader)
        {
            if (UserId.ToString().IsInt() == false || UserId == 0)
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
                        DataTable dtGetGroup = new DataTable();
                        dtGetGroup = dam.FireDataTable("SELECT      GroupId, GroupName   " +
                                                                           $"FROM        [User].[GetMyGroupsbyUserId]({UserId}) " +
                                                                            $"ORDER BY   GroupId ");
                        MyGroup MyGroup = new MyGroup();
                        List<MyGroup> MyGroup_List = new List<MyGroup>();
                        if (dtGetGroup.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtGetGroup.Rows.Count; i++)
                            {
                                MyGroup = new MyGroup
                                {
                                    GroupId = Convert.ToInt32(dtGetGroup.Rows[i]["GroupId"].ToString()),
                                    GroupName = dtGetGroup.Rows[i]["GroupName"].ToString()
                                };
                                MyGroup_List.Add(MyGroup);
                            }
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = MyGroup_List
                            };
                            return Response_MV;
                        }
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoGroup],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
                    }
                }
            }
        }
        #endregion
    }
}