using System.Data;
using System.Data.SqlClient;

namespace ArchiveAPI.Services
{
    /// <summary>
    /// Class Builded By HAEL ROX:
    /// Service work with Database Queries
    /// </summary>
    public class DataAccessService
    {
        private readonly SqlConnection CON;
        private SqlCommand CMD;
        private DataSet? DATA_SET;
        private readonly SqlDataAdapter SQL_DA = new SqlDataAdapter();


        /// <summary>
        /// Constructor to Connection With SQLserver { non-Security } ( Connection-String )
        /// </summary>
        /// <param name="server1"> Name of Server , "String Type" </param>
        /// <param name="database"> Name of DataBase , "String Type" </param>
        public DataAccessService(string server1, string database)
        {
            string[] server = new string[] { "Server=", server1, ";DataBase=", database, ";Integrated Security=True" };
            this.CON = new SqlConnection(string.Concat(server));
        }

        /// <summary>
        /// Constructor to Connection With SQLserver { Security } ( Connection-String )
        /// </summary>
        /// <param name="server1"> Name of Server , "String Type" </param>
        /// <param name="database"> Name of DataBase , "String Type" </param>
        /// <param name="userId"> UserName of SQLserver-Authentication , "String Type" </param>
        /// <param name="password"> Password of SQLserver-Authentication , "String Type" </param>
        public DataAccessService(string server1, string database, string userId, string password)
        {
            string[] server = new string[] { "Server=", server1, ";DataBase=", database, ";User ID=", userId, ";Password=", password };
            this.CON = new SqlConnection(string.Concat(server));
        }

        /// <summary>
        /// Constructor to Connection With SQLserver ( General Connection-String )
        /// </summary>
        /// <param name="connectiongStringName"> Name of Connection-String , "String Type" </param>
        public DataAccessService(string connectiongStringName)
        {
            this.CON = new SqlConnection(connectiongStringName);
        }

        /// <summary>
        /// Method to Checked Connected With SQLserver , Return Bool (True - False) .
        /// True : If There is Connection .
        /// False : If There is not Connection .
        /// </summary>
        /// <returns></returns>
        public bool Connection_Check()
        {
            if (this.CON.State == ConnectionState.Open)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method Doing Backup Of Database SQL Server , And Return True OR False .
        /// True : If Backup is Successfully .
        /// False : If Backup is Not Successfully .
        /// </summary>
        /// <param name="DatabaseName"> The Name Of DataBase , "String Type" . </param>
        /// <param name="PathOutOfBackup"> The Path You Want To Save The Backup Such As ' C:\Users\HAAL\Desktop ' , "String Type" .</param>
        /// <param name="Mark"> The Mark You Want To Add In BackupName , Default Value = "" , "String Type" . </param>
        /// <returns></returns>
        public bool Database_Backup(string DatabaseName, string PathOutOfBackup, string Mark = "")
        {
            try
            {
                if (this.CON.State != ConnectionState.Open)
                {
                    this.CON.Open();
                }
                string SqlCmd = "BACKUP DATABASE [" + DatabaseName + "] TO DISK='" + PathOutOfBackup + "\\" + DatabaseName + DateTime.Now.ToString("yyyy-MM-dd--HHmmss") + Mark.Trim() + ".bak'";
                CMD = new SqlCommand(SqlCmd, this.CON);
                CMD.ExecuteNonQuery();
                this.CON.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Method Doing Restore From Backup File To Database SQL Server , And Return True OR False .
        /// True : If Restore is Successfully .
        /// False : If Restore is Not Successfully .
        /// </summary>
        /// <param name="DatabaseName"> The Name Of DataBase , "String Type" . </param>
        /// <param name="PathInOfBackupWithExtention"> The Path Of Backup With Extention You Want To Restore Into Database Such As ' C:\Users\HAAL\Desktop\Heemo.bak ' , "String Type" .</param>
        /// <returns></returns>
        public bool Database_Restore(string DatabaseName, string PathInOfBackupWithExtention)
        {
            try
            {
                if (this.CON.State != ConnectionState.Open)
                {
                    this.CON.Open();
                }
                string SqlCmd1 = string.Format("ALTER DATABASE [" + DatabaseName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
                CMD = new SqlCommand(SqlCmd1, this.CON);
                CMD.ExecuteNonQuery();

                string SqlCmd2 = "USE MASTER RESTORE DATABASE [" + DatabaseName + "] FROM DISK='" + PathInOfBackupWithExtention + "'WITH REPLACE;";
                CMD = new SqlCommand(SqlCmd2, this.CON);
                CMD.ExecuteNonQuery();

                string SqlCmd3 = string.Format("ALTER DATABASE [" + DatabaseName + "] SET MULTI_USER");
                CMD = new SqlCommand(SqlCmd3, this.CON);
                CMD.ExecuteNonQuery();

                this.CON.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method to Close Connection String if it's Opened .
        /// </summary>
        public void Connection_State_ifOPEN()
        {
            if (this.CON.State == ConnectionState.Open)
            {
                this.CON.Close();
            }
        }

        /// <summary>
        /// Method to Open Connection String if it's Closed .
        /// </summary>
        public void Connection_State_ifCLOSE()
        {
            if (this.CON.State == ConnectionState.Closed)
            {
                this.CON.Open();
            }
        }

        
        public bool CheckConnectionNetwork()
        {
            try
            {
                if (this.CON.State == ConnectionState.Closed)
                {
                    this.CON.Open();
                    this.CON.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Method Doing Query { Select } and Return The Values in DataSet if The Query Was (SELECT) You Must Define Object DataSet Type For Recieve The DataSet From Method ,
        /// if The Query Was (INSERT , DELETE , UPDATE , ets...) You Don't Have to Define Any Object .
        /// </summary>
        /// <param name="Query"> The Query , "String Type" </param>
        /// <param name="ParameterValue"> Parameters Values of Query , "String Type" OR "String-Array Type" { if There Was } .
        /// AND You Can Not Send Any ParameterValue </param>
        /// <returns> Return DataSet </returns>
        public DataSet DoQuery(string Query, params string[] ParameterValue)
        {
            try
            {
                int noOfParam = 0;
                Query = Query.Trim();
                SqlCommand sqlCommand = new SqlCommand()
                {
                    Connection = this.CON,
                    CommandText = Query
                };
                this.CMD = sqlCommand;
                for (int i = 0; i < ParameterValue.Length; i++)
                {
                    this.CMD.Parameters.AddWithValue(this.Getparam(Query, ref noOfParam), ParameterValue[i]);
                }
                Query = Query.TrimStart(new char[0]);

                if (this.CON.State == ConnectionState.Closed)
                {
                    this.CON.Open();
                }
                if (Query.Substring(0, 6).ToUpper() == "SELECT")
                {
                    SqlDataAdapter db = new SqlDataAdapter(this.CMD);
                    DataSet ds = new DataSet();
                    db.Fill(ds);
                    this.CON.Close();
                    this.DATA_SET = ds;
                }
                else
                {
                    this.CMD.ExecuteNonQuery();
                    this.CMD.Parameters.Clear();
                    this.CON.Close();
                    this.DATA_SET = null;
                }
                return this.DATA_SET;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Method Doing Query { Insert , Update , Delete } and Return String Value , So You Must Define Object String Type For Recieve The String From this Method .
        /// </summary>
        /// <param name="Query"> The Query , "String Type" </param>
        /// <param name="Out"> Name of Field You Need to Put Out has Value , "String Type" </param>
        /// <param name="ParameterValue"> Parameters Values of Query , "String Type" OR "String-Array Type" { if There Was } .
        /// AND You Can Not Send Any ParameterValue </param>
        /// <returns> Return String </returns>
        public string DoQueryAndPutOutValue(string Query, string @Out, params string[] ParameterValue)
        {
            try
            {
                int noOfParam = 0;
                string OutValue = null;
                Query = Query.Trim();
                SqlCommand sqlCommand = new SqlCommand()
                {
                    Connection = this.CON,
                    CommandText = Query
                };
                this.CMD = sqlCommand;
                for (int i = 0; i < ParameterValue.Length; i++)
                {
                    this.CMD.Parameters.AddWithValue(this.Getparam(Query, ref noOfParam), ParameterValue[i]);
                }
                Query = Query.TrimStart(new char[0]);
                if (this.CON.State == ConnectionState.Closed)
                {
                    this.CON.Open();
                }
                if (Query.Substring(0, 6).ToUpper() != "SELECT")
                {
                    SqlDataReader myReader = this.CMD.ExecuteReader();
                    while (myReader.Read())
                    {
                        OutValue = myReader[@Out].ToString();
                    }
                    myReader.Close();
                }

                return OutValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string DoQueryExecProcedure(string Query, params string[] ParameterValue)
        {
            try
            {
                int noOfParam = 0;
                string OutValue = null;
                Query = Query.Trim();
                SqlCommand sqlCommand = new SqlCommand()
                {
                    Connection = this.CON,
                    CommandText = Query
                };
                this.CMD = sqlCommand;
                for (int i = 0; i < ParameterValue.Length; i++)
                {
                    this.CMD.Parameters.AddWithValue(this.Getparam(Query, ref noOfParam), ParameterValue[i]);
                }
                Query = Query.TrimStart(new char[0]);
                if (this.CON.State == ConnectionState.Closed)
                {
                    this.CON.Open();
                }
                if (Query.Substring(0, 6).ToUpper() != "SELECT")
                {
                    SqlDataReader myReader = this.CMD.ExecuteReader();

                    OutValue = myReader.RecordsAffected.ToString();
                    // var fff = myReader["ObjId"].ToString();
                    myReader.Close();
                }

                return OutValue;
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// Method Doing StoredProcedure { Insert , Update , Delete } and Return String Value , So You Must Define Object String Type For Recieve The String From this Method .
        /// </summary>
        /// <param name="StoredProcedure"> Name of StoredProcedure , "String Type" </param>
        /// <param name="Out"> Name of Field You Need to Put Out has Value , "String Type" </param>
        /// <param name="ParameterValue"> Parameters Values of Query , "String Type" OR "String-Array Type" { if There Was } .
        /// AND You Can Not Send Any ParameterValue </param>
        /// <returns> Return String </returns>
        public string DoStoredProceAndPutOutValue(string StoredProcedure, string @Out, params string[] ParameterValue)
        {
            try
            {
                int noOfParam = 0;
                string OutValue = "";
                StoredProcedure = StoredProcedure.Trim();
                this.CMD = new SqlCommand(StoredProcedure, CON);
                CMD.CommandType = CommandType.StoredProcedure;
                for (int i = 0; i < ParameterValue.Length; i++)
                {
                    this.CMD.Parameters.AddWithValue(this.Getparam(StoredProcedure, ref noOfParam), ParameterValue[i]);
                }
                StoredProcedure = StoredProcedure.TrimStart(new char[0]);
                if (this.CON.State == ConnectionState.Closed)
                {
                    this.CON.Open();
                }
                if (StoredProcedure.Substring(0, 6).ToUpper() != "SELECT")
                {
                    SqlDataReader myReader = this.CMD.ExecuteReader();
                    while (myReader.Read())
                    {
                        OutValue = myReader[@Out].ToString();
                    }
                    myReader.Close();
                }

                return OutValue;
            }
            catch (Exception ex)
            {
                string ff = ex.Message;
                return null;
            }

        }

        /// <summary>
        /// Method Doing Query and Return The Values in DataTable , So You Must Define Object DataTable Type For Recieve The DataTable From Method .
        /// </summary>
        /// <param name="Query"> The Query ( SELECT Query ) , "String Type" </param>
        /// <returns> Return DataTable </returns>
        public DataTable FireDataTable(string Query)
        {
            try
            {
                DataTable dt = new DataTable();
                if (this.CON.State == ConnectionState.Closed)
                {
                    this.CON.Open();
                }
                SqlCommand sqlCommand = new SqlCommand()
                {
                    Connection = this.CON,
                    CommandText = Query
                };
                this.CMD = sqlCommand;
                this.CMD.CommandTimeout = this.CON.ConnectionTimeout;
                this.CMD.CommandType = CommandType.Text;
                this.CMD.CommandText = Query;
                this.CMD.CommandTimeout = this.CON.ConnectionTimeout;
                this.SQL_DA.SelectCommand = this.CMD;
                this.SQL_DA.Fill(dt);
                this.CON.Close();
                return dt;
            }
            catch (Exception ex)
            {
                var uu = ex;
                return null;
            }
        }

        /// <summary>
        /// Method Doing Query and Return The Values in DataRow , So You Must Define Object DataRow Type For Recieve The DataRow From Method .
        /// </summary>
        /// <param name="Query"> The Query ( SELECT Query ) , "String Type" </param>
        /// <returns> Return DataRow </returns>
        public DataRow FireDataRow(string Query)
        {
            try
            {
                DataTable dt = new DataTable();
                if (this.CON.State == ConnectionState.Closed)
                {
                    this.CON.Open();
                }
                SqlCommand sqlCommand = new SqlCommand()
                {
                    Connection = this.CON,
                    CommandText = Query
                };
                this.CMD = sqlCommand;
                this.CMD.CommandTimeout = this.CON.ConnectionTimeout;
                this.CMD.CommandType = CommandType.Text;
                this.CMD.CommandText = Query;
                this.CMD.CommandTimeout = this.CON.ConnectionTimeout;
                this.SQL_DA.SelectCommand = this.CMD;
                this.SQL_DA.Fill(dt);
                this.CON.Close();
                return dt.Rows[0];
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Method Doing Query and Return String Value , So You Must Define Object String Type For Recieve The String From Method .
        /// </summary>
        /// <param name="Query"> The Query ( SELECT Query 'Seq' ) , "String Type" </param>
        /// <returns> Return String </returns>
        public string FireSQL(string Query)
        {
            try
            {
                string str;
                if (this.CON.State == ConnectionState.Closed)
                {
                    this.CON.Open();
                }
                SqlCommand sqlCommand = new SqlCommand()
                {
                    Connection = this.CON,
                    CommandText = Query
                };
                this.CMD = sqlCommand;
                this.CMD.CommandTimeout = this.CON.ConnectionTimeout;
                str = (this.CMD.ExecuteScalar() == null ? "" : this.CMD.ExecuteScalar().ToString());
                return str;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Method to Check Data Existed , AND Return Bool .
        /// True: Data Existed , false: Data Non-Existed
        /// </summary>
        /// <param name="Query"> The Query ( SELECT Query ) , "String Type" </param>
        /// <returns> Return 'True' OR 'False' </returns>
        public bool CheckingIfDataExisted(string Query)
        {
            bool flag;
            DataTable dT = new DataTable();
            dT = this.FireDataTable(Query);
            if (dT.Rows.Count > 0)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Method Can Be Insert NULL in Feild That Type is "Numeric" OR "Decimal" .
        /// </summary>
        /// <param name="Value"> The NUMBER Content in Any Control (ex: TextBox.text OR Lable.text) , "String Type and Number Format" </param>
        /// <returns> Return Numbers OR NULL </returns>
        public object ToDBNullNumeric(object Value)
        {
            #region  Numeric ميثود خاصة نستدعيها اذا اردنا اضافة رقم او فراغ في قاعدة البيانات وكان نوع الحقل في قاعدة البيانات 
            // هو اي رقم نرسلة في التيكست بوكس  (object value) 
            // هو النص (من نوع نمبر) الذي في التيكس بوكس ,, كالتالي Value مثلا نستدعي هذه الميثود وتكون قيمة الـ
            // ToDBNullNumeric(TextBox.Text)
            #endregion

            if (Value != null && Value.ToString() != "")
            {
                return Convert.ToDecimal(Value);
            }
            else                                                // حيث نستخدمها اذا كنت تريد في قاعدة البيانات تقبل فراغ او نل ,, وهي من نوع نيومرك
            {
                return "NULL";
            }
        }

        /// <summary>
        /// Method Can Be Insert NULL in Feild That Type is "Nvarchar" OR 'any String Type' .
        /// </summary>
        /// <param name="Value"> The STRING Content in Any Control (ex: TextBox.text OR Lable.text) , "String Type and String Format" </param>
        /// <returns> Return String OR NULL </returns>  
        public object ToDBNullString(object Value)
        {
            #region  Nvarchar ميثود خاصة نستدعيها اذا اردنا اضافة نص او فراغ في قاعدة البيانات وكان نوع الحقل في قاعدة البيانات 
            // هو اي النص نرسلة في التيكست بوكس  (object value) 
            // هو النص (من نوع سترينك) الذي في التيكس بوكس ,, كالتالي Value مثلا نستدعي هذه الميثود وتكون قيمة الـ
            // ToDBNullString(TextBox.Text)
            #endregion

            if (Value != null && Value.ToString() != "")
            {
                return Value.ToString();
            }
            else
            {
                return "NULL";
            }
        }

        /// <summary>
        /// Method Can Be Insert NULL in Feild That Type is "Date" OR "DateTime" .
        /// </summary>
        /// <param name="Value"> The DATETIME Content in Formating Date (ex: MaskedTextBox.text OR TextBox.text) , "String Type and Date Format" </param>
        /// <returns> Return DateTime OR NULL </returns>
        public object ToDBNullDateTime(object Value)
        {
            #region  DateTime ميثود خاصة نستدعيها اذا اردنا اضافة تاريخ او فراغ في قاعدة البيانات وكان نوع الحقل في قاعدة البيانات 
            // ToDBNullDateTime(MaskedTextBox.Text)
            #endregion

            if (Value != null && Value.ToString() != "")
            {
                return Convert.ToDateTime(Value);
            }
            else
            {
                return DBNull.Value;
            }
        }

        /// <summary>
        /// Method Doing Checking Into Text and Convert any Char into Text From ( أ , إ , آ ) To ( ا ) ,, and Return The Correct Text ,
        /// Using this Method When INSERT Arabic Words in DataBase .
        /// </summary>
        /// <param name="YourTextForCheck"> The Text You Need to Checking , "String Type" </param>
        /// <returns> Return String </returns>
        public string CheckText(string YourTextForCheck)
        {
            try
            {
                string Value = YourTextForCheck;

                foreach (char C in Value)
                {
                    if (C == 'أ' || C == 'إ' || C == 'آ')
                    {
                        Value = Value.Replace(C, 'ا');
                    }
                }
                return Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        ///////////////////////////////////// Private Methods ////////////////////////////////////////        
        private string Getparam(string query, ref int y)
        {
            try
            {
                string param = "";
                int no = query.IndexOf('@', y);
                int space = (query.IndexOf(" ", no) < 0 ? query.Length : query.IndexOf(" ", no));
                int comma = (query.IndexOf(",", no) < 0 ? query.Length : query.IndexOf(",", no));
                int bracket = (query.IndexOf(")", no) < 0 ? query.Length : query.IndexOf(")", no));
                if (!(space > comma ? true : space > bracket))
                {
                    param = query.Substring(no, space - no);
                    y = query.IndexOf(" ", no + 1);
                }
                else if (!(comma > space ? true : comma > bracket))
                {
                    param = query.Substring(no, comma - no);
                    y = query.IndexOf(",", no + 1);
                }
                else if ((bracket > comma ? true : bracket > space))
                {
                    param = query.Substring(no, query.Length - no);
                }
                else
                {
                    param = query.Substring(no, bracket - no);
                    y = query.IndexOf(")", no + 1);
                }
                return param;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
