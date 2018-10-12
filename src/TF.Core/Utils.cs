using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Gibbed.IO;

namespace TF.Core
{
    internal static class Utils
    {
        public static string CalculateHash(string path)
        {
            /*using (var cp = new SHA1CryptoServiceProvider())
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
            }*/
            return string.Empty;
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

        public static int FindPattern(byte[] input, byte[] pattern, int startIndex)
        {
            var subArray = new byte[input.Length - startIndex];

            Array.Copy(input, startIndex, subArray, 0, subArray.Length);

            var result = -1;
            using (var ms = new MemoryStream(subArray))
            {
                result = ms.FindPattern(pattern);
            }

            if (result != -1)
            {
                return result + startIndex;
            }
            else
            {
                return -1;
            }
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

        public static byte[] GetBytes(this SQLiteDataReader reader, string field)
        {
            const int chunkSize = 2 * 1024;
            var buffer = new byte[chunkSize];
            var fieldIndex = reader.GetOrdinal(field);
            long fieldOffset = 0;
            using (var stream = new MemoryStream())
            {
                long bytesRead;
                while ((bytesRead = reader.GetBytes(fieldIndex, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }

        public static short PeekValueS16(this Stream s, Endian endian)
        {
            var value = s.ReadValueS16(endian);
            s.Seek(-2, SeekOrigin.Current);
            return value;
        }
    }
}
