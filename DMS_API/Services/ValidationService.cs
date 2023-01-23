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
        /// Check password strength,
        /// and return string variable, which level of strength
        /// true: is phoneNumber.
        /// false: is not phoneNumber.
        /// </summary>
        /// <param name="TxtPassword"></param>
        /// <returns></returns>
        public static string IsPasswordStrength(this string TxtPassword)
        {
            string[] Strength = { "يجب ادخال 8 احرف", "ضعيف", "متوسط", "قوي", "قوي جداً" };

            string pBox = TxtPassword.Trim();
            if (pBox.Length < 8)
            {
                return Strength[0];
            }
            else
            {
                int numberOfDigits = 0, numberOfLowerLetters = 0, numberOfUpperLetters = 0, numberOfSymbols = 0;

                foreach (char c in pBox)
                {
                    if (char.IsNumber(c))
                    {
                        numberOfDigits++;
                    }
                    else if (char.IsLower(c))
                    {
                        numberOfLowerLetters++;
                    }
                    else if (char.IsUpper(c))
                    {
                        numberOfUpperLetters++;
                    }
                    else if (char.IsPunctuation(c))
                    {
                        numberOfSymbols++;
                    }
                }

                if (numberOfDigits > 0 && numberOfSymbols > 0 && numberOfLowerLetters > 0 && numberOfUpperLetters > 0)
                {
                    return Strength[4];
                }
                else if ((numberOfDigits.Equals(0) && numberOfSymbols > 0 && numberOfLowerLetters > 0 && numberOfUpperLetters > 0) || (numberOfDigits > 0 && numberOfSymbols.Equals(0) && numberOfLowerLetters > 0 && numberOfUpperLetters > 0) || (numberOfDigits > 0 && numberOfSymbols > 0 && numberOfLowerLetters.Equals(0) && numberOfUpperLetters > 0) || (numberOfDigits > 0 && numberOfSymbols > 0 && numberOfLowerLetters > 0 && numberOfUpperLetters.Equals(0)))
                {
                    return Strength[3];
                }
                else if ((numberOfDigits.Equals(0) && numberOfSymbols.Equals(0) && numberOfLowerLetters > 0 && numberOfUpperLetters > 0) || (numberOfDigits.Equals(0) && numberOfLowerLetters.Equals(0) && numberOfSymbols > 0 && numberOfUpperLetters > 0) || (numberOfDigits.Equals(0) && numberOfUpperLetters.Equals(0) && numberOfLowerLetters > 0 && numberOfSymbols > 0) || (numberOfSymbols.Equals(0) && numberOfLowerLetters.Equals(0) && numberOfDigits > 0 && numberOfUpperLetters > 0) || (numberOfSymbols.Equals(0) && numberOfUpperLetters.Equals(0) && numberOfDigits > 0 && numberOfLowerLetters > 0) || (numberOfLowerLetters.Equals(0) && numberOfUpperLetters.Equals(0) && numberOfDigits > 0 && numberOfSymbols > 0))
                {
                    return Strength[2];
                }
                else
                {
                    return Strength[1];
                }
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
        /// Check Sql Injection is valid or not,
        /// and return bool variable,
        /// true: input is valid.
        /// false: input is not valid. 
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns></returns>
        public static bool SqlInjectionValid(this string input)
        {
            if (input.ToLower().Contains("drop") || input.ToLower().Contains("alter") || input.ToLower().Contains("delete"))
            {
                return false;
            }
            return Regex.IsMatch(input, @"^[a-zA-Z0-9]+$");
        }
        #endregion
    }
}