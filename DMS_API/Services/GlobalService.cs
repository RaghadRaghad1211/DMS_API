using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using Microsoft.AspNetCore.Hosting.Server;
using QRCoder;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Reflection;

namespace DMS_API.Services
{
    public static class GlobalService
    {
        #region Properteis
        private static DataAccessService dam = new DataAccessService(SecurityService.ConnectionString);
        private static SessionService Session_S { get; set; }
        private static DataTable dt { get; set; }
        private static GeneralSerarchModel GeneralSerarch_M { get; set; }
        private static HomeModel Home_M { get; set; }
        private static ResponseModelView Response_MV { get; set; }
        public static readonly int MoodNum = 999;
        public enum ClassType
        {
            User = 1,
            Group = 2,
            Org = 3,
            Folder = 4,
            Document = 5
        }
        #endregion

        #region Functions
        public static string GetMessageColumn(string Lang)
        {
            string Mlang = "MesArName";
            switch (Lang.ToLower())
            {
                case "ar":
                    Mlang = "MesArName";
                    break;
                case "en":
                    Mlang = "MesEnName";
                    break;
                case "kr":
                    Mlang = "MesKrName";
                    break;
                default:
                    Mlang = "MesArName";
                    break;
            }
            return Mlang;
        }
        public static string GetTranslationColumn(string Lang)
        {
            string Mlang = "TrArName";
            switch (Lang.ToLower())
            {
                case "ar":
                    Mlang = "TrArName";
                    break;
                case "en":
                    Mlang = "TrEnName";
                    break;
                case "kr":
                    Mlang = "TrKrName";
                    break;
                default:
                    Mlang = "TrArName";
                    break;
            }
            return Mlang;
        }
        public static string GetTranslationSearchColumn(string Lang)
        {
            string Mlang = "TrKey";
            switch (Lang.ToLower())
            {
                case "ar":
                    Mlang = "TrArName";
                    break;
                case "en":
                    Mlang = "TrEnName";
                    break;
                case "kr":
                    Mlang = "TrKrName";
                    break;
                case "key":
                    Mlang = "TrKey";
                    break;
                default:
                    Mlang = "TrKey";
                    break;
            }
            return Mlang;
        }
        public static string GetUserSearchColumn(SearchUserModelView SearchUser_MV)
        {
            var obj = SearchUser_MV.GetType();
            string where = " WHERE ";
            foreach (PropertyInfo property in obj.GetProperties())
            {
                var name = property.Name;
                var value = property.GetValue(SearchUser_MV, null)?.ToString();
                if (!(value == null || value.Trim() == ""))
                {
                    if (!(name == "PageRows" || name == "PageNumber"))
                    {
                        where = where + $"{name} LIKE '{value}%' AND ";
                    }
                }
            }
            string query = where.Remove(where.Length - 4, 4);
            return query;
        }
        public static string GetQueryAddDocument(DocumentModelView Document_MV)
        {
            //List<KeyValueModel> KeyValue_Mlist = new List<KeyValueModel>();
            //for (int i = 0; i < Document_MV.KeysValues.Count; i++)
            //{
            //    KeyValueModel KeyValue_M = new KeyValueModel()
            //    { Key = Document_MV.KeysValues[i].Key, Value = Document_MV.KeysValues[i].Value };
            //    KeyValue_Mlist.Add(KeyValue_M);
            //}
            //string Query = "";
            //foreach (var item in KeyValue_Mlist)
            //{
            //    Query = Query + item.Key + ":" + item.Value + ",";
            //}
            //Query = Query.Remove(Query.Length - 1, 1);
            //return Query;

            return null;
        }
        public static string GetQueryLinkPro(LinkParentChildModelView LinkParentChild_MV)
        {
            string Query = "";
            for (int i = 0; i < LinkParentChild_MV.ChildIds.Count; i++)
            {

                Query = Query + LinkParentChild_MV.ChildIds[i] + ",";
            }
            Query = Query.Remove(Query.Length - 1, 1);
            return Query;
        }
        public static string GetQueryMoveChilds(MoveChildToNewFolderModelView MoveChildToNewFolder_MV)
        {
            string Query = "";
            for (int i = 0; i < MoveChildToNewFolder_MV.ChildIds.Count; i++)
            {

                Query = Query + MoveChildToNewFolder_MV.ChildIds[i] + ",";
            }
            Query = Query.Remove(Query.Length - 1, 1);
            return Query;
        }
        public static async Task<List<OrgModel>> GetOrgsParentWithChildsByUserLoginID(int userLoginID, bool IsOrgAdmin = false)
        {
            try
            {
                int OrgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                string whereField = OrgOwnerID == 0 ? "OrgUp" : "OrgId";
                string getOrgInfo = $"SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName, OrgIsActive FROM [User].[V_Org]  WHERE {whereField}= {OrgOwnerID} AND OrgIsActive=1";
                //string getOrgInfo = $"SELECT TOP 1 OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName FROM [User].[GetOrgsbyUserId]({userLoginID}) ";
                DataTable dt = new DataTable();
                dt = await Task.Run(() => dam.FireDataTable(getOrgInfo));
                if (dt == null)
                {
                    return null;
                }
                OrgModel Org_M = new OrgModel();
                List<OrgModel> Org_Mlist = new List<OrgModel>();
                if (dt.Rows.Count > 0)
                {
                    Org_M = new OrgModel
                    {
                        OrgId = Convert.ToInt32(dt.Rows[0]["OrgId"].ToString()),
                        OrgUp = Convert.ToInt32(dt.Rows[0]["OrgUp"].ToString()),
                        OrgLevel = Convert.ToInt32(dt.Rows[0]["OrgLevel"].ToString()),
                        OrgArName = dt.Rows[0]["OrgArName"].ToString(),
                        OrgEnName = dt.Rows[0]["OrgEnName"].ToString(),
                        OrgKuName = dt.Rows[0]["OrgKuName"].ToString(),
                        OrgIsActive = bool.Parse(dt.Rows[0]["OrgIsActive"].ToString()),
                        OrgChild = await GlobalService.GetOrgsChilds(Convert.ToInt32(dt.Rows[0]["OrgId"].ToString()))
                    };
                    Org_Mlist.Add(Org_M);
                    return Org_Mlist;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static async Task<List<OrgModel>> GetOrgsChilds(int OrgId)
        {
            try
            {
                string getOrgInfo = $"SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName, OrgIsActive   FROM [User].[GetOrgsChildsByParentId]({OrgId}) WHERE OrgIsActive=1"; // WHERE AND OrgIsActive='{JustActive}'
                DataTable dtChild = new DataTable();
                dtChild = await Task.Run(() => dam.FireDataTable(getOrgInfo));
                OrgModel Org_M1 = new OrgModel();
                List<OrgModel> Org_Mlist1 = new List<OrgModel>();
                if (dtChild.Rows.Count > 0)
                {
                    for (int i = 0; i < dtChild.Rows.Count; i++)
                    {
                        Org_M1 = new OrgModel
                        {
                            OrgId = Convert.ToInt32(dtChild.Rows[i]["OrgId"].ToString()),
                            OrgUp = Convert.ToInt32(dtChild.Rows[i]["OrgUp"].ToString()),
                            OrgLevel = Convert.ToInt32(dtChild.Rows[i]["OrgLevel"].ToString()),
                            OrgArName = dtChild.Rows[i]["OrgArName"].ToString(),
                            OrgEnName = dtChild.Rows[i]["OrgEnName"].ToString(),
                            OrgKuName = dtChild.Rows[i]["OrgKuName"].ToString(),
                            OrgIsActive = bool.Parse(dtChild.Rows[i]["OrgIsActive"].ToString()),
                            OrgChild = await GetOrgsChilds(Convert.ToInt32(dtChild.Rows[i]["OrgId"].ToString()))
                        };
                        Org_Mlist1.Add(Org_M1);

                        //if (IsOrgAdmin == false)
                        //{
                        //    if (Org_M1.OrgIsActive == true)
                        //    {
                        //        Org_Mlist1.Add(Org_M1);
                        //    }
                        //}
                        //else
                        //{
                        //    Org_Mlist1.Add(Org_M1);
                        //}
                    }
                }
                return Org_Mlist1;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<List<OrgTableModel>> GetOrgsParentWithChildsByUserLoginID_Table(int userLoginID)
        {
            try
            {
                string getOrgInfo = "SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName , OrgArNameUp, OrgEnNameUp, OrgKuNameUp, OrgIsActive, ObjDescription  " +
                                   $"FROM [User].[GetOrgsbyUserIdTable]({userLoginID}) ORDER BY OrgId "; // WHERE {whereField} !={OrgOwnerID}
                DataTable dt = new DataTable();
                dt = await Task.Run(() => dam.FireDataTable(getOrgInfo));
                if (dt == null)
                {
                    return null;
                }
                List<OrgTableModel> OrgTable_Mlist = new List<OrgTableModel>();
                OrgTableModel OrgTable_M = new OrgTableModel();


                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        OrgTable_M = new OrgTableModel
                        {
                            OrgId = Convert.ToInt32(dt.Rows[i]["OrgId"].ToString()),
                            OrgUp = Convert.ToInt32(dt.Rows[i]["OrgUp"].ToString()),
                            OrgLevel = Convert.ToInt32(dt.Rows[i]["OrgLevel"].ToString()),
                            OrgArName = dt.Rows[i]["OrgArName"].ToString(),
                            OrgEnName = dt.Rows[i]["OrgEnName"].ToString(),
                            OrgKuName = dt.Rows[i]["OrgKuName"].ToString(),
                            OrgArNameUp = dt.Rows[i]["OrgArNameUp"].ToString(),
                            OrgEnNameUp = dt.Rows[i]["OrgEnNameUp"].ToString(),
                            OrgKuNameUp = dt.Rows[i]["OrgKuNameUp"].ToString(),
                            OrgIsActive = bool.Parse(dt.Rows[i]["OrgIsActive"].ToString()),
                            Note = dt.Rows[i]["ObjDescription"].ToString()
                        };
                        OrgTable_Mlist.Add(OrgTable_M);
                    }

                    return OrgTable_Mlist;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static async Task<ResponseModelView> GeneralSearchByTitle(string title, RequestHeaderModelView RequestHeader)
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
                    if (ValidationService.IsEmpty(title) == true)
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
                        int userLoginID = ((SessionModel)ResponseSession.Data).UserID;
                        int orgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                        string whereField = orgOwnerID == 0 ? "SELECT '0' as OrgId UNION SELECT OrgId" : "SELECT OrgId";

                        string getResult = "SELECT [ObjId], [ObjTitle], [ObjClsId], [ClsName] " +
                                           "FROM   [Main].[V_Objects] " +
                                          $"WHERE  [ObjOrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND ObjTitle LIKE '{title}%' AND ObjIsActive=1 ";

                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(getResult));
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
                        List<GeneralSerarchModel> GeneralSerarch_Mlist = new List<GeneralSerarchModel>();
                        GeneralSerarch_M = new GeneralSerarchModel();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                GeneralSerarch_M = new GeneralSerarchModel
                                {
                                    Id = Convert.ToInt32(dt.Rows[i]["ObjId"].ToString()),
                                    Title = dt.Rows[i]["ObjTitle"].ToString(),
                                    IdType = Convert.ToInt32(dt.Rows[i]["ObjClsId"].ToString()),
                                    Type = dt.Rows[i]["ClsName"].ToString()
                                };
                                GeneralSerarch_Mlist.Add(GeneralSerarch_M);
                            }
                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                                Data = GeneralSerarch_Mlist
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
        public static async Task<ResponseModelView> GetHomeData(PaginationModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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
                    int _PageNumberFav = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                    int _PageRowsFav = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
                    int CurrentPageFav = _PageNumberFav; int PageRowsFav = _PageRowsFav;
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;

                    #region MyDesktopFolder
                    DataTable dtGetDisktopFolder = new DataTable();
                    dtGetDisktopFolder = await Task.Run(() => dam.FireDataTable($"SELECT FolderId, FolderTitle   FROM   [User].[GetFolderDesktopByUserId]({userLoginID})"));
                    MyDesktopFolder MyDesktopFolder = new MyDesktopFolder();
                    List<MyDesktopFolder> MyDesktopFolder_List = new List<MyDesktopFolder>();
                    if (dtGetDisktopFolder.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtGetDisktopFolder.Rows.Count; i++)
                        {
                            MyDesktopFolder = new MyDesktopFolder
                            {
                                FolderDesktopId = Convert.ToInt32(dtGetDisktopFolder.Rows[i]["FolderId"].ToString()),
                                FolderDesktopName = dtGetDisktopFolder.Rows[i]["FolderTitle"].ToString()
                            };
                            MyDesktopFolder_List.Add(MyDesktopFolder);
                        }
                    }
                    #endregion

                    #region MyFavorite
                    DataTable dtGetFavorite = new DataTable();
                    dtGetFavorite = await Task.Run(() => dam.FireDataTable("SELECT ObjFavId AS 'FavoriteId', ObjTitle AS 'FavoriteTitle', ObjClsId AS 'FavTypeId', ClsName AS 'FavTypeName'  " +
                                                                          $"FROM   [User].[V_Favourites] WHERE [ObjUserId] = {userLoginID} AND [IsActive] = 1 " +
                                                                           "ORDER BY    FavoriteId " +
                                                                          $"OFFSET      ({_PageNumberFav}-1)*{_PageRowsFav} ROWS " +
                                                                          $"FETCH NEXT   {_PageRowsFav} ROWS ONLY "));
                    MyFavorite MyFavorite = new MyFavorite();
                    List<MyFavorite> MyFavorite_List = new List<MyFavorite>();
                    if (dtGetFavorite.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtGetFavorite.Rows.Count; i++)
                        {
                            MyFavorite = new MyFavorite
                            {
                                FavoriteId = Convert.ToInt32(dtGetFavorite.Rows[i]["FavoriteId"].ToString()),
                                FavoriteName = dtGetFavorite.Rows[i]["FavoriteTitle"].ToString(),
                                FavTypeId = Convert.ToInt32(dtGetFavorite.Rows[i]["FavTypeId"].ToString()),
                                FavTypeName = dtGetFavorite.Rows[i]["FavTypeName"].ToString()
                            };
                            MyFavorite_List.Add(MyFavorite);
                        }
                    }
                    #endregion

                    #region MyGroup
                    DataTable dtGetGroup = new DataTable();
                    //dtGetGroup = await Task.Run(() => dam.FireDataTable($"SELECT      GroupId, GroupName   FROM    [User].[GetMyGroupsbyUserId]({userLoginID}) " +
                    //                                                    $"ORDER BY    GroupId " +
                    //                                                    $"OFFSET      ({_PageNumber}-1)*{_PageRows} ROWS " +
                    //                                                    $"FETCH NEXT   {_PageRows} ROWS ONLY "));
                    dtGetGroup = await Task.Run(() => dam.FireDataTable($"SELECT      GroupId, GroupName   FROM    [User].[GetMyGroupsbyUserId]({userLoginID}) " +
                                                                        $"ORDER BY    GroupId " +
                                                                        $"OFFSET      ({_PageNumberFav}-1)*{_PageRowsFav} ROWS " +
                                                                        $"FETCH NEXT   {_PageRowsFav} ROWS ONLY "));
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
                    }
                    #endregion


                    Home_M = new HomeModel
                    {
                        MyDesktopFolder = MyDesktopFolder_List,
                        MyFavorites = MyFavorite_List,
                        MyGroups = MyGroup_List
                    };
                    Response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GetSuccess],
                        Data = new { TotalRowsFav = MyFavorite_List.Count, MaxPagFave = Math.Ceiling(MyFavorite_List.Count / (float)_PageRowsFav), CurrentPageFav, PageRowsFav, data = Home_M }//Home_M
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
        public static async Task<string> GetDocumentLocationInServerFolder(int DocumentId, IWebHostEnvironment Environment)
        {
            try
            {
                var path = await Task.Run(() => Environment.WebRootPath + "\\DMSserver");
                int currectFolderDoc = DocumentId % MoodNum;
                return path + "\\" + currectFolderDoc.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static async Task<string> CreateDocumentFolderInServerFolder(int DocumentId, IWebHostEnvironment Environment)
        {
            try
            {
                var path = Path.Combine(await GetDocumentLocationInServerFolder(DocumentId, Environment), DocumentId.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static async Task<bool> IsFolderOpenableToMoveInsideIt(int FolderId, List<int> ObjectsIds)
        {
            if (ObjectsIds.Count > 0)
            {
                foreach (var id in ObjectsIds)
                {
                    if (FolderId == id)
                    {
                        return await Task.FromResult(false);
                    }
                }
                return await Task.FromResult(true);
            }
            return await Task.FromResult(true);
        }
        public static async Task<PermissionTypeModel> CheckUserPermissionsFolderAndDocument(SessionModel ResponseSession, int ObjectId)
        {
            try
            {
                if (ResponseSession.IsOrgAdmin == false && ResponseSession.IsGroupOrgAdmin == false)
                {
                    int ParintId = int.Parse(dam.FireSQL($"SELECT LcParentObjId   FROM [User].[V_Links] WHERE [LcChildObjId]={ObjectId}   "));
                    string getPermissions = "SELECT  [SourObjId], [DestObjId], [IsRead], [IsWrite], [IsManage], [IsQR] " +
                                           $"FROM    [Document].[GetChildsInParentWithPermissions] ({ResponseSession.UserID}, {ParintId}) WHERE SourObjId={ObjectId} ";

                    DataTable dt = new DataTable();
                    dt = await Task.Run(() => dam.FireDataTable(getPermissions));
                    if (dt == null)
                    {
                        return null;
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            PermissionTypeModel PerType_M = new PermissionTypeModel
                            {
                                UserId = Convert.ToInt32(dt.Rows[0]["DestObjId"].ToString()),
                                ObjectId = Convert.ToInt32(dt.Rows[0]["SourObjId"].ToString()),
                                IsRead = bool.Parse(dt.Rows[0]["IsRead"].ToString()),
                                IsWrite = bool.Parse(dt.Rows[0]["IsWrite"].ToString()),
                                IsManage = bool.Parse(dt.Rows[0]["IsManage"].ToString()),
                                IsQR = bool.Parse(dt.Rows[0]["IsQR"].ToString())
                            };
                            return PerType_M;
                        }
                        return null;
                    }
                }
                else
                {
                    PermissionTypeModel PerType_M = new PermissionTypeModel
                    {
                        UserId = ResponseSession.UserID,
                        ObjectId = ObjectId,
                        IsRead = true,
                        IsWrite = true,
                        IsManage = true,
                        IsQR = true
                    };
                    return PerType_M;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static async Task<object> CreateQRcodePNG(string QRtext, int DocumentId, IWebHostEnvironment Environment)
        {
            try
            {
                //QRCodeGenerator QrGenerator = new QRCodeGenerator();
                //QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(QRtext, QRCodeGenerator.ECCLevel.Q);
                //QRCoder.QRCode QrCode = new QRCoder.QRCode(QrCodeInfo);
                //Bitmap QrBitmap = QrCode.GetGraphic(60);

                //MemoryStream ms = new MemoryStream();
                //QrBitmap.Save(ms, ImageFormat.Png);
                //byte[] BitmapArray = ms.ToArray();
                //string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                //return QrUri;






                //QRCodeGenerator QrGenerator = new QRCodeGenerator();
                //QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(QRtext, QRCodeGenerator.ECCLevel.Q);
                //QRCoder.QRCode QrCode = new QRCoder.QRCode(QrCodeInfo);
                ////   System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
                //// imgBarCode.Height = 150;
                //// imgBarCode.Width = 150;
                //using (Bitmap bitMap = QrCode.GetGraphic(60))
                //{
                //    using (MemoryStream ms = new MemoryStream())
                //    {
                //        bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //        byte[] byteImage = ms.ToArray();
                //        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                //        img.Save(Server.MapPath("Images/") + "Test.Jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                //        imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);
                //    }
                //    plBarCode.Controls.Add(imgBarCode);
                //}


                //// var bitmapBytes = BitmapToBytes(qrCodeImage); //Convert bitmap into a byte array
                //return File(bitmapBytes, "image/jpeg"); //Return as file result






                return null;


            }
            catch (Exception ex)
            {
                return null;
            }
        }



        //var path = Path.Combine(await GetDocumentLocationInServerFolder(DocumentId, Environment), DocumentId.ToString());

        #endregion
    }

}