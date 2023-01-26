using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using Microsoft.AspNetCore.Http.Headers;
using Org.BouncyCastle.Crypto.Tls;
using QRCoder;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
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
        #endregion 

        #region Constructor        
        public DocumentService(IWebHostEnvironment environment)
        {
            Environment = environment;
            dam = new DataAccessService(SecurityService.ConnectionString);



           //  var keyring = SecurityService.EncryptDocument("F:\\IIS\\DMS\\dm.pdf", "F:\\IIS\\DMS\\");
              // string keyring = @"{|\|DC-0F-!R@Q}$dwSocTfGR01MPaFnvWUpLw==$fHaKKkPFIFmqdjtFkSZ32LxdMzzRDLhC9l3ddoxfEMg=";

              //   var ee = SecurityService.DecryptDocument("F:\\IIS\\DMS\\dm.pdf.enc", keyring, "55");


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
                    if (Document_MV.DocumentTitle.IsEmpty() == true)
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
                    else if (!Document_MV.DocumentFile.Length.FileSizeIsValid())
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.InvalidFileSize],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else if (Document_MV.DocumentPerantId.ToString().IsInt() == false || Document_MV.DocumentPerantId <= 0 ||
                             Document_MV.DocumentOrgOwnerID.ToString().IsInt() == false || Document_MV.DocumentOrgOwnerID <= 0)
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

                                var outValue = dam.FireDataTable(exeut);
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
                                    int DocId = (int)Convert.ToInt64(outValue.Rows[0][0].ToString());
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
                                        var DocFolder = await GlobalService.CreateDocumentFolderInServerFolder(DocId, Environment);
                                        if (DocFolder != null)
                                        {
                                            string DocumentFileName = SecurityService.RoundomKey(GlobalService.LengthKey) + SecurityService.EnecryptText(DocId.ToString()) + SecurityService.RoundomKey(GlobalService.LengthKey) + Path.GetExtension(DocFileNameWithExten).Trim();
                                            string FilePath = Path.Combine(DocFolder, DocumentFileName);
                                            using (FileStream filestream = System.IO.File.Create(FilePath))
                                            {
                                                Document_MV.DocumentFile.CopyTo(filestream);
                                                filestream.Flush();
                                                filestream.Close();
                                            }

                                            // تشفير
                                            string DocKey = SecurityService.EncryptDocument(FilePath, Path.GetDirectoryName(FilePath));
                                            if (DocKey.IsEmpty() == true)
                                            {
                                                Response_MV = new ResponseModelView
                                                {
                                                    Success = false,
                                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DocKeyFaild],
                                                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                                                };
                                                return Response_MV;
                                            }
                                            else
                                            {
                                                string insert = $"insert into  [Document].[MasterKeys] VALUES({DocId},'{DocKey}',{1})";
                                                dam.DoQuery(insert);
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
                        bool haveFile = true;
                        if (Document_MV.DocumentFile == null) haveFile = false;

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
                        else if (Document_MV.DocumentPerantId.ToString().IsInt() == false || Document_MV.DocumentPerantId <= 0 ||
                                 Document_MV.DocumentOrgOwnerID.ToString().IsInt() == false || Document_MV.DocumentOrgOwnerID <= 0 ||
                                 Document_MV.DocumentId.ToString().IsInt() == false || Document_MV.DocumentId <= 0)
                        {
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.IsInt],
                                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                            };
                            return Response_MV;
                        }
                        if (haveFile == true)
                        {
                            if (Document_MV.DocumentFile.Length <= 0)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.DocumentFileMustUpload],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                            else if (!Document_MV.DocumentFile.Length.FileSizeIsValid())
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.InvalidFileSize],
                                    Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                                };
                                return Response_MV;
                            }
                        }

                        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) FROM [Document].[V_Documents] WHERE ObjTitle = '{Document_MV.DocumentTitle}' AND ObjId != {Document_MV.DocumentId} AND " +
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


                            var outValue = dam.FireDataTable(exeut);
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
                                if (haveFile == true)
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
                                            string DocumentFileName = SecurityService.RoundomKey(GlobalService.LengthKey) + SecurityService.EnecryptText(outValue.Rows[0][0].ToString()) + SecurityService.RoundomKey(GlobalService.LengthKey) + Path.GetExtension(DocFileNameWithExten).Trim();
                                            string fillPath = Path.Combine(DocFolder, DocumentFileName);
                                            using (FileStream filestream = System.IO.File.Create(fillPath))
                                            {
                                                Document_MV.DocumentFile.CopyTo(filestream);
                                                filestream.Flush();
                                                filestream.Close();
                                            }



                                            //// تشفير
                                            //var MasterKey = SecurityService.EncryptDocument(FilePath, Path.GetDirectoryName(FilePath));
                                            //DataTable DTuserPass = new DataTable();
                                            //DTuserPass = dam.FireDataTable($"SELECT UserId, UsPassword FROM [User].[GetUsersPassHaveReadOnObject]({int.Parse(outValue.Rows[0][0].ToString())})");
                                            //if (DTuserPass.Rows.Count > 0)
                                            //{
                                            //    for (int i = 0; i < DTuserPass.Rows.Count; i++)
                                            //    {
                                            //        var KeyRing = SecurityService.Encrypt(MasterKey, DTuserPass.Rows[0]["UsPassword"].ToString());
                                            //        // insert to keyring table.
                                            //    }
                                            //}


                                        }
                                    }
                                }
                                else
                                {
                                    string getOldDocFile = Path.Combine(await GlobalService.GetDocumentLocationInServerFolder(Document_MV.DocumentId, Environment), Document_MV.DocumentId.ToString(),
                                                                        Path.GetFileName(await GlobalService.GetFullPathOfDocumentNameInServerFolder(Document_MV.DocumentId, Environment)));


                                    var DocFolder = await GlobalService.CreateDocumentFolderInServerFolder(Convert.ToInt32(outValue.Rows[0][0].ToString()), Environment);
                                    if (DocFolder != null)
                                    {
                                        string DocumentFileName = SecurityService.RoundomKey(GlobalService.LengthKey) + SecurityService.EnecryptText(outValue.Rows[0][0].ToString()) + SecurityService.RoundomKey(GlobalService.LengthKey) + Path.GetExtension(getOldDocFile).Trim();
                                        string getNewDocFile = Path.Combine(DocFolder, DocumentFileName);
                                        File.Copy(getOldDocFile, getNewDocFile);






                                        //// تشفير
                                        //var MasterKey = SecurityService.EncryptDocument(getNewDocFile, Path.GetDirectoryName(getNewDocFile));
                                        //DataTable DTuserPass = new DataTable();
                                        //DTuserPass = dam.FireDataTable($"SELECT UserId, UsPassword FROM [User].[GetUsersPassHaveReadOnObject]({int.Parse(outValue.Rows[0][0].ToString())})");
                                        //if (DTuserPass.Rows.Count > 0)
                                        //{
                                        //    for (int i = 0; i < DTuserPass.Rows.Count; i++)
                                        //    {
                                        //        var KeyRing = SecurityService.Encrypt(MasterKey, DTuserPass.Rows[0]["UsPassword"].ToString());
                                        //        // insert to keyring table.
                                        //    }
                                        //}






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
                if (DocumentId.ToString().IsInt() == false || DocumentId <= 0)
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
                                dt = dam.FireDataTable(getDocumentmatedateInfo);
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
                                        DocumentFilePath = await GlobalService.GetFullPathOfDocumentNameInServerFolder(DocumentId, Environment)
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