using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DMS_API.Services
{
    public class OrgService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private OrgModel Org_M { get; set; }
        private List<OrgModel> Org_Mlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public OrgService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region Actions
        public async Task<ResponseModelView> GetOrgsParentWithChilds(RequestHeaderModelView RequestHeader)
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
                    List<OrgModel> Org_Mlist = new List<OrgModel>();
                    Org_Mlist = await HelpService.GetOrgsParentWithChildsByUserLoginID(userLoginID, ((SessionModel)ResponseSession.Data).IsOrgAdmin);

                    if (Org_Mlist == null)
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
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                            Data = Org_Mlist
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
        public async Task<ResponseModelView> AddOrg(AddOrgModelView AddOrg_MV, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(AddOrg_MV.OrgArName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.TitelArIsEmpty],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ValidationService.IsEmpty(AddOrg_MV.OrgEnName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = AddOrg_MV.OrgEnName + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.TitelEnIsEmpty],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Orgs WHERE OrgArName = '{AddOrg_MV.OrgArName}' AND OrgUp={AddOrg_MV.OrgUp} "));
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        if (checkDeblicate == 0)
                        {
                            string exeut = $"EXEC [User].[AddOrgPro] '{AddOrg_MV.OrgArName}', '{userLoginID}', '{orgOwnerID}', '{AddOrg_MV.Note}', '{AddOrg_MV.OrgUp}',  '{AddOrg_MV.OrgEnName}', '{AddOrg_MV.OrgKuName}' ";
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
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
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
        public async Task<ResponseModelView> EditOrg(EditOrgModelView EditOrg_MV, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(EditOrg_MV.OrgArName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.TitelArIsEmpty],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (ValidationService.IsEmpty(EditOrg_MV.OrgEnName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = EditOrg_MV.OrgEnName + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.TitelEnIsEmpty],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {

                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [User].Orgs WHERE OrgId = {EditOrg_MV.OrgId}  "));
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        if (checkDeblicate > 0)
                        {
                            string exeut = $"EXEC [User].[UpdateOrgPro]  '{EditOrg_MV.OrgId}', '{EditOrg_MV.OrgArName}', '{userLoginID}', '{orgOwnerID}','{EditOrg_MV.IsActive}',  '{EditOrg_MV.Note}', '{EditOrg_MV.OrgUp}', '{EditOrg_MV.OrgEnName}', '{EditOrg_MV.OrgKuName}'";

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
                                Message = EditOrg_MV.OrgArName + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsNotExist],
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