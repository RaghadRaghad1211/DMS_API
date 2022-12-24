using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.WebRequestMethods;

namespace DMS_API.Services
{
    public class DocumentService
    {
        #region Properteis
        private IWebHostEnvironment Environment { get; }
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private DocumentModel Document_M { get; set; }
        private List<DocumentModel> Document_Mlist { get; set; }
        private DocumentMetadataModelView ViewDocument_MV { get; set; }
        private KeyValueModel KeyValue_M { get; set; }
        private List<KeyValueModel> KeyValue_Mlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        private const int LengthKey = 15;
        private const int ClassID = 5; // Document
        #endregion

        #region Constructor        
        public DocumentService(IWebHostEnvironment environment)
        {
            Environment = environment;
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
                    //int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                    //string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";
                    string getDocumentInfo = "SELECT  ObjId, ObjTitle, ObjClsId, ClsName, ObjIsActive, ObjCreationDate, ObjDescription, UserOwnerID, " +
                                           "          OwnerFullName, OwnerUserName, OrgOwner, OrgEnName,OrgArName , OrgKuName " +
                                           "FROM      [Document].V_Documents " +
                                          $"WHERE     ObjId={id}  ";


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
                    if (ValidationService.IsEmpty(Document_MV.DocumentTitle) == true || string.IsNullOrEmpty(Document_MV.DocumentTitle))
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DocumentTitelMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (Document_MV.DocumentFile.Length <= 0)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DocumentFileMustUpload],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [Document].[V_Documents] WHERE ObjTitle = '{Document_MV.DocumentTitle}' AND " +
                                                                         $"ObjId IN (SELECT LcChildObjId FROM [Main].[GetChildsInParent]({Document_MV.DocumentPerantId},{(int)GlobalService.ClassType.Folder})) AND ObjClsId ={ClassID} AND ObjIsActive=1 "));
                        if (checkDeblicate == 0)
                        {
                            if (ValidationService.IsEmpty(Document_MV.KeysValues) == true)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustFillInformation],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }

                            //string Query = GlobalService.GetQueryAddDocument(Document_MV);

                            string exeut = "DECLARE   @MyNewValue bigint " +
                                          $"EXEC      [Document].[AddDocumentPro] '{ClassID}','{Document_MV.DocumentTitle}', '{userLoginID}', '{Document_MV.DocumentOrgOwnerID}', " +
                                          $"         '{Document_MV.DocumentDescription}', '{Document_MV.KeysValues}', '{Document_MV.DocumentPerantId}', '{(int)GlobalService.ClassType.Folder}', " +
                                          $"         @NewValue = @MyNewValue OUTPUT  SELECT @MyNewValue AS newValue ";

                            var outValue = await Task.Run(() => dam.FireDataTable(exeut));
                            if (outValue.Rows[0][0].ToString() == 0.ToString() || outValue.Rows[0][0].ToString() == null || outValue.Rows[0][0].ToString().Trim() == "")
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
                                if (Document_MV.DocumentFile.Length > 0)
                                {
                                    string DocFileNameWithExten = Document_MV.DocumentFile.FileName;
                                    if (Path.GetExtension(DocFileNameWithExten.ToLower()).Trim() != ".pdf")
                                    {
                                        Response_MV = new ResponseModelView
                                        {
                                            Success = false,
                                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExtensionMustBePFD],
                                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                        };
                                        return Response_MV;
                                    }
                                    var DocFolder = await GlobalService.CreateDocumentFolderInServerFolder(Convert.ToInt32(outValue.Rows[0][0].ToString()), Environment);
                                    if (DocFolder != null)
                                    {
                                        // تشفير




                                        string fillPath = Path.Combine(DocFolder, SecurityService.RoundomKey(LengthKey) + outValue.Rows[0][0].ToString() + SecurityService.RoundomKey(LengthKey) + Path.GetExtension(DocFileNameWithExten).Trim());
                                        //string fillPath = Path.Combine(DocFolder, outValue + ".pdf");
                                        using (FileStream filestream = System.IO.File.Create(fillPath))
                                        {
                                            Document_MV.DocumentFile.CopyTo(filestream);
                                            filestream.Flush();
                                            filestream.Close();
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
                        else
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = Document_MV.DocumentTitle + " " + MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsExist],
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
        public async Task<ResponseModelView> EditDocument(DocumentModelView Document_MV, RequestHeaderModelView RequestHeader)
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
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DocumentTitelMustEnter],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (Document_MV.DocumentFile.Length <= 0)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DocumentFileMustUpload],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        if (ValidationService.IsEmpty(Document_MV.KeysValues) == true)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.MustSelectedObjects],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }

                        //string Query = GlobalService.GetQueryAddDocument(Document_MV);

                        string exeut = "DECLARE   @MyNewValue bigint " +
                                      $"EXEC      [Document].[UpdateDocumentPro] '{Document_MV.DocumentId}','{ClassID}','{Document_MV.DocumentTitle}', '{userLoginID}', '{Document_MV.DocumentOrgOwnerID}', " +
                                      $"         '{Document_MV.DocumentDescription}', '{Document_MV.KeysValues}', '{Document_MV.DocumentPerantId}', '{(int)GlobalService.ClassType.Folder}', " +
                                      $"         @NewValue = @MyNewValue OUTPUT  SELECT @MyNewValue AS newValue ";


                        var outValue = await Task.Run(() => dam.FireDataTable(exeut));
                        if (outValue.Rows[0][0].ToString() == 0.ToString() || outValue.Rows[0][0].ToString() == null || outValue.Rows[0][0].ToString().Trim() == "")
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
                            if (Document_MV.DocumentFile.Length > 0)
                            {
                                string DocFileNameWithExten = Document_MV.DocumentFile.FileName;
                                if (Path.GetExtension(DocFileNameWithExten.ToLower()).Trim() != ".pdf")
                                {
                                    Response_MV = new ResponseModelView
                                    {
                                        Success = false,
                                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.ExtensionMustBePFD],
                                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                    };
                                    return Response_MV;
                                }
                                var DocFolder = await GlobalService.CreateDocumentFolderInServerFolder(Convert.ToInt32(outValue.Rows[0][0].ToString()), Environment);
                                if (DocFolder != null)
                                {
                                    // تشفير




                                    string fillPath = Path.Combine(DocFolder, SecurityService.RoundomKey(LengthKey) + outValue.Rows[0][0].ToString() + SecurityService.RoundomKey(LengthKey) + Path.GetExtension(DocFileNameWithExten).Trim());
                                    //string fillPath = Path.Combine(DocFolder, outValue + ".pdf");
                                    using (FileStream filestream = System.IO.File.Create(fillPath))
                                    {
                                        Document_MV.DocumentFile.CopyTo(filestream);
                                        filestream.Flush();
                                        filestream.Close();
                                    }
                                }
                            }
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
        public async Task<ResponseModelView> ViewDocumentMetadata(int DocumentId, RequestHeaderModelView RequestHeader)
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
                    int CheckActivation = int.Parse(dam.FireSQL($"SELECT COUNT(*) FROM [Document].[V_Documents] WHERE ObjId={DocumentId} AND ObjIsActive=1"));
                    if (CheckActivation == 0)
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
                        string getDocumentmatedateInfo = "SELECT    ObjId, ObjClsId, CDToolBoxId AS 'ToolId', TbToolName AS 'ToolType', CDID AS 'Key', KVValue AS 'Value', TbMultiSelect " +
                                                         "FROM      [Document].V_DocumentsMetadata " +
                                                        $"WHERE     ObjId={DocumentId}  ";
                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getDocumentmatedateInfo));
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

                        KeyValue_Mlist = new List<KeyValueModel>();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                KeyValue_M = new KeyValueModel
                                {
                                    Key = Convert.ToInt32(dt.Rows[i]["Key"].ToString()),
                                    Value = dt.Rows[i]["Value"].ToString(),
                                    ToolId = Convert.ToInt32(dt.Rows[i]["ToolId"].ToString()),
                                    ToolType = dt.Rows[i]["ToolType"].ToString(),
                                };
                                KeyValue_Mlist.Add(KeyValue_M);
                            }

                            string getDocPath = SecurityService.HostFilesUrl + "/" +
                                                  (int.Parse(dt.Rows[0]["ObjId"].ToString()) % GlobalService.MoodNum).ToString() + "/" +
                                                   dt.Rows[0]["ObjId"].ToString() + "/" +
                                                   Path.GetFileName(Directory.GetFiles(
                                                                              Path.Combine(
                                                                                   await GlobalService.GetDocumentLocationInServerFolder
                                                                                         (Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()), Environment),
                                                                                          Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()).ToString())).
                                                                                                                              FirstOrDefault(x => Path.GetFileName(x).
                                                                                                                                             Remove(0, LengthKey).
                                                                                                                                             StartsWith(dt.Rows[0]["ObjId"].ToString())));
                            //StartsWith(dt.Rows[0]["ObjId"].ToString())));
                            ViewDocument_MV = new DocumentMetadataModelView
                            {
                                ObjId = Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()),
                                ObjClsId = Convert.ToInt32(dt.Rows[0]["ObjClsId"].ToString()),
                                KeysValues = KeyValue_Mlist,
                                DocumentFilePath = getDocPath
                                #region old code
                                //DocumentFilePath = SecurityService.HostFilesUrl + "/" +
                                //                  (int.Parse(dt.Rows[0]["ObjId"].ToString()) % GlobalService.MoodNum).ToString() + "/" +
                                //                   dt.Rows[0]["ObjId"].ToString() + "/" +
                                //                   dt.Rows[0]["ObjId"].ToString() + ".pdf"
                                #endregion
                            };

                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = ViewDocument_MV
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