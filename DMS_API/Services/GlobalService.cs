using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using iTextSharp.text.pdf;
using iTextSharp.text;
using QRCoder;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Reflection;
using System.Text;
using System.Reflection.Metadata;
using Microsoft.Extensions.FileProviders;

namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Different Methods
    /// </summary>
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
        public const int LengthKey = 10;
        public static readonly string MaxFileSize = ((100 / 1024f) * 1024f).ToString("0.00");
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
        /// <summary>
        /// Get column name from Message table in database which depends on language
        /// </summary>
        /// <param name="Lang">Language</param>
        /// <returns></returns>
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
        /// <summary>
        /// Get column name from Translation table in database which depends on language
        /// </summary>
        /// <param name="Lang">Language</param>
        /// <returns></returns>
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
        /// <summary>
        /// Get column name from Message table in database for search which depends on language
        /// </summary>
        /// <param name="Lang">Language</param>
        /// <returns></returns>
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
        /// <summary>
        /// Get query for advance Search User 
        /// </summary>
        /// <param name="SearchUser_MV">Search Model</param>
        /// <returns></returns>
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
        /// <summary>
        /// Get query for add document
        /// </summary>
        /// <param name="Document_MV"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get query for link between parent and childs,
        /// for add, remove and move.
        /// </summary>
        /// <param name="ChildIds">Childs Id</param>
        /// <returns></returns>
        public static string GetQueryLinkPro(List<int> ChildIds)
        {
            string Query = "";
            for (int i = 0; i < ChildIds.Count; i++)
            {
                Query = Query + ChildIds[i] + ",";
            }
            Query = Query.Remove(Query.Length - 1, 1);
            return Query;
        }
        /// <summary>
        /// Get tree of organization with childs,
        /// which depends on user login Id.
        /// </summary>
        /// <param name="userLoginID"> user login Id</param>
        /// <returns></returns>
        public static async Task<List<OrgModel>> GetOrgsParentWithChildsByUserLoginID(int userLoginID)
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
        /// <summary>
        /// /// Get childs of organization,
        /// which depends on organization Id.
        /// </summary>
        /// <param name="OrgId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get flat of organization with childs,
        /// which depends on user login Id.
        /// </summary>
        /// <param name="userLoginID">user login Id</param>
        /// <returns></returns>
        public static async Task<List<OrgTableModel>> GetOrgsParentWithChildsByUserLoginID_Table(int userLoginID)
        {
            try
            {
                string getOrgInfo = "SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName , OrgArNameUp, OrgEnNameUp, OrgKuNameUp, OrgIsActive, ObjDescription  " +
                                   $"FROM [User].[GetOrgsbyUserIdTable]({userLoginID}) ORDER BY OrgId ";
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
        /// <summary>
        /// General serach users and groups
        /// </summary>
        /// <param name="title">Title Search</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns></returns>
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
                                          $"WHERE  [ObjOrgOwner] IN ({whereField} FROM [User].GetOrgsbyUserId({userLoginID})) AND " +
                                          $"       [ObjClsId] IN ({(int)ClassType.User,(int)ClassType.Group}) " +
                                          $"ObjTitle LIKE '{title}%' AND ObjIsActive=1 ";

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
        /// <summary>
        /// Get desktop folder, favourites folders and groups of user login 
        /// </summary>
        /// <param name="Pagination_MV">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <returns></returns>
        public static async Task<ResponseModelView> GetHomeData(PaginationHomeModelView Pagination_MV, RequestHeaderModelView RequestHeader)
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

                    #region MyDesktopFolder
                    int _PageNumberDesktop = Pagination_MV.PageNumberDesktop == 0 ? 1 : Pagination_MV.PageNumberDesktop;
                    int _PageRowsDesktop = Pagination_MV.PageRowsDesktop == 0 ? 1 : Pagination_MV.PageRowsDesktop;
                    var MaxTotalDesktop = dam.FireDataTable($"SELECT COUNT(*) AS TotalRowsDesktop, CEILING(COUNT(*) / CAST({_PageRowsDesktop} AS FLOAT)) AS MaxPageDesktop " +
                                                         $"FROM        [User].[GetFolderDesktopByUserId]({userLoginID}) ");

                    DataTable dtGetDisktopFolder = new DataTable();
                    dtGetDisktopFolder = await Task.Run(() => dam.FireDataTable("SELECT      FolderId, FolderTitle  " +
                                                                               $"FROM        [User].[GetFolderDesktopByUserId]({userLoginID}) " +
                                                                                "ORDER BY    FolderId " +
                                                                               $"OFFSET      ({_PageNumberDesktop}-1)*{_PageRowsDesktop} ROWS " +
                                                                               $"FETCH NEXT   {_PageRowsDesktop} ROWS ONLY "));
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
                    int _PageNumberFav = Pagination_MV.PageNumberFav == 0 ? 1 : Pagination_MV.PageNumberFav;
                    int _PageRowsFav = Pagination_MV.PageRowsFav == 0 ? 1 : Pagination_MV.PageRowsFav;
                    var MaxTotalFav = dam.FireDataTable($"SELECT COUNT(*) AS TotalRowsFav, CEILING(COUNT(*) / CAST({_PageRowsFav} AS FLOAT)) AS MaxPageFav " +
                                                         $"FROM   [User].[V_Favourites] WHERE [ObjUserId] = {userLoginID} AND [IsActive] = 1  ");
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
                    int _PageNumberGroup = Pagination_MV.PageNumberGroup == 0 ? 1 : Pagination_MV.PageNumberGroup;
                    int _PageRowsGroup = Pagination_MV.PageRowsGroup == 0 ? 1 : Pagination_MV.PageRowsGroup;
                    var MaxTotalGroup = dam.FireDataTable($"SELECT COUNT(*) AS TotalRowsGroup, CEILING(COUNT(*) / CAST({_PageRowsGroup} AS FLOAT)) AS MaxPageGroup " +
                                                         $"FROM    [User].[GetMyGroupsbyUserId]({userLoginID}) ");
                    DataTable dtGetGroup = new DataTable();
                    dtGetGroup = await Task.Run(() => dam.FireDataTable($"SELECT      GroupId, GroupName   FROM    [User].[GetMyGroupsbyUserId]({userLoginID}) " +
                                                                        $"ORDER BY    GroupId " +
                                                                        $"OFFSET      ({_PageNumberGroup}-1)*{_PageRowsGroup} ROWS " +
                                                                        $"FETCH NEXT   {_PageRowsGroup} ROWS ONLY "));
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
                        Data = new
                        {
                            TotalRowsDesktop = MaxTotalDesktop.Rows[0]["TotalRowsDesktop"],
                            MaxPageDesktop = MaxTotalDesktop.Rows[0]["MaxPageDesktop"],
                            CurrentPageDesktop = _PageNumberDesktop,
                            PageRowsDesktop = _PageRowsDesktop,

                            TotalRowsFav = MaxTotalFav.Rows[0]["TotalRowsFav"],
                            MaxPagFav = MaxTotalFav.Rows[0]["MaxPageFav"],
                            CurrentPageFav = _PageNumberFav,
                            PageRowsFav = _PageRowsFav,

                            TotalRowsGroup = MaxTotalGroup.Rows[0]["TotalRowsGroup"],
                            MaxPagGroup = MaxTotalGroup.Rows[0]["MaxPageGroup"],
                            CurrentPageGroup = _PageNumberGroup,
                            PageRowsGroup = _PageRowsGroup,

                            data = Home_M
                        }
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
        /// <summary>
        /// Get folder location in server where document save it.
        /// </summary>
        /// <param name="DocumentId">Document Id</param>
        /// <param name="Environment">Environment Parameter</param>
        /// <returns></returns>
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
        /// <summary>
        /// Get temp folder location in server where QR PDF save it.
        /// </summary>
        /// <param name="Environment">Environment Parameter</param>
        /// <returns></returns>
        public static async Task<string> GetTempQrLocationInServerFolder(IWebHostEnvironment Environment)
        {
            try
            {
                var path = await Task.Run(() => Path.Combine(Environment.WebRootPath, "DMSserver", $"QRtemp_{DateTime.Now.ToString("dd-MM-yyyy")}"));
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Create and save document in folder server.
        /// </summary>
        /// <param name="DocumentId">Document Id</param>
        /// <param name="Environment">Environment Parameter</param>
        /// <returns></returns>
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
        /// <summary>
        /// Get full path of document with name and extintion in folder server.
        /// </summary>
        /// <param name="DocumentId">Document Id</param>
        /// <param name="LengthKey">Length Key encrypted</param>
        /// <param name="Environment">Environment Parameter</param>
        /// <returns></returns>
        public static async Task<string> GetFullPathOfDocumentNameInServerFolder(int DocumentId, int LengthKey, IWebHostEnvironment Environment)
        {
            try
            {
                string getParentFolder = SecurityService.HostFilesUrl + "/" + (DocumentId % GlobalService.MoodNum).ToString() + "/" + DocumentId.ToString();
                string[] getFiles = Directory.GetFiles(
                                              Path.Combine(
                                             await GlobalService.GetDocumentLocationInServerFolder(DocumentId, Environment),
                                                   DocumentId.ToString()), "*.pdf");
                foreach (var file in getFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName.Contains(SecurityService.EnecryptText(DocumentId.ToString())) && fileName.Length == LengthKey * 2 + SecurityService.EnecryptText(DocumentId.ToString()).Length)
                    {
                        return getParentFolder + "/" + Path.GetFileName(file);
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
            #region OldCode
            //string getPath = SecurityService.HostFilesUrl + "/" +
            //                    (DocumentId % GlobalService.MoodNum).ToString() + "/" +
            //                     DocumentId.ToString() + "/" +
            //                     Path.GetFileName(
            //                          Directory.GetFiles(
            //                                    Path.Combine(
            //                                   await GlobalService.GetDocumentLocationInServerFolder(DocumentId, Environment),
            //                                         DocumentId.ToString())).
            //                                                         SingleOrDefault(
            //                                                                 x => Path.GetFileName(x).
            //                                                                 Remove(0, LengthKey).
            //                                                                 StartsWith(SecurityService.EnecryptText(DOC_QR.ToString()))));
            #endregion
        }
        /// <summary>
        /// Get full path of QR PDF with name and extintion in folder server.
        /// </summary>
        /// <param name="QrFileName">Qr File Name</param>
        /// <returns></returns>
        public static async Task<string> GetFullPathOfQrPdfNameInServerFolder(string QrFileName)
        {
            try
            {
                string getQR_PDF = await Task.Run(() => SecurityService.HostFilesUrl + "/" + $"QRtemp_{DateTime.Now.ToString("dd-MM-yyyy")}" + "/" + QrFileName + ".pdf");
                return getQR_PDF;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Check the folder can open for move action.
        /// </summary>
        /// <param name="FolderId">Folder Id</param>
        /// <param name="ObjectsIds">Objects Id you want to move</param>
        /// <returns></returns>
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
        /// <summary>
        /// Get user permissions (IsRead, IsWrite, IsManage, IsQR) on folder, document. 
        /// Get full permissions for Admin.
        /// depends on user login Id
        /// </summary>
        /// <param name="ResponseSession">Body Parameter</param>
        /// <param name="ObjectId">Object Id of folder, document</param>
        /// <returns></returns>
        public static async Task<PermissionTypeModel> CheckUserPermissionsOnFolderAndDocument(SessionModel ResponseSession, int ObjectId)
        {
            try
            {
                if (ResponseSession.IsOrgAdmin == false && ResponseSession.IsGroupOrgAdmin == false)
                {
                    string ParentId = dam.FireSQL($"SELECT LcParentObjId   FROM [User].[V_Links] WHERE [LcChildObjId]={ObjectId}   ");
                    if (ParentId.IsEmpty() == true)
                    {
                        PermissionTypeModel PerType_M = new PermissionTypeModel
                        {
                            UserId = ResponseSession.UserID,
                            ObjectId = ObjectId,
                            IsRead = true,
                            IsWrite = false,
                            IsManage = false,
                            IsQR = false
                        };
                        return PerType_M;
                    }
                    else
                    {
                        string getPermissions = "SELECT  [SourObjId], [IsRead], [IsWrite], [IsManage], [IsQR] " +
                                               $"FROM    [Document].[GetChildsInParentWithPermissions] ({ResponseSession.UserID}, {Convert.ToInt32(ParentId)}) WHERE SourObjId={ObjectId} ";

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
                                    UserId = ResponseSession.UserID,//Convert.ToInt32(dt.Rows[0]["DestObjId"].ToString()),
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
        /// <summary>
        /// Generate PDF file with QR code for document, and return the path of PDF
        /// </summary>
        /// <param name="QRLookup_M">Body Parameters</param>
        /// <param name="RequestHeader">Header Parameters</param>
        /// <param name="Environment">Environment parameter</param>
        /// <returns></returns>
        public static async Task<ResponseModelView> GenerateQRcodePDF(QRLookupModel QRLookup_M, RequestHeaderModelView RequestHeader, IWebHostEnvironment Environment)
        {
            try
            {
                string QRquery = "INSERT INTO [Main].[QRLookup] (QrObjId, QrIsPraivet, QrExpiry, QrIsActive) OUTPUT INSERTED.QrId " +
                                        $"VALUES({QRLookup_M.QrDocumentId}, {Convert.ToInt16(QRLookup_M.QrIsPraivet)}, '{DateTime.Now}', {1}) ";
                string outValueQRcodeId = await Task.Run(() => dam.DoQueryAndPutOutValue(QRquery, "QrId"));
                if (outValueQRcodeId == null || outValueQRcodeId.Trim() == "")
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
                    Session_S = new SessionService();
                    var ResponseSession = await Session_S.CheckAuthorizationResponse(RequestHeader);
                    DataTable getUserLoginInfo = new DataTable();
                    getUserLoginInfo = dam.FireDataTable($"SELECT  FullName, UsUserName  FROM [User].[V_Users] WHERE UserID={((SessionModel)ResponseSession.Data).UserID} ");

                    DataTable getDocInfo = new DataTable();
                    getDocInfo = dam.FireDataTable($"SELECT  ObjTitle, ObjCreationDate, OrgArName  FROM [Document].[V_Documents] WHERE ObjId={QRLookup_M.QrDocumentId} ");

                    #region Design PDF
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    string MyFont = Environment.WebRootPath + "\\Fonts\\ARIALBD.TTF";
                    BaseFont bf = BaseFont.CreateFont(MyFont, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                    #region Header
                    PdfPTable HeaderTable = new PdfPTable(3)
                    {
                        WidthPercentage = 100f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        SpacingBefore = 10,
                        DefaultCell =
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border=0, BorderWidth=0
                    }
                    };

                    Paragraph ParHeaderTitle = new Paragraph($"عنوان الوثيقة: {getDocInfo.Rows[0]["ObjTitle"].ToString()}")
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.VerySmall, 0, PdfSettingsModel.BLACK),
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell CellHeaderTitle = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellHeaderTitle.AddElement(ParHeaderTitle);

                    Paragraph ParHeaderOrg = new Paragraph($"التشكيل: {getDocInfo.Rows[0]["OrgArName"].ToString()}")
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.VerySmall, 0, PdfSettingsModel.BLACK),
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell CellHeaderOrg = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0
                    };
                    CellHeaderOrg.AddElement(ParHeaderOrg);

                    Paragraph ParHeaderCreationDate = new Paragraph($"تاريخ انشاء الوثيقة: {DateTime.Parse(getDocInfo.Rows[0]["ObjCreationDate"].ToString()).ToShortDateString()}")
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.VerySmall, 0, PdfSettingsModel.BLACK),
                        Alignment = Element.ALIGN_CENTER
                    };
                    PdfPCell CellHeaderCreationDate = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_CENTER,
                        Border = 0,
                        BorderWidthBottom = 0
                    };
                    CellHeaderCreationDate.AddElement(new Phrase(ParHeaderCreationDate));


                    Paragraph ParHeaderCreateQR = new Paragraph($"تاريخ رمز التحقق: {DateTime.Now}")
                    {
                        Font = new iTextSharp.text.Font(bf, 11, 0, PdfSettingsModel.BLACK),
                        Alignment = Element.ALIGN_CENTER
                    };
                    PdfPCell CellHeaderCreateQR = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_LEFT,
                        Border = 0,
                        BorderWidthBottom = 0
                    };
                    CellHeaderCreateQR.AddElement(new Phrase(ParHeaderCreateQR));


                    string imageDefoltLogo = Environment.WebRootPath + "\\Logos\\DefultLogo.png";
                    iTextSharp.text.Image LogoPNG = iTextSharp.text.Image.GetInstance(imageDefoltLogo);
                    LogoPNG.ScaleToFit(100f, 100f);
                    LogoPNG.ScalePercent(5f);
                    LogoPNG.SpacingBefore = 2f;
                    LogoPNG.SpacingAfter = 2f;
                    LogoPNG.Alignment = Element.ALIGN_CENTER;
                    PdfPCell CellHeaderLogo = new PdfPCell(LogoPNG)
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Colspan = 1,
                        Border = 0,
                        BorderWidthBottom = 0
                    };


                    PdfPCell CellEmpty = new PdfPCell(new Phrase(" ")) { Border = 0 };

                    HeaderTable.AddCell(CellEmpty);
                    HeaderTable.AddCell(CellEmpty);
                    HeaderTable.AddCell(CellEmpty);
                    HeaderTable.AddCell(CellHeaderCreateQR);
                    HeaderTable.AddCell(CellEmpty);
                    HeaderTable.AddCell(CellHeaderTitle);
                    HeaderTable.AddCell(CellEmpty);
                    HeaderTable.AddCell(CellEmpty);
                    HeaderTable.AddCell(CellEmpty);
                    HeaderTable.AddCell(CellHeaderCreationDate);
                    HeaderTable.AddCell(CellHeaderLogo);
                    HeaderTable.AddCell(CellHeaderOrg);
                    #endregion

                    #region Line
                    PdfPTable LineTable = new PdfPTable(1)
                    {
                        WidthPercentage = 100f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        SpacingBefore = 20,
                        DefaultCell =
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border=2, BorderWidth=2
                    }
                    };
                    LineTable.AddCell("");
                    #endregion

                    #region BeforQR
                    PdfPTable BeforQRTable = new PdfPTable(1)
                    {
                        WidthPercentage = 100f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        SpacingBefore = 10,
                        DefaultCell =
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border=0, BorderWidth=0
                    }
                    };

                    Paragraph ParBeforQRLine1 = new Paragraph(PdfSettingsModel.ParagraphBeforQRLine1)
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.Medium, 0, PdfSettingsModel.BLACK),
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell CellBeforQRLine1 = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellBeforQRLine1.AddElement(ParBeforQRLine1);

                    Paragraph ParBeforQRLine2 = new Paragraph(PdfSettingsModel.ParagraphBeforQRLine2)
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.Medium, 0, PdfSettingsModel.BLACK),
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell CellBeforQRLine2 = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellBeforQRLine2.AddElement(ParBeforQRLine2);

                    Paragraph ParBeforQRLine3 = new Paragraph(PdfSettingsModel.ParagraphBeforQRLine3)
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.Larg, 0, PdfSettingsModel.RED),
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell CellBeforQRLine3 = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellBeforQRLine3.AddElement(ParBeforQRLine3);


                    BeforQRTable.AddCell(CellBeforQRLine1);
                    BeforQRTable.AddCell("\n");
                    BeforQRTable.AddCell(CellBeforQRLine2);
                    BeforQRTable.AddCell("\n");
                    BeforQRTable.AddCell(CellBeforQRLine3);
                    #endregion

                    #region QR
                    PdfPTable QRTable = new PdfPTable(1)
                    {
                        WidthPercentage = 100f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        SpacingBefore = 10,
                        DefaultCell =
                                {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border=0, BorderWidth=0
                                 }
                    };

                    var path = await GetTempQrLocationInServerFolder(Environment);
                    string QrFileName = SecurityService.RoundomKey(GlobalService.LengthKey) + SecurityService.EnecryptText(outValueQRcodeId.ToString()) + SecurityService.RoundomKey(GlobalService.LengthKey);
                    string fullPathQR = Path.Combine(path, QrFileName) + ".pdf";
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();                    
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(QrFileName, QRCodeGenerator.ECCLevel.H);
                    QRCoder.QRCode QrCode = new QRCoder.QRCode(QrCodeInfo);
                    Bitmap UrBitmap = new Bitmap(Environment.WebRootPath + "\\Logos\\URlogo.png");
                    Bitmap QrBitmap = QrCode.GetGraphic(60,Color.Black,Color.White, UrBitmap,30);
                    //Bitmap QrBitmap = QrCode.GetGraphic(PdfSettingsModel.QRsize, Color.Black,Color.White,true);
                    byte[] bytes;
                    using (MemoryStream memory = new MemoryStream())
                    {
                        QrBitmap.Save(memory, ImageFormat.Png);
                        bytes = memory.ToArray();
                    }
                    iTextSharp.text.Image QRpng = iTextSharp.text.Image.GetInstance(bytes);
                    QRpng.ScaleToFit(100f, 100f);
                    QRpng.ScalePercent(8f);
                    QRpng.Alignment = Element.ALIGN_CENTER;
                    PdfPCell CellQRimage = new PdfPCell(QRpng)
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_CENTER,
                        Colspan = 1,
                        Border = 0
                    };
                    Paragraph ParQRcode = new Paragraph(QrFileName)
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.VerySmall, 0, PdfSettingsModel.BLACK),
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell CellQRcode = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellQRcode.AddElement(ParQRcode);

                    QRTable.AddCell(CellQRimage);
                    QRTable.AddCell(CellQRcode);
                    QRTable.AddCell("\n");
                    #endregion

                    #region AfterQR
                    PdfPTable AfterQRTable = new PdfPTable(1)
                    {
                        WidthPercentage = 100f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        SpacingBefore = 10,
                        DefaultCell =
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border=0, BorderWidth=0
                    }
                    };

                    Paragraph ParAfterQRLine1 = new Paragraph(PdfSettingsModel.ParagraphAfterQRLine1)
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.Small, 0, PdfSettingsModel.BLUE),
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell CellAfterQRLine1 = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellAfterQRLine1.AddElement(ParAfterQRLine1);

                    Paragraph ParAfterQRLine2 = new Paragraph(PdfSettingsModel.ParagraphAfterQRLine2)
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.Small, 0, PdfSettingsModel.BLUE),
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell CellAfterQRLine2 = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellAfterQRLine2.AddElement(ParAfterQRLine2);

                    AfterQRTable.AddCell(CellAfterQRLine1);
                    AfterQRTable.AddCell("\n");
                    AfterQRTable.AddCell(CellAfterQRLine2);
                    #endregion

                    #region Footer
                    PdfPTable FooterTable = new PdfPTable(1)
                    {
                        WidthPercentage = 100f,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        RunDirection = Element.ALIGN_RIGHT,
                        SpacingBefore = 10,
                        DefaultCell =
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_LEFT,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border=0, BorderWidth=0
                    }
                    };

                    Paragraph ParFooterLine1 = new Paragraph(PdfSettingsModel.ParagraphFooterLine1)
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.VeryMini, 0, PdfSettingsModel.RED),
                        Alignment = Element.ALIGN_LEFT,
                    };
                    PdfPCell CellFooterLine1 = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_LEFT,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellFooterLine1.AddElement(ParFooterLine1);

                    Paragraph ParFooterLine2 = new Paragraph(getUserLoginInfo.Rows[0]["UsUserName"].ToString())
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.VeryMini, 0, PdfSettingsModel.RED),
                        Alignment = Element.ALIGN_LEFT,
                    };
                    PdfPCell CellFooterLine2 = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_LEFT,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellFooterLine2.AddElement(ParFooterLine2);

                    Paragraph ParFooterLine3 = new Paragraph(getUserLoginInfo.Rows[0]["FullName"].ToString())
                    {
                        Font = new iTextSharp.text.Font(bf, (float)PdfSettingsModel.FontSize.VeryMini, 0, PdfSettingsModel.RED),
                        Alignment = Element.ALIGN_LEFT,
                    };
                    PdfPCell CellFooterLine3 = new PdfPCell
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_LEFT,
                        RunDirection = Element.ALIGN_RIGHT,
                        Border = 0,
                        BorderWidthBottom = 0,
                    };
                    CellFooterLine3.AddElement(ParFooterLine3);

                    FooterTable.AddCell("\n");
                    FooterTable.AddCell(CellFooterLine1);
                    FooterTable.AddCell(CellFooterLine2);
                    FooterTable.AddCell(CellFooterLine3);
                    #endregion

                    using (FileStream stream = new FileStream(fullPathQR, FileMode.Create, FileAccess.ReadWrite))
                    {
                        iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4, 10f, 10f, 10f, 10f);
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();
                        pdfDoc.Add(HeaderTable);
                        pdfDoc.Add(LineTable);
                        pdfDoc.Add(BeforQRTable);
                        pdfDoc.Add(QRTable);
                        pdfDoc.Add(AfterQRTable);
                        pdfDoc.Add(FooterTable);
                        pdfDoc.NewPage();
                        pdfDoc.Close();
                        stream.Close();
                    }
                    #endregion

                    Response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GeneratQR],
                        Data = new { QrPdfFilePath = GetFullPathOfQrPdfNameInServerFolder(QrFileName) }
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
        #endregion
    }
}