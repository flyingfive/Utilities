using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FlyingFive.Security
{
    /// <summary>
    /// 3DES加密器
    /// </summary>
    public class DES3CryptographicProvider : ICryptographicProvider
    {
        private TripleDESCryptoServiceProvider _serviceProvider = null;
        private string _password = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        public DES3CryptographicProvider(string password)
        {
            if (password.Length > 24) { throw new ArgumentException("密码长度不能超过24位。必需为：字母、数字、特殊符号三种内容组合。"); }
            this._password = password;
            _serviceProvider = new TripleDESCryptoServiceProvider();
            _serviceProvider.Mode = CipherMode.ECB;
            _serviceProvider.Padding = PaddingMode.PKCS7;
        }

        /// <summary>
        /// 字符串加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>加密后的字符串</returns>
        public string Encrypt(string plainText)
        {
            var key = new byte[24];
            var pwdData = System.Text.Encoding.UTF8.GetBytes(_password);
            Array.Copy(pwdData, key, pwdData.Length);
            _serviceProvider.Key = key;
            var encryptor = _serviceProvider.CreateEncryptor();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(plainText);
            var cipherText = Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
            return cipherText;
        }

        /// <summary>
        /// 字符串解密
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <returns>解密后的字符串</returns>
        public string Decrypt(string cipherText)
        {
            var key = new byte[24];
            var pwdData = System.Text.Encoding.UTF8.GetBytes(_password);
            Array.Copy(pwdData, key, pwdData.Length);
            _serviceProvider.Key = key;
            //_serviceProvider.Key = System.Text.Encoding.UTF8.GetBytes(password);
            ICryptoTransform decryptor = _serviceProvider.CreateDecryptor();
            byte[] Buffer = Convert.FromBase64String(cipherText);
            var plainText = System.Text.Encoding.UTF8.GetString(decryptor.TransformFinalBlock(Buffer, 0, Buffer.Length));
            return plainText;
        }

        /// <summary>
        /// 字节加密
        /// </summary>
        /// <param name="plainData">明文</param>
        /// <returns>加密后的字节数据</returns>
        public byte[] Encrypt(byte[] plainData)
        {
            var key = new byte[24];
            var pwdData = System.Text.Encoding.UTF8.GetBytes(_password);
            Array.Copy(pwdData, key, pwdData.Length);
            _serviceProvider.Key = key;
            var encryptor = _serviceProvider.CreateEncryptor();
            var cipherData = encryptor.TransformFinalBlock(plainData, 0, plainData.Length);
            return cipherData;
        }

        /// <summary>
        /// 字节解密
        /// </summary>
        /// <param name="cipherData">密文</param>
        /// <returns>解密后的字节数据</returns>
        public byte[] Decrypt(byte[] cipherData)
        {
            var key = new byte[24];
            var pwdData = System.Text.Encoding.UTF8.GetBytes(_password);
            Array.Copy(pwdData, key, pwdData.Length);
            _serviceProvider.Key = key;
            ICryptoTransform decryptor = _serviceProvider.CreateDecryptor();
            var plainData = decryptor.TransformFinalBlock(cipherData, 0, cipherData.Length);
            return plainData;
        }
    }
}
