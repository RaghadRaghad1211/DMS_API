using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;

namespace DMS_API.Services
{
    public class TranslationService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private DataTable dt { get; set; }
        private TranslationModel Translation_M { get; set; }
        private TranslationModelView translation_MV { get; set; }
        private List<TranslationModel> Translation_Mlist { get; set; }
        private ResponseModelView response_MV { get; set; }

        #endregion

        #region Constructor        
        public TranslationService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region CURD Functions
        public async Task<ResponseModelView> GetTranslationList(string Lang)
        {
            try
            {
                string Tlang = "TrArName";
                switch (Lang)
                {
                    case "Kr":
                        Tlang = "TrKrName";
                        break;
                    case "En":
                        Tlang = "TrEnName";
                        break;
                    default:
                        break;
                        //case "kr":
                        //    Tlang = "TrKrName";
                        //    break;
                }

                string get = "SELECT Trid, TrArName, TrEnName, TrKrName FROM Main.Translation";
                dt = new DataTable();
                dt = await Task.Run(() => dam.FireDataTable(get));
                Translation_Mlist = new List<TranslationModel>();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Translation_M = new TranslationModel
                        {
                            Trid = Convert.ToInt32(dt.Rows[i]["Trid"].ToString()),
                            TrArName = dt.Rows[i]["TrArName"].ToString(),
                            TrEnName = dt.Rows[i]["TrEnName"].ToString(),
                            TrKrName = dt.Rows[i]["TrKrName"].ToString()
                        };
                        Translation_Mlist.Add(Translation_M);
                    }

                    response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = dam.FireSQL($"SELECT {Tlang} FROM Main.Messages WHERE TrEnName= 'english' "),
                        Data = Translation_Mlist
                    };
                    return response_MV;
                }
                else
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = dam.FireSQL($"SELECT {Tlang} FROM Main.Messages WHERE TrEnName= 'Error' "),
                        Data = new List<object>()
                    };
                    return response_MV;
                }
            }
            catch (Exception ex)
            {
                response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = ex.Message,
                    Data = new List<object>()
                };
                return response_MV;
            }
        }
        #endregion
    }
}
