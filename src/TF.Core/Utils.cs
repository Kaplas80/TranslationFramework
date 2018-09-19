using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace TF.Core
{
    internal static class Utils
    {
        public static string CalculateHash(string path)
        {
            using (var cp = new SHA1CryptoServiceProvider())
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var hash = cp.ComputeHash(fs);

                    var sBuilder = new StringBuilder();

                    foreach (var value in hash)
                    {
                        sBuilder.Append(value.ToString("x2"));
                    }

                    return sBuilder.ToString();
                }
            }
        }

        public static int GetLength(this string str, Encoding encoding)
        {
            var temp = str.Replace("^^", string.Empty); // Limpio las pausas, que no cuentan para la longitud
            return encoding.GetByteCount(temp);
        }

        public static string ReplaceChars(string input, IList<Tuple<string, string>> charReplacementList)
        {
            foreach (var tuple in charReplacementList)
            {
                input = input.Replace(tuple.Item1, tuple.Item2);
            }

            return input;
        }

        public static int FindPattern(this Stream stream, byte[] pattern)
        {
            if (pattern.Length > stream.Length)
                return -1;

            var buffer = new byte[pattern.Length];

            while (stream.Read(buffer, 0, pattern.Length) == pattern.Length)
            {
                if (pattern.SequenceEqual(buffer))
                {
                    return (int)(stream.Position - pattern.Length);
                }

                stream.Position -= pattern.Length - PadLeftSequence(buffer, pattern);
            }

            return -1;
        }

        private static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            var i = 1;
            while (i < bytes.Length)
            {
                var n = bytes.Length - i;
                var aux1 = new byte[n];
                var aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                {
                    return i;
                }
                i++;
            }
            return i;
        }
    }
}
