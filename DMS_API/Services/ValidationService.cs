using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_API.Services
{
    public static class Validation
    {
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str.Trim());
        }

        public static bool IsInt(this string num)
        {
            return int.TryParse(num.Trim(), out int value);
        }

        public static bool IsDate(this string date)
        {
            return DateTime.TryParseExact(date.Trim(), new string[] { "dd/MM/yyyy", "MM/dd/yyyy" },
                                          CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime value);
        }

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

        public static bool IsPasswordStrength1(this string pass)
        {
            if (string.IsNullOrWhiteSpace(pass) ||
                pass.Length < 8 ||
                pass.Any(char.IsUpper).ToString().Length == 0 ||
                pass.Any(char.IsLower).ToString().Length == 0 ||
                pass.Any(char.IsDigit).ToString().Length == 0 ||
                pass.Any(char.IsLetter).ToString().Length == 0)
                return false;

            return true;
        }
    }
}