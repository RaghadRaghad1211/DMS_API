using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
namespace DMS_API.Services
{
    /// <summary>
    /// Service work with Validations
    /// </summary>
    public static class ValidationService
    {
        #region Functions
        /// <summary>
        /// Check Text is empty or null,
        /// and return bool variable,
        /// true: text is empty.
        /// false: text is not empty.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string str)
        {
            str = str == null ? "" : str;
            return string.IsNullOrEmpty(str.Trim());
        }
        /// <summary>
        /// Check class fields are empty or null,
        /// and return string variable which field is empty.
        /// </summary>
        /// <param name="ObjClass"></param>
        /// <returns></returns>
        public static string IsEmptyList(this object ObjClass)
        {
            var obj = ObjClass.GetType();
            string msg = ""; int x = 1;
            foreach (PropertyInfo property in obj.GetProperties())
            {
                var name = property.Name;
                var value = property.GetValue(ObjClass, null)?.ToString();
                if (value == null || value.Trim() == "")
                {
                    msg = msg + x.ToString() + ". ";
                    msg = msg + name + "  ";
                    //msg = msg + name + "\n";
                    x++;
                }
            }
            return msg;
        }
        /// <summary>
        /// Check variable is integer or not,
        /// and return bool variable,
        /// true: is integer.
        /// false: is not integer.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool IsInt(this string num)
        {
            return int.TryParse(num.Trim(), out int value);
        }
        /// <summary>
        /// Check variable is date or not,
        /// and return bool variable,
        /// true: is date.
        /// false: is not date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsDate(this string date)
        {
            return DateTime.TryParseExact(date.Trim(), new string[] { "dd/MM/yyyy", "MM/dd/yyyy" },
                                          CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime value);
        }
        /// <summary>
        /// Check variable is date or not,
        /// and return bool variable,
        /// true: is date.
        /// false: is not date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool CheckDate(string date)
        {
            try
            {
                DateTime.ParseExact(date, "dd/MM/yyyy", null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Check variable is phoneNumber or not,
        /// and return bool variable,
        /// true: is phoneNumber.
        /// false: is not phoneNumber.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static bool IsPhoneNumber(this string phoneNumber)
        {
            int count = phoneNumber.Length;
            if (count == 13)
            {
                if (phoneNumber.Substring(0, 4) == "9647")
                {
                    return Regex.Match(phoneNumber, @"^([0-9]{13})$").Success;
                }
                return false;
            }
            return false;
        }
        /// <summary>
        /// Check variable is email or not,
        /// and return bool variable,
        /// true: is email.
        /// false: is not email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(this string email)
        {

            try
            {
                if (email.Contains('@'))
                {
                    return email.Contains('.');
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Check file size of document is valid or not,
        /// and return bool variable,
        /// true: size is valid.
        /// false: size is not valid.
        /// </summary>
        /// <param name="DocumentSize">Document Size</param>
        /// <returns></returns>
        public static bool FileSizeIsValid(this long DocumentSize)
        {
            float fileSizeMB = float.Parse((DocumentSize / (1024f * 1024f)).ToString("0.00"));
            float maxFileSizeMB = float.Parse(GlobalService.MaxFileSize);
            if (fileSizeMB > maxFileSizeMB)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Check parameter is Sql Injection or not,
        /// and return bool variable,
        /// true: input is Sql Injection.
        /// false: input is not Sql Injection. 
        /// </summary>
        /// <param name="Input">input parameter</param>
        /// <returns></returns>
        public static bool IsSqlInjection(this string Input)
        {
            bool isSQLInjection = false;
            string[] sqlCheckList = { "--",";--",";","/*","*/","@@","@","=","+","char","nchar","varchar","nvarchar","convert",
                                      "set","union","alter","begin","cast","create","cursor","and","or","end","exec","execute",
                                      "declare","select","insert","update","delete","waitfor","drop","fetch","kill","truncate",
                                      "from","sys","sysobjects","syscolumns","database","table","xp_cmdshell"};
            Input = Input == null ? "" : Input.Trim().ToLower();
            for (int i = 0; i <= sqlCheckList.Length - 1; i++)
            {
                if ((Input.Contains(sqlCheckList[i], StringComparison.OrdinalIgnoreCase)))
                { isSQLInjection = true; }
            }
            return isSQLInjection;
        }
        /// <summary>
        /// Check class parameters is Sql Injection or not,
        /// and return bool variable,
        /// true: input is Sql Injection.
        /// false: input is not Sql Injection. 
        /// </summary>
        /// <param name="InputClass">input class parameters</param>
        /// <returns></returns>
        public static bool IsSqlInjectionList(this object InputClass)
        {
            bool isSQLInjection = false;
            string[] sqlCheckList = { "--",";--",";","/*","*/","@@","@","=","+","char","nchar","varchar","nvarchar","convert",
                                      "set","union","alter","begin","cast","create","cursor","and","or","end","exec","execute",
                                      "declare","select","insert","update","delete","waitfor","drop","fetch","kill","truncate",
                                      "from","sys","sysobjects","syscolumns","database","table","xp_cmdshell"};
            var obj = InputClass.GetType();
            foreach (PropertyInfo property in obj.GetProperties())
            {
                var name = property.Name;
                var value = property.GetValue(InputClass, null)?.ToString();
                value = value == null ? "" : value.Trim().ToLower();
                // string CheckString = value.Replace("'", "''");
                for (int i = 0; i <= sqlCheckList.Length - 1; i++)
                {
                    if (property.PropertyType.Name != "IFormFile")
                    {
                        if ((value.Contains(sqlCheckList[i], StringComparison.OrdinalIgnoreCase)))
                        { isSQLInjection = true; }
                    }
                }
            }
            return isSQLInjection;
        }
        #endregion
    }
}