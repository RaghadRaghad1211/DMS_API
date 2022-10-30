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
        private DataTable Dt { get; set; }
        private TranslationModel Translation_M { get; set; }
        private List<TranslationModel> Translation_Mlist { get; set; }
        private SessionService Session_S { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor        
        public TranslationService()
        {
            dam = new DataAccessService(SecurityService.ConnectionString);
        }
        #endregion

        #region CURD Functions        
        public async Task<ResponseModelView> GetTranslationList(PaginationModelView Pagination_MV, string Token, string Lang)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(Token, Lang);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
                {
                    string ColLang = HelpService.GetMessageColumn(Lang);
                    int _PageNumber = Pagination_MV.PageNumber == 0 ? 1 : Pagination_MV.PageNumber;
                    int _PageRows = Pagination_MV.PageRows == 0 ? 1 : Pagination_MV.PageRows;
                    int CurrentPage = _PageNumber;
                    var MaxTotal = dam.FireDataTable($"SELECT COUNT(*) AS TotalRows, CEILING(COUNT(*) / CAST({_PageRows} AS FLOAT)) AS MaxPage FROM Main.Translation");
                    if (MaxTotal == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetFaild],
                            Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
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
                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NoData],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation ORDER BY TrId " +
                                                         $"OFFSET ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                         $"FETCH NEXT {_PageRows} ROWS ONLY ";
                            Dt = new DataTable();
                            Dt = await Task.Run(() => dam.FireDataTable(get));
                            if (Dt == null)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError],
                                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                                };
                                return Response_MV;
                            }
                            Translation_Mlist = new List<TranslationModel>();
                            if (Dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < Dt.Rows.Count; i++)
                                {
                                    Translation_M = new TranslationModel
                                    {
                                        Trid = Convert.ToInt32(Dt.Rows[i]["Trid"].ToString()),
                                        TrKey = Dt.Rows[i]["TrKey"].ToString(),
                                        TrArName = Dt.Rows[i]["TrArName"].ToString(),
                                        TrEnName = Dt.Rows[i]["TrEnName"].ToString(),
                                        TrKrName = Dt.Rows[i]["TrKrName"].ToString()
                                    };
                                    Translation_Mlist.Add(Translation_M);
                                }

                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Translation_Mlist }
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NoData],
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
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }

        public async Task<ResponseModelView> GetTranslationByID(int id, string Token, string Lang)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(Token, Lang);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
                {
                    string Mlang = HelpService.GetMessageColumn(Lang);
                    string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation WHERE Trid={id}";
                    Dt = new DataTable();
                    Dt = await Task.Run(() => dam.FireDataTable(get));
                    if (Dt == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError],
                            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                        };
                        return Response_MV;
                    }
                    if (Dt.Rows.Count > 0)
                    {
                        Translation_M = new TranslationModel
                        {
                            Trid = Convert.ToInt32(Dt.Rows[0]["Trid"].ToString()),
                            TrKey = Dt.Rows[0]["TrKey"].ToString(),
                            TrArName = Dt.Rows[0]["TrArName"].ToString(),
                            TrEnName = Dt.Rows[0]["TrEnName"].ToString(),
                            TrKrName = Dt.Rows[0]["TrKrName"].ToString()
                        };

                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                            Data = Translation_M
                        };
                        return Response_MV;
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NoData],
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
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }

        public async Task<ResponseModelView> AddTranslationWords(TranslationModel Translation_M, string Token, string Lang)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(Token, Lang);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
                {
                    if (ValidationService.IsEmpty(Translation_M.TrKey) == true || ValidationService.IsEmpty(Translation_M.TrArName) == true || ValidationService.IsEmpty(Translation_M.TrEnName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.MustFillInformation],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
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
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.InsertFaild],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.InsertSuccess],
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
                                Message = Translation_M.TrKey.ToString() + " " + MessageService.MsgDictionary[Lang.ToLower()][MessageService.IsExist],
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
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }

        public async Task<ResponseModelView> EditTranslationWords(TranslationModel Translation_M, string Token, string Lang)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(Token, Lang);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
                {
                    if (ValidationService.IsEmpty(Translation_M.TrArName) == true || ValidationService.IsEmpty(Translation_M.TrEnName) == true)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.MustFillInformation],
                            Data = new HttpResponseMessage(HttpStatusCode.BadRequest).StatusCode
                        };
                        return Response_MV;
                    }
                    else
                    {
                        string Mlang = HelpService.GetMessageColumn(Lang);
                        int check = Convert.ToInt32(dam.FireSQL($"SELECT COUNT(Trid) FROM Main.Translation WHERE Trid={Translation_M.Trid}"));
                        if (check > 0)
                        {
                            string update = $"UPDATE Main.Translation SET TrArName='{Translation_M.TrArName}', TrEnName='{Translation_M.TrEnName}', TrKrName='{Translation_M.TrKrName}'  WHERE Trid={Translation_M.Trid} ";
                            await Task.Run(() => dam.DoQuery(update));

                            Response_MV = new ResponseModelView
                            {
                                Success = true,
                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.UpdateSuccess],
                                Data = new HttpResponseMessage(HttpStatusCode.OK).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            // not found id for this record
                            Response_MV = new ResponseModelView
                            {
                                Success = false,
                                Message = Translation_M.Trid.ToString() + " " + MessageService.MsgDictionary[Lang.ToLower()][MessageService.IsNotExist],
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
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }

        public async Task<ResponseModelView> SearchTranslationWords(SearchTranslationModelView SearchTranslation_MV, string Token, string Lang)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(Token, Lang);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
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
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError],
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
                                Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NoData],
                                Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                            };
                            return Response_MV;
                        }
                        else
                        {
                            string get = $"SELECT Trid, TrKey, TrArName, TrEnName, TrKrName FROM Main.Translation " +
                                                         $"WHERE {ColumnSearch} = '{SearchTranslation_MV.WordSearch}' ORDER BY TrId " +
                                                         $"OFFSET ({_PageNumber}-1)*{_PageRows} ROWS " +
                                                         $"FETCH NEXT {_PageRows} ROWS ONLY ";
                            Dt = new DataTable();
                            Dt = await Task.Run(() => dam.FireDataTable(get));
                            if (Dt == null)
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetFaild],
                                    Data = new HttpResponseMessage(HttpStatusCode.NotFound).StatusCode
                                };
                                return Response_MV;
                            }
                            Translation_Mlist = new List<TranslationModel>();
                            if (Dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < Dt.Rows.Count; i++)
                                {
                                    Translation_M = new TranslationModel
                                    {
                                        Trid = Convert.ToInt32(Dt.Rows[i]["Trid"].ToString()),
                                        TrKey = Dt.Rows[i]["TrKey"].ToString(),
                                        TrArName = Dt.Rows[i]["TrArName"].ToString(),
                                        TrEnName = Dt.Rows[i]["TrEnName"].ToString(),
                                        TrKrName = Dt.Rows[i]["TrKrName"].ToString()
                                    };
                                    Translation_Mlist.Add(Translation_M);
                                }
                                Response_MV = new ResponseModelView
                                {
                                    Success = true,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                                    Data = new { TotalRows = MaxTotal.Rows[0]["TotalRows"], MaxPage = MaxTotal.Rows[0]["MaxPage"], CurrentPage, data = Translation_Mlist }
                                };
                                return Response_MV;
                            }
                            else
                            {
                                Response_MV = new ResponseModelView
                                {
                                    Success = false,
                                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NoData],
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
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }

        public async Task<ResponseModelView> GetTranslationPage(string Token, string Lang)
        {
            try
            {
                Session_S = new SessionService();
                var ResponseSession = await Session_S.CheckAuthorizationResponse(Token, Lang);
                if (ResponseSession.Success == false)
                {
                    return ResponseSession;
                }
                else
                {
                    string Mlang = HelpService.GetTranslationColumn(Lang);
                    string get = $"SELECT DISTINCT  TrKey, {Mlang} AS 'Word' FROM Main.Translation";
                    Dt = new DataTable();
                    Dt = await Task.Run(() => dam.FireDataTable(get));
                    if (Dt == null)
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError],
                            Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                        };
                        return Response_MV;
                    }
                    Dictionary<string, string> dict1 = new();
                    if (Dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < Dt.Rows.Count; i++)
                        {
                            dict1.Add(Dt.Rows[i]["TrKey"].ToString(), Dt.Rows[i]["Word"].ToString());
                        }
                        //Dictionary<string, Dictionary<string, string>> dict2 = new();
                        //dict2.Add("Trans", dict1);
                        Dictionary<string, Dictionary<string, string>> dict2 = new()
                    {
                        { "Trans", dict1 }
                    };

                        Response_MV = new ResponseModelView
                        {
                            Success = true,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.GetSuccess],
                            Data = dict2
                        };
                        return Response_MV;
                    }
                    else
                    {
                        Response_MV = new ResponseModelView
                        {
                            Success = false,
                            Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.NoData],
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
                    Message = MessageService.MsgDictionary[Lang.ToLower()][MessageService.ExceptionError] + " - " + ex.Message,
                    Data = new HttpResponseMessage(HttpStatusCode.ExpectationFailed).StatusCode
                };
                return Response_MV;
            }
        }
        #endregion
    }
}