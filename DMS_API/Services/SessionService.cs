using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace DMS_API.Services
{
    public class SessionService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private DataTable Dt { get; set; }
        private SessionModel Session_M { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public SessionService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region CURD Functions 
        public async Task<ResponseModelView> CheckAuthorizationResponse(string Token, string Lang)
        {
            try
            {
                if (ValidationService.IsEmpty(Token) == true)
                {
                    Response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.TokenEmpty],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return Response_MV;
                }
                else
                {
                    Session_M = new SessionModel();
                    Session_M = await Task.Run(() => this.CheckAuthentication(Token));
                    if (Session_M == null)
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
                        if (Session_M.UserID == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.Unauthorized],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            if (Session_M.IsExpairy == true)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExpiredToken],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = true
                                };
                                return Response_MV;
                            }
                            //else if (Session_M.IsAdministrator == false)
                            //{
                            //    Response_MV = new ResponseModelView
                            //    {
                            //        Success = false,
                            //        Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.Forbidden],
                            //        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
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
        private async Task<SessionModel> CheckAuthentication(string UserToken)
        {
            try
            {
                string get = "SELECT   UserID, IsAdministrator, IsExpairy " +
                                 "FROM     Security.V_Session " +
                                $"WHERE Token='{UserToken}' ";
                Dt = new DataTable();
                Dt = await Task.Run(() => dam.FireDataTable(get));
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
                    Session_M = new SessionModel
                    {
                        UserID = Convert.ToInt32(Dt.Rows[0]["UserID"].ToString()),
                        IsAdministrator = Convert.ToBoolean(Dt.Rows[0]["IsAdministrator"].ToString()), // Forbidden
                        IsExpairy = Convert.ToBoolean(Dt.Rows[0]["IsExpairy"].ToString()) // ExpiredToken
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
                    string outValue = await Task.Run(() => dam.DoQueryAndPutOutValue(insert, "UserID"));
                    if (outValue == null || outValue.Trim() == "") { return false; }
                    else { return true; }
                }
                else
                {
                    string update = $"UPDATE Security.Session " +
                                    $"SET Token='{jwtToken.TokenID}', Expairy='{jwtToken.TokenExpairy}' " +
                                    $"WHERE UserID={jwtToken.UserID}";
                    await Task.Run(() => dam.DoQuery(update));
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}