using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils.NET.Utils
{
    public enum PasswordStrength
    {
        Blank,
        VeryWeak,
        Weak,
        Medium,
        Strong
    }

    public static class StringUtils
    {
        public static readonly char[] alphaNumericCharacters = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public static uint ParseHex(string hex)
        {
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            return Convert.ToUInt32(hex, 16);
        }

        public static IEnumerable<T> ComponentsFromString<T>(string source, char delimeter, Func<string, T> parser)
        {
            return source.Split(delimeter).Select(_ => parser(_));
        }

        public static string ComponentsToString<T>(char delimeter, params T[] components)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < components.Length; i++)
            {
                builder.Append(components[i].ToString());
                if (i != components.Length - 1)
                    builder.Append(delimeter);
            }
            return builder.ToString();
        }

        public static string ComponentsToString<T>(string delimeter, params T[] components)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < components.Length; i++)
            {
                builder.Append(components[i].ToString());
                if (i != components.Length - 1)
                    builder.Append(delimeter);
            }
            return builder.ToString();
        }

        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        public static string ApplyPlural(string word, int count)
        {
            if (count == 1)
                return word;
            return word + 's';
        }

        public static bool DoesMatchPattern(string input, string pattern)
        {
            pattern = pattern.ToLower();
            pattern = pattern.Replace(" ", "");
            input = input.ToLower();
            input = input.Replace(" ", "");

            int lastFound = 0;

            for (int i = 0; i < pattern.Length; i++)
            {
                var searchCharacter = pattern[i];
                int index = input.IndexOf(searchCharacter, lastFound);
                if (index < 0) return false;
                lastFound = index + 1;
            }
            return true;
        }

        public static string Labelize(string input)
        {
            var builder = new StringBuilder(input);
            bool readyForSpace = false;
            for (int i = 0; i < builder.Length; i++)
            {
                var c = builder[i];
                if (char.IsLower(c))
                {
                    readyForSpace = true;
                    continue;
                }

                if (char.IsUpper(c) && readyForSpace)
                {
                    builder.Insert(i, ' ');
                    i++;
                    continue;
                }
            }
            return builder.ToString();
        }

        public static PasswordStrength GetPasswordStrength(string password)
        {
            int score = 0;
            if (string.IsNullOrWhiteSpace(password)) return PasswordStrength.Blank;
            if (password.Length >= 8) score++;
            if (HasUppercaseLetter(password) && HasLowercaseLetter(password)) score++;
            if (HasDigit(password)) score++;
            if (HasSpecialCharacter(password)) score++;
            return (PasswordStrength)score;
        }

        public static bool HasUppercaseLetter(string input)
        {
            foreach (var c in input)
                if (char.IsUpper(c))
                    return true;
            return false;
        }

        public static bool HasLowercaseLetter(string input)
        {
            foreach (var c in input)
                if (char.IsLower(c))
                    return true;
            return false;
        }

        public static bool HasDigit(string input)
        {
            foreach (var c in input)
                if (char.IsDigit(c))
                    return true;
            return false;
        }

        public static bool HasSpecialCharacter(string input)
        {
            foreach (var c in input)
                if (!char.IsLetterOrDigit(c))
                    return true;
            return false;
        }
        

        public static bool IsValidEmail(string email) // function pulled from https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (email.Contains('+')) return false;

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string ToHexString(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            int length = data.Length;
            char[] hex = new char[length * 2];
            int num1 = 0;
            for (int index = 0; index < length * 2; index += 2)
            {
                byte num2 = data[num1++];
                hex[index] = GetHexValue(num2 / 0x10);
                hex[index + 1] = GetHexValue(num2 % 0x10);
            }
            return new string(hex);
        }

        private static char GetHexValue(int i)
        {
            if (i < 10)
            {
                return (char)(i + 0x30);
            }
            return (char)((i - 10) + 0x41);
        }
    }
}
