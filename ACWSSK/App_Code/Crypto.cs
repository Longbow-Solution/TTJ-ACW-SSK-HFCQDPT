﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ACWSSK.App_Code
{
    public partial class Crypto
    {
        public static string Encrypt(string Input, string Password, string Salt)
        {
            if (Input == null || Input.Length <= 0) return "";

            Rfc2898DeriveBytes keyGen = __createKeyGen(Password, Salt);
            ICryptoTransform transformer = __createEncryptor(keyGen);
            byte[] transformed = __transform(Encoding.Default.GetBytes(Input), transformer);

            return Convert.ToBase64String(transformed);
        }

        public static string Decrypt(string Input, string Password, string Salt)
        {
            if (Input == null || Input.Length <= 0) return "";

            Rfc2898DeriveBytes keyGen = __createKeyGen(Password, Salt);
            ICryptoTransform transformer = __createDecryptor(keyGen);
            byte[] transformed = __transform(Convert.FromBase64String(Input), transformer);

            return Encoding.Default.GetString(transformed);
        }

        static string Hash(string Input)
        {
            if (Input == null || Input.Length <= 0) return "";

            StringBuilder result = new StringBuilder();
            SHA1 provider = SHA1.Create();
            byte[] __result = provider.ComputeHash(Encoding.Default.GetBytes(Input));

            foreach (Byte b in __result)
                result.Append(String.Format("{0:x2}", b));

            return result.ToString();

        }

        static Rfc2898DeriveBytes __createKeyGen(string Password, string Salt)
        {
            return new Rfc2898DeriveBytes(Password, Encoding.Default.GetBytes(Salt));
        }

        static ICryptoTransform __createEncryptor(Rfc2898DeriveBytes KeyGen)
        {
            TripleDES provider = TripleDES.Create();
            return provider.CreateEncryptor(KeyGen.GetBytes(16), KeyGen.GetBytes(16));
        }

        static ICryptoTransform __createDecryptor(Rfc2898DeriveBytes KeyGen)
        {
            TripleDES provider = TripleDES.Create();
            return provider.CreateDecryptor(KeyGen.GetBytes(16), KeyGen.GetBytes(16));
        }

        static byte[] __transform(byte[] Input, ICryptoTransform Transformer)
        {
            MemoryStream ms = new MemoryStream();
            byte[] result;

            CryptoStream writer = new CryptoStream(ms, Transformer, CryptoStreamMode.Write);
            writer.Write(Input, 0, Input.Length);
            writer.FlushFinalBlock();

            ms.Position = 0;
            result = ms.ToArray();

            ms.Close();
            writer.Close();
            return result;
        }
    }
}