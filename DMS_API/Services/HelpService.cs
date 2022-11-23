using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Reflection;
namespace DMS_API.Services
{
    public static class HelpService
    {
        #region Properteis
        private static DataAccessService dam = new DataAccessService(SecurityService.ConnectionString);
        public enum ParentClass
        {
            Group = 2,
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
                        OrgChild = await HelpService.GetOrgsChilds(Convert.ToInt32(dt.Rows[0]["OrgId"].ToString()), IsOrgAdmin)
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
        private static async Task<List<OrgModel>> GetOrgsChilds(int OrgId, bool IsOrgAdmin)
        {
            try
            {
                string getOrgInfo = $"SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName, OrgIsActive   FROM [User].[GetOrgsChildsByParentId]({OrgId}) AND OrgIsActive=1"; // WHERE AND OrgIsActive='{JustActive}'
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
                            OrgChild = await GetOrgsChilds(Convert.ToInt32(dtChild.Rows[i]["OrgId"].ToString()), IsOrgAdmin)
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
                //int OrgOwnerID = Convert.ToInt32(dam.FireSQL($"SELECT OrgOwner FROM [User].V_Users WHERE UserID = {userLoginID} "));
                //string whereField = OrgOwnerID == 0 ? "OrgUp" : "OrgId";
                string getOrgInfo = $"SELECT OrgId, OrgUp, OrgLevel, OrgArName, OrgEnName, OrgKuName , OrgArNameUp, OrgEnNameUp, OrgKuNameUp, OrgIsActive  FROM [User].[GetOrgsbyUserIdTable]({userLoginID}) ORDER BY OrgId "; // WHERE {whereField} !={OrgOwnerID}
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
                            OrgIsActive = bool.Parse(dt.Rows[0]["OrgIsActive"].ToString())
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
        #endregion
    }
}