using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;
using System.Text;

namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Session
    /// </summary>
    public class SessionService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private DataTable Dt { get; set; }
        private SessionModel Session_M { get; set; }
        private ResponseModelView Response_MV { get; set; }
        private readonly string GroupOrgAdmins = "GroupOrgAdmins";
        #endregion

        #region Constructor        
        public SessionService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Functions 
        /// <summary>
        /// Check Authorization of user by his token.
        /// </summary>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> CheckAuthorizationResponse(RequestHeaderModelView RequestHeader)
        {
            try
            {
                if (RequestHeader.Token.IsEmpty() == true)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.TokenEmpty],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return await Task.FromResult(Response_MV);
                }
                else
                {
                    Session_M = new SessionModel();
                    Session_M = this.CheckAuthentication(RequestHeader.Token);
                    if (Session_M == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExceptionError],
                            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                        };
                        return await Task.FromResult(Response_MV);
                    }
                    else
                    {
                        if (Session_M.UserID == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.Unauthorized],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return await Task.FromResult(Response_MV);
                        }
                        else
                        {
                            if (Session_M.IsActive == false)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DisactiveToken],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return await Task.FromResult(Response_MV);
                            }
                            else if (Session_M.IsExpairy == true)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExpiredToken],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return await Task.FromResult(Response_MV);
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Data = Session_M
                                };
                                return await Task.FromResult(Response_MV);
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
        private SessionModel CheckAuthentication(string UserToken)
        {
            try
            {
                string get = "SELECT   UserID, IsAdministrator, IsOrgAdmin, IsActive, IsExpairy " +
                                 "FROM     Security.V_Session " +
                                $"WHERE Token='{UserToken}' ";
                Dt = new DataTable();
                Dt = dam.FireDataTable(get);
                if (Dt == null)
                {
                    Session_M = new SessionModel
                    {
                        UserID = 0 // Unauthorized
                    };
                    return Session_M;
                }
                else
                {
                    int inGroupAdmins = int.Parse(dam.FireSQL($"SELECT  COUNT(*)   FROM    [User].[GetMyGroupsbyUserId]({Convert.ToInt32(Dt.Rows[0]["UserID"].ToString())}) WHERE GroupName= '{GroupOrgAdmins}'   "));
                    Session_M = new SessionModel
                    {
                        UserID = Convert.ToInt32(Dt.Rows[0]["UserID"].ToString()),
                        IsAdministrator = Convert.ToBoolean(Dt.Rows[0]["IsAdministrator"].ToString()), // Forbidden
                        IsOrgAdmin = Convert.ToBoolean(Dt.Rows[0]["IsOrgAdmin"].ToString()), // Forbidden
                        IsGroupOrgAdmin = inGroupAdmins > 0 ? true : false, // Forbidden
                        IsExpairy = Convert.ToBoolean(Dt.Rows[0]["IsExpairy"].ToString()), // ExpiredToken
                        IsActive = Convert.ToBoolean(Dt.Rows[0]["IsActive"].ToString()) // DisactiveToken
                    };
                    return Session_M;
                }
            }
            catch (Exception)
            {
                Session_M = new SessionModel();
                return Session_M;
            }
        }
        /// <summary>
        /// Add Session in database by user token when user logined,
        /// return bool variable,
        /// true: add session is Success.
        /// false: add session Faild.
        /// </summary>
        /// <param name="jwtToken">User Token</param>
        /// <returns>bool</returns>
        public async Task<bool> AddSession(JwtToken jwtToken)
        {
            try
            {
                int checkUserID = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM Security.Session WHERE UserID={jwtToken.UserID}"));
                if (checkUserID == 0)
                {
                    //dam.DoQuery($"DELETE FROM Security.Session WHERE UserID={jwtToken.UserID}");
                    string insert = "INSERT INTO Security.Session (Token, UserID, Expairy) OUTPUT INSERTED.UserID " +
                              $"VALUES('{jwtToken.TokenID}',{jwtToken.UserID}, '{jwtToken.TokenExpairy}')";
                    string outValue = dam.DoQueryAndPutOutValue(insert, "UserID");
                    if (outValue == null || outValue.Trim() == "") { return await Task.FromResult(false); }
                    else { return await Task.FromResult(true); }
                }
                else
                {
                    string update = $"UPDATE Security.Session " +
                                    $"SET Token='{jwtToken.TokenID}', Expairy='{jwtToken.TokenExpairy}', IsActive=1 " +
                                    $"WHERE UserID={jwtToken.UserID}";
                    dam.DoQuery(update);
                    return await Task.FromResult(true);
                }
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }
        #endregion
    }
}