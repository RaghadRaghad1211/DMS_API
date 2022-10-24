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
        private DataTable dt { get; set; }
        private UserModel User_M { get; set; }
        private List<UserModel> User_Mlist { get; set; }
        private ResponseModelView response_MV { get; set; }
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
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["UsernameMustEnter"],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return response_MV;
                }
                else if (ValidationService.IsEmpty(User_MV.Password) == true)
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["PasswordMustEnter"],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return response_MV;
                }
                else
                {
                    if (User_MV.Password.Length < 8)
                    {
                        response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()]["Password8Characters"],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return response_MV;
                    }
                    else
                    { // $ystemAdm!n
                        string getUserInfo = "SELECT   UserID, FullName, UsUserName, CONVERT(varchar(MAX), UsPassword) AS UsPassword, Role, UserIsActive, " +
                                            "         UsPhoneNo, UsEmail, UsUserEmpNo, UsUserIdintNo, UsIsOnLine, OrgArName, OrgEnName, OrgKuName, Note " +
                                            "FROM     [User].V_Users " +
                                           $"WHERE    [UsUserName] = '{User_MV.Username}' AND UsPassword = CONVERT(varbinary(max),'{SecurityService.PasswordEnecrypt(User_MV.Password)}')";

                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getUserInfo));
                        if (dt.Rows.Count > 0)
                        {
                            if (bool.Parse(dt.Rows[0]["UserIsActive"].ToString()) == false)
                            {
                                response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()]["UserNotActive"],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotAcceptable).StatusCode
                                };
                                return response_MV;
                            }
                            else
                            {
                                User_M = new UserModel
                                {
                                    UserID = Convert.ToInt32(dt.Rows[0]["UserID"].ToString()),
                                    FullName = dt.Rows[0]["FullName"].ToString(),
                                    UserName = dt.Rows[0]["UsUserName"].ToString(),
                                    Password = dt.Rows[0]["UsPassword"].ToString(),
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
                                var UserToken = SecurityService.GeneratTokenAuthenticate(User_M);
                                response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()]["GetSuccess"],
                                    Data = new { data = User_M, Token = UserToken }
                                };
                                return response_MV;
                            }

                        }
                        else
                        {
                            response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["NoData"],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return response_MV;
                        }
                    }
                }

            }
            catch (Exception)
            {
                response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()]["LoginFaild"],
                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                };
                return response_MV;
            }
        }
        #endregion

    }
}
