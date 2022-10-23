using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System;
using System.Collections.Generic;
using System.Data;

namespace DMS_API.Services
{
    public class TranslationService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private DataTable dt { get; set; }
        private TranslationModel Translation_M { get; set; }
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
        public async Task<ResponseModelView> GetTranslationList(RequestPaginationModelView Pagination_MV, string Lang)
        {
            try
            {
                string Mlang = GetMessageLanguages(Lang);
                //var msgA = MessageService.MsgDictionary["Ar"]["InvalidUsername"];
                int _PageNumber = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                int _PageRows = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;

                int CurrentPage = _PageNumber;
                //int TotalRows = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(*) AS TotalRows FROM Main.Translation"));
                //int MaxPage = Convert.ToInt32(dam.FireSQL($"SELECT CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage FROM Main.Translation"));

                var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage FROM Main.Translation");

                string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation ORDER BY TrId " +
                             $"OFFSET ({_PageNumber}-1)*{_PageRows} ROWS " +
                             $"FETCH NEXT {_PageRows} ROWS ONLY ";
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
                            TrKey = dt.Rows[i]["TrKey"].ToString(),
                            TrArName = dt.Rows[i]["TrArName"].ToString(),
                            TrEnName = dt.Rows[i]["TrEnName"].ToString(),
                            TrKrName = dt.Rows[i]["TrKrName"].ToString()
                        };
                        Translation_Mlist.Add(Translation_M);
                    }

                    response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = Mlang,// dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
                        Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Translation_Mlist }
                    };
                    return response_MV;
                }
                else
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = Mlang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
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

        public async Task<ResponseModelView> GetTranslationByID(int id, string Lang)
        {
            try
            {

                string Mlang = GetMessageLanguages(Lang);

                string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation WHERE Trid={id}";
                dt = new DataTable();
                dt = await Task.Run(() => dam.FireDataTable(get));
                if (dt.Rows.Count > 0)
                {
                    Translation_M = new TranslationModel
                    {
                        Trid = Convert.ToInt32(dt.Rows[0]["Trid"].ToString()),
                        TrKey = dt.Rows[0]["TrKey"].ToString(),
                        TrArName = dt.Rows[0]["TrArName"].ToString(),
                        TrEnName = dt.Rows[0]["TrEnName"].ToString(),
                        TrKrName = dt.Rows[0]["TrKrName"].ToString()
                    };

                    response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = Mlang,// dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
                        Data = Translation_M
                    };
                    return response_MV;
                }
                else
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = Mlang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
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

        public async Task<ResponseModelView> AddTranslationWords(TranslationModel Translation_M, string Lang)
        {
            try
            {
                string Mlang = GetMessageLanguages(Lang);

                int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(TrKey) FROM Main.Translation WHERE TrKey = '{Translation_M.TrKey}' "));
                if (checkDeblicate == 0)
                {
                    string insert = "INSERT INTO Main.Translation (TrKey, TrArName, TrEnName, TrKrName) VALUES(@TrKey, @TrArName, @TrEnName, @TrKrName) ";
                    dam.DoQuery(insert, Translation_M.TrKey, Translation_M.TrArName, Translation_M.TrEnName, Translation_M.TrKrName);

                    string get = "SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation";
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
                                TrKey = dt.Rows[0]["TrKey"].ToString(),
                                TrArName = dt.Rows[i]["TrArName"].ToString(),
                                TrEnName = dt.Rows[i]["TrEnName"].ToString(),
                                TrKrName = dt.Rows[i]["TrKrName"].ToString()
                            };
                            Translation_Mlist.Add(Translation_M);
                        }

                        response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = Mlang,// dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
                            Data = Translation_Mlist
                        };
                        return response_MV;
                    }
                    else
                    {
                        response_MV = new ResponseModelView
                        {

                            Success = false,
                            Message = Mlang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
                            Data = new List<object>()
                        };
                        return response_MV;
                    }
                }
                else
                {
                    response_MV = new ResponseModelView
                    {
                        // الحقل متكرر
                        Success = false,
                        Message = Mlang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
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

        public async Task<ResponseModelView> EditTranslationWords(TranslationModel Translation_M, string Lang)
        {
            try
            {
                string Mlang = GetMessageLanguages(Lang);
                int check = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(Trid) FROM Main.Translation WHERE Trid={Translation_M.Trid}"));
                if (check > 0)
                {
                    string update = $"UPDATE Main.Translation SET TrArName='{Translation_M.TrArName}', TrEnName='{Translation_M.TrEnName}', TrKrName='{Translation_M.TrKrName}'  WHERE Trid={Translation_M.Trid} ";
                    var gg = await Task.Run(() => dam.DoQuery(update));

                    response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = Mlang,// dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
                        Data = new List<object>()
                    };
                    return response_MV;
                }
                else
                {
                    // not found id for this record
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = Mlang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
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

        public async Task<ResponseModelView> SearchTranslationWords(RequestSearchTranslationModelView SearchTranslation_MV, string Lang)
        {
            try
            {
                string Mlang = GetMessageLanguages(Lang);
                string ColumnSearch = GetTranslationSearchColumn(SearchTranslation_MV.KeySearch);
                //var msgA = MessageService.MsgDictionary["Ar"]["InvalidUsername"];
                int _PageNumber = SearchTranslation_MV.PageNumber == 0 ? 1 : SearchTranslation_MV.PageNumber;
                int _PageRows = SearchTranslation_MV.PageRows == 0 ? 1 : SearchTranslation_MV.PageRows;
                int CurrentPage = _PageNumber;
                var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                 $"FROM Main.Translation WHERE {ColumnSearch} = '{SearchTranslation_MV.WordSearch}' ");

                string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation " +
                             $"WHERE {ColumnSearch} = '{SearchTranslation_MV.WordSearch}' ORDER BY TrId " +
                             $"OFFSET ({_PageNumber}-1)*{_PageRows} ROWS " +
                             $"FETCH NEXT {_PageRows} ROWS ONLY ";
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
                            TrKey = dt.Rows[i]["TrKey"].ToString(),
                            TrArName = dt.Rows[i]["TrArName"].ToString(),
                            TrEnName = dt.Rows[i]["TrEnName"].ToString(),
                            TrKrName = dt.Rows[i]["TrKrName"].ToString()
                        };
                        Translation_Mlist.Add(Translation_M);
                    }

                    response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = Mlang,// dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
                        Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Translation_Mlist }
                    };
                    return response_MV;
                }
                else
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = Mlang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
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

        public async Task<ResponseModelView> GetTranslationPage(string Lang)
        {
            try
            {
                string Mlang = GetTranslationLanguages(Lang);

                string get = $"SELECT DISTINCT  TrKey, {Mlang} AS 'Word' FROM Main.Translation";
                dt = new DataTable();
                dt = await Task.Run(() => dam.FireDataTable(get));
                Dictionary<string, string> dict1 = new();
                //dict[Lang] = Lang;
                // List<TranslationPageModel> Translation_Mlist = new List<TranslationModel> ();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        dict1.Add(dt.Rows[i]["TrKey"].ToString(), dt.Rows[i]["Word"].ToString());

                    }

                    Dictionary<string, Dictionary<string, string>> dict2 = new();
                    dict2.Add("Trans", dict1);

                    response_MV = new ResponseModelView
                    {
                        Success = true,
                        Message = Lang,// dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
                        Data = dict2
                    };
                    return response_MV;
                }
                else
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = Lang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
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

        #region MyRegion
        //public async Task<ResponseModelView> EditTranslationWords1(TranslationModel Translation_M, string Lang)
        //{
        //    try
        //    {
        //        string Mlang = GetMessageLanguages(Lang);
        //        int check = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(Trid) FROM Main.Translation WHERE Trid={Translation_M.Trid}"));
        //        if (check > 0)
        //        {
        //            string update = $"UPDATE Main.Translation SET TrArName='{Translation_M.TrArName}', TrEnName='{Translation_M.TrEnName}', TrKrName='{Translation_M.TrKrName}'  WHERE Trid={Translation_M.Trid} ";
        //            var gg = dam.DoQuery(update);


        //            string get = "SELECT Trid, TrArName, TrEnName, TrKrName FROM Main.Translation";
        //            dt = new DataTable();
        //            dt = await Task.Run(() => dam.FireDataTable(get));
        //            Translation_Mlist = new List<TranslationModel>();
        //            if (dt.Rows.Count > 0)
        //            {
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    Translation_M = new TranslationModel
        //                    {
        //                        Trid = Convert.ToInt32(dt.Rows[i]["Trid"].ToString()),
        //                        TrArName = dt.Rows[i]["TrArName"].ToString(),
        //                        TrEnName = dt.Rows[i]["TrEnName"].ToString(),
        //                        TrKrName = dt.Rows[i]["TrKrName"].ToString()
        //                    };
        //                    Translation_Mlist.Add(Translation_M);
        //                }

        //                response_MV = new ResponseModelView
        //                {
        //                    Success = true,
        //                    Message = Mlang,// dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
        //                    Data = Translation_Mlist
        //                };
        //                return response_MV;
        //            }
        //            else
        //            {
        //                response_MV = new ResponseModelView
        //                {
        //                    Success = false,
        //                    Message = Mlang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
        //                    Data = new List<object>()
        //                };
        //                return response_MV;
        //            }
        //        }
        //        else
        //        {
        //            // not found id for this record
        //            response_MV = new ResponseModelView
        //            {
        //                Success = false,
        //                Message = Mlang,//dam.FireSQL($"SELECT {Mlang} FROM Main.Messages WHERE MesEnName= 'english' "),
        //                Data = new List<object>()
        //            };
        //            return response_MV;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response_MV = new ResponseModelView
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //            Data = new List<object>()
        //        };
        //        return response_MV;
        //    }
        //}
        #endregion


        #endregion

        #region Methods
        private string GetMessageLanguages(string Lang)
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
        private string GetTranslationLanguages(string Lang)
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
        private string GetTranslationSearchColumn(string Lang)
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
        #endregion
    }
}
