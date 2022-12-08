using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace DMS_API.Services
{
    public class DocumentService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private DocumentModel Document_M { get; set; }
        private List<DocumentModel> Document_Mlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        private const int ClassID = 5; // Document
        #endregion

        #region Constructor        
        public DocumentService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region CURD Functions
        public async Task<ResponseModelView> GetDocumentsList(PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                    string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                    var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                             $"FROM [Document].V_Documents  WHERE [OrgOwner] IN ({whereField} FROM [User].[GetOrgsbyUserId]({userLoginID})) AND ObjClsId ={ClassID} ");
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
                            string getDocumentInfo = "SELECT    ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, ObjCreationDate, ObjDescription, UserOwnerID, " +
                                                 "            OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                                 "FROM        [Document].V_Documents " +
                                                $"WHERE       [OrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} " +
                                                 "ORDER BY    ObjId " +
                                                $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                $"FETCH NEXT   {_PageRows} ROWS ONLY ";

                            dt = new DataTable();
                            dt = await Task.Run(() => dam.FireDataTable(getDocumentInfo));
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
                            Document_Mlist = new List<DocumentModel>();
                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    Document_M = new DocumentModel
                                    {
                                        ObjId = Convert.ToInt32(dt.Rows[i]["ObjId"].ToString()),
                                        ObjTitle = dt.Rows[i]["ObjTitle"].ToString(),
                                        ObjClsId = Convert.ToInt32(dt.Rows[i]["ObjClsId"].ToString()),
                                        ClsName = dt.Rows[i]["ClsName"].ToString(),
                                        ObjIsActive = bool.Parse(dt.Rows[i]["ObjIsActive"].ToString()),
                                        ObjCreationDate = DateTime.Parse(dt.Rows[i]["ObjCreationDate"].ToString()),
                                        ObjDescription = dt.Rows[i]["ObjDescription"].ToString(),
                                        UserOwnerID = Convert.ToInt32(dt.Rows[i]["UserOwnerID"].ToString()),
                                        OwnerFullName = dt.Rows[i]["OwnerFullName"].ToString(),
                                        OwnerUserName = dt.Rows[i]["OwnerUserName"].ToString(),
                                        OrgOwner = dt.Rows[i]["OrgOwner"].ToString(),
                                        OrgEnName = dt.Rows[i]["OrgEnName"].ToString(),
                                        OrgArName = dt.Rows[i]["OrgArName"].ToString(),
                                        OrgKuName = dt.Rows[i]["OrgKuName"].ToString(),
                                    };
                                    Document_Mlist.Add(Document_M);
                                }

                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Document_Mlist }
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
        public async Task<ResponseModelView> GetDocumentsByID(int id, RequestHeaderModelView RequestHeader)
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
                    int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                    //string whereField = orgOwnerID == 0 ? "OrgUp" : "OrgId";
                    string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                    string getDocumentInfo = "SELECT  ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, ObjCreationDate, ObjDescription, UserOwnerID, " +
                                           "          OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                           "FROM      [Document].V_Documents " +
                                          $"WHERE     ObjId={id} AND [OrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} ";


                    dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getDocumentInfo));
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
                    Document_Mlist = new List<DocumentModel>();
                    if (dt.Rows.Count > 0)
                    {
                        Document_M = new DocumentModel
                        {
                            ObjId = Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()),
                            ObjTitle = dt.Rows[0]["ObjTitle"].ToString(),
                            ObjClsId = Convert.ToInt32(dt.Rows[0]["ObjClsId"].ToString()),
                            ClsName = dt.Rows[0]["ClsName"].ToString(),
                            ObjIsActive = bool.Parse(dt.Rows[0]["ObjIsActive"].ToString()),
                            ObjCreationDate = DateTime.Parse(dt.Rows[0]["ObjCreationDate"].ToString()),
                            ObjDescription = dt.Rows[0]["ObjDescription"].ToString(),
                            UserOwnerID = Convert.ToInt32(dt.Rows[0]["UserOwnerID"].ToString()),
                            OwnerFullName = dt.Rows[0]["OwnerFullName"].ToString(),
                            OwnerUserName = dt.Rows[0]["OwnerUserName"].ToString(),
                            OrgOwner = dt.Rows[0]["OrgOwner"].ToString(),
                            OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                            OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                            OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                        };
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                            Data = Document_M
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
        public async Task<ResponseModelView> AddDocument(DocumentModelView Document_MV, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(Document_MV.DocumentTitle) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FolderNameMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        //int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        //string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                        //int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [Main].[Objects] WHERE ObjTitle = '{Document_MV.DocumentTitle}' AND " +
                        //                                                 $"[ObjOrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} "));
                        //if (checkDeblicate == 0)
                        //{
                        if (Document_MV.KeysValues.Count == 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectedObjects],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                        string Query = HelpService.GetQueryAddDocument(Document_MV);
                        string exeut = $"EXEC [Document].[AddDocumentPro] '{ClassID}','{Document_MV.DocumentTitle}', '{userLoginID}', '{Document_MV.DocumentOrgOwnerID}', '{Document_MV.DocumentDescription}', '{Query}', '{Document_MV.DocumentPerantId}', '{(int)HelpService.ClassType.Folder}' ";

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
                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.InsertSuccess],
                            Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                        };
                        return Response_MV;
                        //}
                        //else
                        //{
                        //    Response_MV = new ResponseModelView
                        //    {
                        //        Success = false,
                        //        Message = Document_MV.DocumentTitle + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
                        //        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        //    };
                        //    return Response_MV;
                        //}
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

        public async Task<ResponseModelView> SearchDocumentByName(string Name, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(Name) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.FolderNameMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                        string getDocumentInfo = "SELECT  ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, ObjCreationDate, ObjDescription, UserOwnerID, " +
                                                 "        OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                                 "FROM    [Document].V_Documents " +
                                                $"WHERE   ObjTitle LIKE '{Name}%' AND [OrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjClsId ={ClassID} ";


                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getDocumentInfo));
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
                        Document_Mlist = new List<DocumentModel>();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                Document_M = new DocumentModel
                                {
                                    ObjId = Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()),
                                    ObjTitle = dt.Rows[0]["ObjTitle"].ToString(),
                                    ObjClsId = Convert.ToInt32(dt.Rows[0]["ObjClsId"].ToString()),
                                    ClsName = dt.Rows[0]["ClsName"].ToString(),
                                    ObjIsActive = bool.Parse(dt.Rows[0]["ObjIsActive"].ToString()),
                                    ObjCreationDate = DateTime.Parse(dt.Rows[0]["ObjCreationDate"].ToString()),
                                    ObjDescription = dt.Rows[0]["ObjDescription"].ToString(),
                                    UserOwnerID = Convert.ToInt32(dt.Rows[0]["UserOwnerID"].ToString()),
                                    OwnerFullName = dt.Rows[0]["OwnerFullName"].ToString(),
                                    OwnerUserName = dt.Rows[0]["OwnerUserName"].ToString(),
                                    OrgOwner = dt.Rows[0]["OrgOwner"].ToString(),
                                    OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                                    OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                                    OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                                };
                                Document_Mlist.Add(Document_M);
                            }

                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = Document_Mlist
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



        #endregion
    }
}





// get permession when user click on write or manage or QR...
// SELECT SourObjId, SourName, SourClsId, SourClsType, DestObjId, DestName, DestClsId, DestClsType, PerRead, PerWrite, PerManage, PerQR
// FROM [Document].[GetPermissionsOnObject](UserId,@ObjectId )
// WHERE SourObjId =ObjClicked AND PerManage =1



// to open folder get chilid where user have permession...
// SELECT DISTINCT OpenObject.SourObjId, OpenObject.SourName, OpenObject.SourClsId, OpenObject.SourClsType FROM (SELECT * FROM [Document].[GetPermissionsOnObject](41,6 )) OpenObject
