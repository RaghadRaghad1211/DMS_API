using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;
namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Documents
    /// </summary>
    public class DocumentService
    {
        #region Properteis
        private IWebHostEnvironment Environment { get; }
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private DocumentMetadataModelView ViewDocument_MV { get; set; }
        private KeyValueModel KeyValue_M { get; set; }
        private List<KeyValueModel> KeyValue_Mlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        private const int LengthKey = 15;
        #endregion 

        #region Constructor        
        public DocumentService(IWebHostEnvironment environment)
        {
            Environment = environment;
            dam = new DataAccessService(SecurityService.ConnectionString);
            //var ddd = GlobalService.CreateQRcodePNG(100, 300, Environment);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Everyone To Do:
        /// Add Document in Folder
        /// </summary>
        /// <param name="Document_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                                                                         $"ObjId IN (SELECT LcChildObjId FROM [Main].[GetChildsInParent]({Document_MV.DocumentPerantId},{(int)GlobalService.ClassType.Folder})) AND ObjClsId ={Convert.ToInt32(GlobalService.ClassType.Document)} AND ObjIsActive=1 "));
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
                            else
                            {
                                string exeut = "DECLARE   @MyNewValue bigint " +
                                              $"EXEC      [Document].[AddDocumentPro] '{Convert.ToInt32(GlobalService.ClassType.Document)}','{Document_MV.DocumentTitle}', '{userLoginID}', '{Document_MV.DocumentOrgOwnerID}', " +
                                              $"         '{Document_MV.DocumentDescription}', '{Document_MV.KeysValues}', '{Document_MV.DocumentPerantId}', '{(int)GlobalService.ClassType.Folder}', " +
                                              $"          @NewValue = @MyNewValue OUTPUT  SELECT @MyNewValue AS newValue ";

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


                                            string DocumentFileName = SecurityService.RoundomKey(LengthKey) + outValue.Rows[0][0].ToString() + SecurityService.RoundomKey(LengthKey) + Path.GetExtension(DocFileNameWithExten).Trim();
                                            string fillPath = Path.Combine(DocFolder, DocumentFileName);
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
        /// <summary>
        /// Users Have IsWeite & Admins To Do:
        /// Edit Document in Folder
        /// </summary>
        /// <param name="Document_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
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
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, Document_MV.DocumentId).Result;
                    bool checkManagePermission = result == null ? false : result.IsWrite;
                    if (checkManagePermission == true)
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
                            int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [Document].[V_Documents] WHERE ObjTitle = '{Document_MV.DocumentTitle}' AND " +
                                                                             $"ObjId IN (SELECT LcChildObjId FROM [Main].[GetChildsInParent]({Document_MV.DocumentPerantId},{(int)GlobalService.ClassType.Folder})) AND ObjClsId ={Convert.ToInt32(GlobalService.ClassType.Document)} AND ObjIsActive=1 "));
                            if (checkDeblicate == 0)
                            {
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
                                string exeut = "DECLARE   @MyNewValue bigint " +
                                              $"EXEC      [Document].[UpdateDocumentPro] '{Document_MV.DocumentId}','{Convert.ToInt32(GlobalService.ClassType.Document)}','{Document_MV.DocumentTitle}', '{userLoginID}', '{Document_MV.DocumentOrgOwnerID}', " +
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



                                            string DocumentFileName = SecurityService.RoundomKey(LengthKey) + outValue.Rows[0][0].ToString() + SecurityService.RoundomKey(LengthKey) + Path.GetExtension(DocFileNameWithExten).Trim();
                                            string fillPath = Path.Combine(DocFolder, DocumentFileName);
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
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
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
        /// <summary>
        /// Users Have IsRead & Admins To Do:
        /// View Document with metadata and PDF
        /// </summary>
        /// <param name="DocumentId">ID of document that neet to view</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns>Response { (bool)Success, (string)Message, (object)Data}</returns>
        public async Task<ResponseModelView> GetDocumentMetadata(int DocumentId, RequestHeaderModelView RequestHeader)
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
                    var result = GlobalService.CheckUserPermissionsOnFolderAndDocument((SessionModel)ResponseSession.Data, DocumentId).Result;
                    bool checkManagePermission = result == null ? false : result.IsRead;
                    if (checkManagePermission == true)
                    {
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
                                ViewDocument_MV = new DocumentMetadataModelView
                                {
                                    ObjId = DocumentId,
                                    ObjClsId = Convert.ToInt32(GlobalService.ClassType.Document),
                                    KeysValues = KeyValue_Mlist,
                                    DocumentFilePath = await GlobalService.GetFullPathOfDocumentNameInServerFolder(DocumentId, LengthKey, Environment)
                                    #region old code
                                    //string getDocPath = SecurityService.HostFilesUrl + "/" +
                                    //                      (int.Parse(dt.Rows[0]["ObjId"].ToString()) % GlobalService.MoodNum).ToString() + "/" +
                                    //                       dt.Rows[0]["ObjId"].ToString() + "/" +
                                    //                       Path.GetFileName(Directory.GetFiles(
                                    //                                                  Path.Combine(
                                    //                                                       await GlobalService.GetDocumentLocationInServerFolder
                                    //                                                             (Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()), Environment),
                                    //                                                              Convert.ToInt32(dt.Rows[0]["ObjId"].ToString()).ToString())).
                                    //                                                                                                  FirstOrDefault(x => Path.GetFileName(x).
                                    //                                                                                                                 Remove(0, LengthKey).
                                    //                                                                                                                 StartsWith(dt.Rows[0]["ObjId"].ToString())));
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
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.NoPermission],
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
        #endregion
    }
}