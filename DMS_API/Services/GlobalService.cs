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
        public const int LengthKey = 15;
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
            catch (Exception ex)
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
                    int userLoginID = ((SessionModel)ResponseSession.Data).UserID;

                    #region MyDesktopFolder
                    int _PageNumberDesktop = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                    int _PageRowsDesktop = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
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
                    int _PageNumberFav = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                    int _PageRowsFav = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
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
                    int _PageNumberGroup = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                    int _PageRowsGroup = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
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
            string getDocPath = SecurityService.HostFilesUrl + "/" +
                                (DocumentId % GlobalService.MoodNum).ToString() + "/" +
                                 DocumentId.ToString() + "/" +
                                 Path.GetFileName(
                                      Directory.GetFiles(
                                                Path.Combine(
                                               await GlobalService.GetDocumentLocationInServerFolder(DocumentId, Environment),
                                                     DocumentId.ToString())).
                                                                     FirstOrDefault(
                                                                             x => Path.GetFileName(x).
                                                                             Remove(0, LengthKey).
                                                                             StartsWith(DocumentId.ToString())));
            return getDocPath;

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
                                        $"VALUE({QRLookup_M.QrDocumentId}, {QRLookup_M.QrIsPraivet}, '{dam.DoQuery("GETDATE")}', {QRLookup_M.QrIsActive}) ";
                string outValue = await Task.Run(() => dam.DoQueryAndPutOutValue(QRquery, "QrId"));
                if (outValue == null || outValue.Trim() == "")
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
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(QRLookup_M.QrId.ToString(), QRCodeGenerator.ECCLevel.H);
                    QRCoder.QRCode QrCode = new QRCoder.QRCode(QrCodeInfo);
                    Bitmap QrBitmap = QrCode.GetGraphic(60);
                    var path = Path.Combine(await GetDocumentLocationInServerFolder(QRLookup_M.QrDocumentId, Environment), QRLookup_M.QrDocumentId.ToString());
                    string QrFileName = SecurityService.RoundomKey(GlobalService.LengthKey) + QRLookup_M.QrId.ToString() + SecurityService.RoundomKey(GlobalService.LengthKey) + ".png";
                    string fullPathQR = Path.Combine(path, QrFileName);
                    //QrBitmap.Save(fullPathQR);
                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (FileStream fs = new FileStream(fullPathQR, FileMode.Create, FileAccess.ReadWrite))
                        {
                            QrBitmap.Save(memory, ImageFormat.Png);
                            byte[] bytes = memory.ToArray();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                    Response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = MessageService.MsgDictionary[RequestHeader.Lang.ToLower()][MessageService.GeneratQR],
                        Data = "the path of pdf with QR location"
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

        public static void PrintReceipt()
        {
            try
            {
                #region Section-FontTablesSettings
                //اضافة نوع خط للغة العربية
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                FontSelector selector = new FontSelector();
                string fontLoc = "F:\\TestProjects\\DMS_API\\DMS_API\\wwwroot\\Fonts\\ARIAL.TTF";
                // "C:\Users\pc\source\repos\WindowsFormsApp1\WindowsFormsApp1\Fonts\Scheherazade-Regular.ttf"              
                BaseFont bf = BaseFont.CreateFont(fontLoc, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 18);
                iTextSharp.text.Font fontMiddle = new iTextSharp.text.Font(bf, 20);
                iTextSharp.text.Font fontsmall = new iTextSharp.text.Font(bf, 14);
                // iTextSharp.text.Font red = new iTextSharp.text.Font(bf,16);
                iTextSharp.text.Font fontRed = new iTextSharp.text.Font(bf, 16);//, new iTextSharp.text.BaseColor(255, 0, 0));
                iTextSharp.text.Font fontblue = new iTextSharp.text.Font(bf, 16);
                selector.AddFont(FontFactory.GetFont(FontFactory.TIMES_ROMAN, 16));

                PdfPTable HeaderTable = new PdfPTable(3);//الجدول الاول الذي يضم صورة الشعار والمعلومات  الكتاب العامة
                PdfPTable pdfLineTable = new PdfPTable(1);//like line
                PdfPTable pdfTable1 = new PdfPTable(1);//ان حفاظك النص             
                PdfPTable QRTable = new PdfPTable(1);//الجدول الذي يضم qr image               
                PdfPTable pdfTable2 = new PdfPTable(1);//النص لمزيد من المعلومات
                PdfPTable FooterTable = new PdfPTable(1);//لطباعة الوقت والتاريخ الحالي

                HeaderTable.WidthPercentage = 100f;
                HeaderTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                HeaderTable.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                HeaderTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                HeaderTable.RunDirection = PdfPCell.ALIGN_RIGHT;

                pdfTable1.WidthPercentage = 100f;
                pdfTable1.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfTable1.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfTable1.DefaultCell.BorderWidth = 0;
                pdfTable1.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                pdfTable1.RunDirection = PdfPCell.ALIGN_RIGHT;

                pdfTable2.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                pdfTable2.RunDirection = PdfPCell.ALIGN_RIGHT;
                pdfTable2.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                pdfTable2.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfTable2.DefaultCell.BorderWidth = 0;
                pdfTable2.WidthPercentage = 100f;

                FooterTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                FooterTable.RunDirection = PdfPCell.ALIGN_RIGHT;
                FooterTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                FooterTable.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                FooterTable.DefaultCell.BorderWidth = 0;
                FooterTable.WidthPercentage = 100f;

                #endregion
                #region Section-HeaderTable

                //add first part
                /************************************************/
                //Chunk ch1 = new Chunk(" العنوان: علي نضال احمد محمد", font);
                Paragraph para1 = new Paragraph(" العنوان: علي نضال احمد محمد", fontsmall); para1.Alignment = Element.ALIGN_CENTER;
                para1.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                para1.Font.SetStyle(0); para1.Font.Size = 12;

                Paragraph para2 = new Paragraph(" دائرة كاتب عدل الاعظمية ", fontsmall); para2.Alignment = Element.ALIGN_CENTER;
                para2.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                para2.Font.SetStyle(0); para1.Font.Size = 12;
                Chunk ch2 = new Chunk(" دائرة كاتب عدل الاعظمية ", fontsmall);
                ch2.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                ch2.Font.SetStyle(0); ch2.Font.Size = 12;
                Chunk ch3 = new Chunk("0تاريخ الاضافة في النظام :2/01/2023", fontsmall);
                ch3.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                ch3.Font.SetStyle(0); ch3.Font.Size = 12;

                PdfPCell Cell1 = new PdfPCell(); Cell1.AddElement(para1); Cell1.HorizontalAlignment = Element.ALIGN_RIGHT;
                PdfPCell Cell2 = new PdfPCell(); Cell2.AddElement(ch2); Cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                PdfPCell Cell3 = new PdfPCell(); Cell3.AddElement(ch3); Cell3.HorizontalAlignment = Element.ALIGN_RIGHT;
                PdfPCell Cell4 = new PdfPCell(new Phrase(""));
                PdfPCell Cell5 = new PdfPCell(new Phrase(""));


                //اضافة الشعار اعلى الكتاب
                string imageURL = "F:\\TestProjects\\DMS_API\\DMS_API\\wwwroot\\Logos\\DefultLogo.png";
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                PdfPCell imageCell = new PdfPCell(jpg); imageCell.HorizontalAlignment = Element.ALIGN_CENTER;
                imageCell.Colspan = 1; Cell1.Colspan = 1; Cell2.Colspan = 1; Cell3.Colspan = 1; Cell4.Colspan = 1; Cell5.Colspan = 1;// either 1 if you need to insert one cell
                imageCell.Border = 0; Cell1.Border = 0; Cell2.Border = 0; Cell3.Border = 0; Cell4.Border = 0; Cell5.Border = 0;
                Cell1.VerticalAlignment = Element.ALIGN_TOP; Cell2.VerticalAlignment = Element.ALIGN_TOP;
                //تفاصيل اعداد الخلايا في جدول الheader
                imageCell.RunDirection = Element.ALIGN_RIGHT;
                imageCell.VerticalAlignment = Element.ALIGN_TOP;
                Cell1.RunDirection = Cell2.RunDirection = Cell3.RunDirection = Cell4.RunDirection = Cell5.RunDirection = Element.ALIGN_RIGHT;
                Cell1.VerticalAlignment = Element.ALIGN_BOTTOM; Cell1.HorizontalAlignment = Element.ALIGN_RIGHT;
                Cell2.VerticalAlignment = Element.ALIGN_BOTTOM; Cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                Cell3.VerticalAlignment = Element.ALIGN_CENTER; Cell3.HorizontalAlignment = Element.ALIGN_RIGHT;
                Cell4.VerticalAlignment = Element.ALIGN_CENTER; Cell4.HorizontalAlignment = Element.ALIGN_RIGHT;
                Cell5.VerticalAlignment = Element.ALIGN_CENTER; Cell5.HorizontalAlignment = Element.ALIGN_JUSTIFIED;

                HeaderTable.AddCell(Cell2);
                HeaderTable.AddCell(imageCell);
                HeaderTable.AddCell(Cell1);
                HeaderTable.AddCell(Cell4);
                HeaderTable.AddCell(Cell5);
                HeaderTable.AddCell(Cell3);
                //Resize image depend upon your need
                jpg.ScaleToFit(100f, 80f);
                //Give space before image
                jpg.SpacingBefore = 2f;
                //Give some space after the image
                jpg.SpacingAfter = 2f;
                jpg.Alignment = Element.ALIGN_CENTER;

                #endregion
                #region Section-instructionsAndQR
                //add second part
                Chunk c1 = new Chunk("ان حفاظك على هذه الوثيقة دون ضرر يمكنك من استخدامها في الدوائر المرتبطة بهذا النظام", fontMiddle); //FontFactory.GetFont("Times New Roman"));
                c1.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                c1.Font.SetStyle(0); c1.Font.Size = 18;
                Phrase p1 = new Phrase(); p1.Add(c1);
                pdfTable1.AddCell(p1); //cell.RunDirection = PdfPCell.ALIGN_RIGHT;
                pdfTable1.AddCell("\n");
                //add third part
                Chunk c2 = new Chunk("يمكنك حفظ صورة للوثيقة في هاتفك لأستخدامها عند الضرورة", fontMiddle);
                c2.Font.Color = new iTextSharp.text.BaseColor(Color.Black);//new iTextSharp.text.BaseColor(0, 0, 40);
                c2.Font.SetStyle(0); c2.Font.Size = 16;
                Phrase p2 = new Phrase();
                p2.Add(c2);
                //pdfTable2.AddCell(p2);
                pdfTable1.AddCell(p2);
                //add fourth part
                Chunk c3 = new Chunk("نؤيد صحة صدور الوثيقة الالكترونية بعد مطابقتها مع الوثيقة الورقية", fontRed);// FontFactory.GetFont("Times New Roman"));
                c3.Font.Color = new iTextSharp.text.BaseColor(Color.Red);
                c3.Font.SetStyle(0);    //c3.Font.Size = 11;
                Phrase p3 = new Phrase(); p3.Add(c3);
                // pdfTable3.AddCell(p3);
                pdfTable1.AddCell("\n");
                pdfTable1.AddCell(p3);
                //add line 
                pdfLineTable.AddCell("");//line  
                ///*******Read image stream***************/
                QRCodeGenerator QrGenerator = new QRCodeGenerator(); byte[] bytes;
                string strToQR = "Shaymaa1977cssadasdaasddsfdsfsdafasdfsdfasdfsadafsdfasdfasdfd";
                int len = strToQR.Length;
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(strToQR, QRCodeGenerator.ECCLevel.H);//(QRLookup_M.QrId.ToString(), QRCodeGenerator.ECCLevel.H);
                QRCoder.QRCode QrCode = new QRCoder.QRCode(QrCodeInfo);
                Bitmap QrBitmap = QrCode.GetGraphic(60);
                using (MemoryStream memory = new MemoryStream())
                {
                    //using (FileStream fs = new FileStream(imageURL, FileMode.Create, FileAccess.ReadWrite))
                    //{
                    QrBitmap.Save(memory, ImageFormat.Png);
                    bytes = memory.ToArray();
                    //fs.Write(bytes, 0, bytes.Length);
                    //}
                }
                iTextSharp.text.Image QRjpg = iTextSharp.text.Image.GetInstance(bytes);
                PdfPCell QRimageCell = new PdfPCell(jpg);
                QRimageCell.Colspan = 1;
                QRimageCell.Border = 0;
                QRimageCell.RunDirection = Element.ALIGN_CENTER;
                QRimageCell.VerticalAlignment = Element.ALIGN_TOP;
                QRjpg.ScaleToFit(140f, 120f);
                //QRjpg.Width = 200;
                //Give space before image
                QRjpg.SpacingBefore = 2f;
                //Give some space after the image
                QRjpg.SpacingAfter = 2f;
                QRjpg.Alignment = Element.ALIGN_CENTER;
                QRTable.AddCell(QRimageCell);
                QRTable.AddCell("\n");
                Chunk cc = new Chunk("QRimageCellqwwqwqwfhgghjhgjqwqwq", font); //FontFactory.GetFont("Times New Roman"));
                cc.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                cc.Font.SetStyle(0); cc.Font.Size = 14;
                Phrase pp = new Phrase(); pp.Add(cc);
                PdfPCell ppCell = new PdfPCell();
                ppCell.AddElement(pp);
                ppCell.VerticalAlignment = Element.ALIGN_CENTER;// CellCitizen.HorizontalAlignment = Element.ALIGN_CENTER;
                ppCell.Colspan = 1; ppCell.Border = 0;

                QRTable.AddCell(ppCell); //cell.RunDirection 
               
                ///**************************************/
                #endregion
                #region Section-Infos
                //add fifth part
                Chunk chCitizen = new Chunk("عزيزي المواطن في حالة حدوث أي تلكؤ أو مشكلة في قراءة رمز الوصول السريع يرجى الاتصال على الرقم المجاني 5599", fontblue);
                chCitizen.Font.Color = new iTextSharp.text.BaseColor(Color.Blue);
                chCitizen.Font.SetStyle(0);
                Paragraph paraCitizen = new Paragraph(chCitizen); paraCitizen.Alignment = Element.ALIGN_CENTER;
                //var para = new Paragraph(webAddress);
                PdfPCell CellCitizen = new PdfPCell();
                CellCitizen.AddElement(paraCitizen);
                CellCitizen.VerticalAlignment = Element.ALIGN_CENTER;// CellCitizen.HorizontalAlignment = Element.ALIGN_CENTER;
                CellCitizen.Colspan = 1; CellCitizen.Border = 0;

                Chunk chInfo = new Chunk("لمزيد من المعلومات عن الخدمات الحكومية الالكترونية، بالأمكان زيارة الرابط التالي ", fontblue);
                chInfo.Font.Color = new iTextSharp.text.BaseColor(Color.Blue);
                chInfo.Font.SetStyle(0);
                Paragraph ParaInfo = new Paragraph(chInfo); ParaInfo.Alignment = Element.ALIGN_CENTER;

                PdfPCell CellInfo = new PdfPCell();
                CellInfo.AddElement(ParaInfo);
                CellInfo.VerticalAlignment = Element.ALIGN_CENTER;// CellCitizen.HorizontalAlignment = Element.ALIGN_CENTER;
                CellInfo.Colspan = 1; CellInfo.Border = 0;

                pdfTable2.AddCell(CellCitizen);
                pdfTable2.AddCell("\n");
                pdfTable2.AddCell(CellInfo);

                Chunk chUrl = new Chunk("https://ur.gov.iq ", font);
                Paragraph paraUrl = new Paragraph(chUrl); paraUrl.Alignment = Element.ALIGN_CENTER;
                PdfPCell CellUrURLL = new PdfPCell(); CellUrURLL.AddElement(paraUrl);
                CellUrURLL.VerticalAlignment = Element.ALIGN_CENTER;
                CellInfo.Colspan = 1; CellInfo.Border = 0; CellUrURLL.Colspan = 1; CellUrURLL.Border = 0;
                pdfTable2.AddCell(CellUrURLL);

                DateTime d = DateTime.Now;
                string now = d.ToString("dd/MM/yyyy   HH:mm:ss");//d.ToString("HH:mm:ss")
                Paragraph paradate = new Paragraph(now); paradate.Alignment = Element.ALIGN_LEFT; paradate.Font.Size = 10;
                PdfPCell Celldate = new PdfPCell(); Celldate.AddElement(paradate);
                Celldate.VerticalAlignment = Element.ALIGN_LEFT;
                Celldate.Colspan = 1; Celldate.Border = 0;
                Chunk Chtext1 = new Chunk("نص تجريبي 1", font); //FontFactory.GetFont("Times New Roman"));
                Chtext1.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                Chtext1.Font.SetStyle(0); Chtext1.Font.Size = 10;
                Phrase Ptext1 = new Phrase(); Ptext1.Add(Chtext1);
                Chunk Chtext2 = new Chunk("نص تجريبي 2", font); //FontFactory.GetFont("Times New Roman"));
                Chtext2.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                Chtext2.Font.SetStyle(0); Chtext2.Font.Size = 10;
                Phrase Ptext2 = new Phrase(); Ptext2.Add(Chtext2);
                Chunk Chtext3 = new Chunk("نص تجريبي 2", font); //FontFactory.GetFont("Times New Roman"));
                Chtext3.Font.Color = new iTextSharp.text.BaseColor(Color.Black);
                Chtext3.Font.SetStyle(0); Chtext3.Font.Size = 10;
                Phrase Ptext3 = new Phrase(); Ptext3.Add(Chtext3);
                PdfPCell Celltext1 = new PdfPCell(); Celltext1.AddElement(Ptext1); Celltext1.RunDirection = PdfPCell.ALIGN_LEFT; Celltext1.Border = 0;
                PdfPCell Celltext2 = new PdfPCell(); Celltext2.AddElement(Ptext2); Celltext2.RunDirection = PdfPCell.ALIGN_LEFT; Celltext2.Border = 0;
                PdfPCell Celltext3 = new PdfPCell(); Celltext3.AddElement(Ptext3); Celltext3.RunDirection = PdfPCell.ALIGN_LEFT; Celltext3.Border = 0;
                FooterTable.AddCell(Celldate);
                FooterTable.AddCell(Celltext1); //cell.RunDirection = PdfPCell.ALIGN_RIGHT;
                FooterTable.AddCell(Celltext2);
                FooterTable.AddCell(Celltext3);
                #endregion
                #region Section-BuildPdfFile
                string folderPath = "F:\\PDF\\";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                //File Name
                int fileCount = Directory.GetFiles(folderPath).Length;
                string strFileName = "DescriptionForm" + (fileCount + 1) + ".pdf";
                //هنا اضافة الاجزاء الى ملف ال pdf بالتسلسل

                using (FileStream stream = new FileStream(folderPath + strFileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    pdfDoc.Add(HeaderTable);

                    pdfLineTable.SpacingBefore = 20f;
                    pdfDoc.Add(pdfLineTable);
                    pdfTable1.SpacingBefore = 20f;
                    pdfDoc.Add(pdfTable1);
                    pdfTable1.SpacingAfter = 20f;

                    QRjpg.ScalePercent(10f);
                    pdfDoc.Add(QRjpg);

                    pdfTable2.SpacingBefore = 20f;
                    pdfDoc.Add(pdfTable2);
                    pdfTable2.SpacingAfter = 20f;

                    FooterTable.SpacingBefore = 20f;
                    pdfDoc.Add(FooterTable);
                    FooterTable.SpacingAfter = 20f;

                    pdfDoc.NewPage();

                    pdfDoc.Close();
                    stream.Close();
                }
                #endregion 
                #region Display PDF
                System.Diagnostics.Process.Start(folderPath + "\\" + strFileName);
                #endregion
            }
            catch (Exception ex)
            {

            }
        }

        #endregion
    }
}