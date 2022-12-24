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
        private List<OrgTableModel> OrgTable_Mlist { get; set; }
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
                    Org_Mlist = await GlobalService.GetOrgsParentWithChildsByUserLoginID(userLoginID, ((SessionModel)ResponseSession.Data).IsOrgAdmin);

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
        public async Task<ResponseModelView> GetOrgsParentWithChilds_Table(PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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
                    List<OrgTableModel> OrgTable_Mlist = new List<OrgTableModel>();
                    OrgTable_Mlist = await GlobalService.GetOrgsParentWithChildsByUserLoginID_Table(userLoginID);

                    if (OrgTable_Mlist == null)
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
                            Data = new
                            {
                                TotalRows = OrgTable_Mlist.Count,
                                MaxPage = Math.Ceiling(OrgTable_Mlist.Count / (float)_PageRows),
                                CurrentPage = _PageNumber,
                                PageRows = _PageRows,
                                data = new
                                {
                                    Parent = OrgTable_Mlist[0],
                                    Child = OrgTable_Mlist.Where(x => x.OrgId != OrgTable_Mlist[0].OrgId)
                                                      .Skip((_PageNumber - 1) * _PageRows).Take(_PageRows)
                                }
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
        public async Task<ResponseModelView> GetOrgByID(int OrgID, RequestHeaderModelView RequestHeader)
        {
            try
            {
                string getOrgInfo = "SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName, " +
                                    "       OrgArNameUp, OrgEnNameUp, OrgKuNameUp, OrgIsActive, ObjDescription  " +
                                   $"FROM  [User].[V_OrgTable]  WHERE OrgId= {OrgID} ";
                DataTable dt = new DataTable();
                dt = await Task.Run(() => dam.FireDataTable(getOrgInfo));
                if (dt == null)
                {
                    return null;
                }
                OrgTableModel OrgTable_M = new OrgTableModel();
                if (dt.Rows.Count > 0)
                {
                    OrgTable_M = new OrgTableModel
                    {
                        OrgId = Convert.ToInt32(dt.Rows[0]["OrgId"].ToString()),
                        OrgUp = Convert.ToInt32(dt.Rows[0]["OrgUp"].ToString()),
                        OrgLevel = Convert.ToInt32(dt.Rows[0]["OrgLevel"].ToString()),
                        OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                        OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                        OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                        OrgArNameUp = dt.Rows[0]["OrgArNameUp"].ToString(),
                        OrgEnNameUp = dt.Rows[0]["OrgEnNameUp"].ToString(),
                        OrgKuNameUp = dt.Rows[0]["OrgKuNameUp"].ToString(),
                        OrgIsActive = bool.Parse(dt.Rows[0]["OrgIsActive"].ToString()),
                        Note = dt.Rows[0]["ObjDescription"].ToString()
                    };
                    Response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                        Data = OrgTable_M
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
        public async Task<ResponseModelView> SearchOrgsByArName(string ArName, RequestHeaderModelView RequestHeader)
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
                    List<OrgTableModel> OrgTable_Mlist = new List<OrgTableModel>();
                    OrgTable_Mlist = await GlobalService.GetOrgsParentWithChildsByUserLoginID_Table(userLoginID);

                    if (OrgTable_Mlist == null)
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
                            Data = new { Org = OrgTable_Mlist.Where(x => x.OrgArName.Contains(ArName)) }
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
                                string OrgAdminUsername = "Admin@" + SecurityService.RoundomPassword(6);
                                string exeut = $"EXEC [User].[AddOrgPro] '{AddOrg_MV.OrgArName}', '{userLoginID}', '{orgOwnerID}', '{AddOrg_MV.Note}', '{AddOrg_MV.OrgUp}', '{AddOrg_MV.OrgEnName}', '{AddOrg_MV.OrgKuName}', '{OrgAdminUsername}', '{SecurityService.PasswordEnecrypt("00000000", OrgAdminUsername)}' ";
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