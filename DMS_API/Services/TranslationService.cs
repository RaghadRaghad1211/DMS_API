using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;
using System.Net;

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
        public async Task<ResponseModelView> GetTranslationList(PaginationModelView Pagination_MV, string Lang)
        {
            try
            {
                string ColLang = HelpService.GetMessageColumn(Lang);
                int _PageNumber = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                int _PageRows = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
                int CurrentPage = _PageNumber;
                var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage FROM Main.Translation");
                if (MaxTotal == null)
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"],
                        Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                    };
                    return response_MV;
                }
                else
                {
                    if (MaxTotal.Rows.Count == 0)
                    {
                        response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()]["NoData"],
                            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                        };
                        return response_MV;
                    }
                    else
                    {
                        string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation ORDER BY TrId " +
                                                     $"OFFSET ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                     $"FETCH NEXT {_PageRows} ROWS ONLY ";
                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(get));
                        if (dt == null)
                        {
                            response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return response_MV;
                        }
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
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["GetSuccess"],
                                Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Translation_Mlist }
                            };
                            return response_MV;
                        }
                        else
                        {
                            response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["NoData"],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return response_MV;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                };
                return response_MV;
            }
        }

        public async Task<ResponseModelView> GetTranslationByID(int id, string Lang)
        {
            try
            {
                string Mlang = HelpService.GetMessageColumn(Lang);

                string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation WHERE Trid={id}";
                dt = new DataTable();
                dt = await Task.Run(() => dam.FireDataTable(get));
                if (dt == null)
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"],
                        Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                    };
                    return response_MV;
                }
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
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["GetSuccess"],
                        Data = Translation_M
                    };
                    return response_MV;
                }
                else
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["NoData"],
                        Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                    };
                    return response_MV;
                }
            }
            catch (Exception ex)
            {
                response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                };
                return response_MV;
            }
        }

        public async Task<ResponseModelView> AddTranslationWords(TranslationModel Translation_M, string Lang)
        {
            try
            {
                if (ValidationService.IsEmpty(Translation_M.TrKey) == true || ValidationService.IsEmpty(Translation_M.TrArName) == true || ValidationService.IsEmpty(Translation_M.TrEnName) == true)
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["MustFillInformation"],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return response_MV;
                }
                else
                {
                    string Mlang = HelpService.GetMessageColumn(Lang);
                    int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(TrKey) FROM Main.Translation WHERE TrKey = '{Translation_M.TrKey}' "));
                    if (checkDeblicate == 0)
                    {
                        string insert = "INSERT INTO Main.Translation (TrKey, TrArName, TrEnName, TrKrName) OUTPUT INSERTED.TrId VALUES(@TrKey, @TrArName, @TrEnName, @TrKrName) ";
                        string outValue = await Task.Run(() => dam.DoQueryAndPutOutValue(insert, "TrId", Translation_M.TrKey, Translation_M.TrArName, Translation_M.TrEnName, Translation_M.TrKrName));
                        if (outValue == null || outValue.Trim() == "")
                        {
                            response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["InsertFaild"],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return response_MV;
                        }
                        else
                        {
                            response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["InsertSuccess"],
                                Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                            };
                            return response_MV;
                        }
                    }
                    else
                    {
                        response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = Translation_M.TrKey.ToString() + " " + MessageService.MsgDictionary[Lang.ToLower()]["IsExist"],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return response_MV;
                    }
                }
            }
            catch (Exception ex)
            {
                response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()]["InsertFaild"] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                };
                return response_MV;
            }
        }

        public async Task<ResponseModelView> EditTranslationWords(TranslationModel Translation_M, string Lang)
        {
            try
            {
                if (ValidationService.IsEmpty(Translation_M.TrArName) == true || ValidationService.IsEmpty(Translation_M.TrEnName) == true)
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["MustFillInformation"],
                        Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                    };
                    return response_MV;
                }
                else
                {
                    string Mlang = HelpService.GetMessageColumn(Lang);
                    int check = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(Trid) FROM Main.Translation WHERE Trid={Translation_M.Trid}"));
                    if (check > 0)
                    {
                        string update = $"UPDATE Main.Translation SET TrArName='{Translation_M.TrArName}', TrEnName='{Translation_M.TrEnName}', TrKrName='{Translation_M.TrKrName}'  WHERE Trid={Translation_M.Trid} ";
                        await Task.Run(() => dam.DoQuery(update));

                        response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[Lang.ToLower()]["UpdateSuccess"],
                            Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                        };
                        return response_MV;
                    }
                    else
                    {
                        // not found id for this record
                        response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = Translation_M.Trid.ToString() + " " + MessageService.MsgDictionary[Lang.ToLower()]["IsNotExist"],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return response_MV;
                    }
                }


            }
            catch (Exception ex)
            {
                response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()]["UpdateFaild"] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                };
                return response_MV;
            }
        }

        public async Task<ResponseModelView> SearchTranslationWords(SearchTranslationModelView SearchTranslation_MV, string Lang)
        {
            try
            {
                string Mlang = HelpService.GetMessageColumn(Lang);
                string ColumnSearch = HelpService.GetTranslationSearchColumn(SearchTranslation_MV.KeySearch);
                int _PageNumber = SearchTranslation_MV.PageNumber == 0 ? 1 : SearchTranslation_MV.PageNumber;
                int _PageRows = SearchTranslation_MV.PageRows == 0 ? 1 : SearchTranslation_MV.PageRows;
                int CurrentPage = _PageNumber;
                var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage " +
                                                 $"FROM Main.Translation WHERE {ColumnSearch} = '{SearchTranslation_MV.WordSearch}' ");
                if (MaxTotal == null)
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"],
                        Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                    };
                    return response_MV;
                }
                else
                {
                    if (MaxTotal.Rows.Count == 0)
                    {
                        response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()]["NoData"],
                            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                        };
                        return response_MV;
                    }
                    else
                    {
                        string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation " +
                                                     $"WHERE {ColumnSearch} = '{SearchTranslation_MV.WordSearch}' ORDER BY TrId " +
                                                     $"OFFSET ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                     $"FETCH NEXT {_PageRows} ROWS ONLY ";
                        dt = new DataTable();
                        dt = await Task.Run(() => dam.FireDataTable(get));
                        if (dt == null)
                        {
                            response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return response_MV;
                        }
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
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["GetSuccess"],
                                Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Translation_Mlist }
                            };
                            return response_MV;
                        }
                        else
                        {
                            response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = MessageService.MsgDictionary[Lang.ToLower()]["NoData"],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return response_MV;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                };
                return response_MV;
            }
        }

        public async Task<ResponseModelView> GetTranslationPage(string Lang)
        {
            try
            {
                string Mlang = HelpService.GetTranslationColumn(Lang);

                string get = $"SELECT DISTINCT  TrKey, {Mlang} AS 'Word' FROM Main.Translation";
                dt = new DataTable();
                dt = await Task.Run(() => dam.FireDataTable(get));
                if (dt == null)
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"],
                        Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                    };
                    return response_MV;
                }
                Dictionary<string, string> dict1 = new();
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
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["GetSuccess"],
                        Data = dict2
                    };
                    return response_MV;
                }
                else
                {
                    response_MV = new ResponseModelView
                    {
                        Success = false,
                        Message = MessageService.MsgDictionary[Lang.ToLower()]["NoData"],
                        Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                    };
                    return response_MV;
                }
            }
            catch (Exception ex)
            {
                response_MV = new ResponseModelView
                {
                    Success = false,
                    Message = MessageService.MsgDictionary[Lang.ToLower()]["GetFaild"] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
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

        //public async Task<ResponseModelView> AddTranslationWords1(TranslationModel Translation_M, string Lang)
        //{
        //    try
        //    {
        //        string Mlang = HelpService.GetMessageColumn(Lang);

        //        int checkDeblicate = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(TrKey) FROM Main.Translation WHERE TrKey = '{Translation_M.TrKey}' "));
        //        if (checkDeblicate == 0)
        //        {
        //            string insert = "INSERT INTO Main.Translation (TrKey, TrArName, TrEnName, TrKrName) OUTPUT INSERTED.TrId VALUES(@TrKey, @TrArName, @TrEnName, @TrKrName) ";
        //            string outValue = dam.DoQueryAndPutOutValue(insert, Translation_M.TrKey, Translation_M.TrArName, Translation_M.TrEnName, Translation_M.TrKrName);
        //            if (outValue == null)
        //            {
        //                response_MV = new ResponseModelView
        //                {
        //                    Success = false,
        //                    Message = MessageService.MsgDictionary[Lang.ToLower()]["InsertFaild"],
        //                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
        //                };
        //                return response_MV;
        //            }
        //            string get = "SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation";
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
        //                        TrKey = dt.Rows[0]["TrKey"].ToString(),
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
        //            response_MV = new ResponseModelView
        //            {
        //                // الحقل متكرر
        //                Success = false,
        //                Message = Translation_M.TrKey.ToString() + " " + MessageService.MsgDictionary[Lang.ToLower()]["IsExist"],
        //                Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
        //            };
        //            return response_MV;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        response_MV = new ResponseModelView
        //        {
        //            Success = false,
        //            Message = MessageService.MsgDictionary[Lang.ToLower()]["InsertFaild"] + " - " + ex.Message,
        //            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
        //        };
        //        return response_MV;
        //    }
        //}
        #endregion


        #endregion
    }
}
