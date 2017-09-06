using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Conch.Common
{
    public class Utility
    {
        public static string Encrypt(string source)
        {
            using(MD5 md5 = MD5.Create())
            {
                var hash = GetMd5Hash(md5, source);
                return hash;
            }
        }

        public static bool VerifyEncrypted(string source, string encryptedCode)
        {
            var hash = Encrypt(source);
            if (0 == hash.CompareTo(encryptedCode))
            {
                return true;
            }
            return false;
        }
        static private string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }



    internal class SymmetricMethod

    {
        /// <summary>

        /// 用对称加密算法加密

        /// </summary>

        /// <param name="str">需要被加密的字符串</param>

        /// <returns>返回加密后的字符串</returns>

        static public string EncryptToSelf(string str)

        {
            var key = "Guz(%&hj7x89H$yuBI0456FtmaT5&fvHUFCy76*h凤飞飞%(HilJ$lhj!y6&(*jkP87jH7dasdasda$@#$$%$#^SFAdasa#$##@￥@";
            var value = "E4ghj*Ghg7!rNIfb&95GUY86GfghUb发生#er57HBh(u%g6HJ($42￥@#￥#@￥%#dadasdsasd";
            SymmetricMethod symmetMth = new SymmetricMethod(key, value);

            return symmetMth.Encrypto(str);

        }



        private SymmetricAlgorithm mobjCryptoService;

        private string Key = "";

        private string IVKey = "";

        /// <summary>

        /// 对称加密类的构造函数

        /// </summary>

        /// <param name="keyValue">产生随机密钥的值</param>

        /// <param name="IVKeyValue">产生向量密钥的值</param>

        internal SymmetricMethod(string keyValue, string IVKeyValue)

        {

            mobjCryptoService = new RijndaelManaged();

            //Key = "Guz(%&hj7x89H$yuBI0456FtmaT5&fvHUFCy76*h凤飞飞%(HilJ$lhj!y6&(*jkP87jH7dasdasda$@#$$%$#^SFAdasa#$##@¥@";

            Key = keyValue;

            IVKey = IVKeyValue;

        }

        /// <summary>

        /// 获得密钥

        /// </summary>

        /// <returns>密钥</returns>

        private byte[] GetLegalKey()

        {

            string sTemp = Key;

            mobjCryptoService.GenerateKey();

            byte[] bytTemp = mobjCryptoService.Key;

            int KeyLength = bytTemp.Length;

            if (sTemp.Length > KeyLength)

                sTemp = sTemp.Substring(0, KeyLength);

            else if (sTemp.Length < KeyLength)

                sTemp = sTemp.PadRight(KeyLength, ' ');

            return ASCIIEncoding.ASCII.GetBytes(sTemp);

        }

        /// <summary>

        /// 获得初始向量IV

        /// </summary>

        /// <param name="IVValue">用于得到随机向量的密钥</param>

        /// <returns>初试向量IV</returns>

        private byte[] GetLegalIV(string IVValue)

        {

            //string sTemp = "E4ghj*Ghg7!rNIfb&95GUY86GfghUb发生#er57HBh(u%g6HJ($42¥@#¥#@¥%#dadasdsasd";

            mobjCryptoService.GenerateIV();

            byte[] bytTemp = mobjCryptoService.IV;

            int IVLength = bytTemp.Length;

            if (IVValue.Length > IVLength)

                IVValue = IVValue.Substring(0, IVLength);

            else if (IVValue.Length < IVLength)

                IVValue = IVValue.PadRight(IVLength, ' ');

            return ASCIIEncoding.ASCII.GetBytes(IVValue);

        }

        /// <summary>

        /// 加密方法

        /// </summary>

        /// <param name="Source">待加密的串</param>

        /// <returns>经过加密的串</returns>

        internal string Encrypto(string Source)

        {

            byte[] bytIn = UTF8Encoding.UTF8.GetBytes(Source);

            MemoryStream ms = new MemoryStream();

            mobjCryptoService.Key = GetLegalKey();

            mobjCryptoService.IV = GetLegalIV(IVKey);

            ICryptoTransform encrypto = mobjCryptoService.CreateEncryptor();

            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);

            cs.Write(bytIn, 0, bytIn.Length);

            cs.FlushFinalBlock();

            ms.Close();

            byte[] bytOut = ms.ToArray();

            return Convert.ToBase64String(bytOut);

        }

    }


}