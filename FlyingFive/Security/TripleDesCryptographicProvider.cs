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
    public class TripleDesCryptographicProvider : ICryptographicProvider, IDisposable
    {
        private TripleDESCryptoServiceProvider _serviceProvider = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">密钥，长度为16或24</param>
        /// <param name="iv">加密向量</param>
        public TripleDesCryptographicProvider(string key, string iv = "")
        {
            if (key.Length != 16 && key.Length != 24) { throw new ArgumentException("密码长度不正确。必需为：字母、数字、特殊符号三种内容组合。"); }
            if (!string.IsNullOrEmpty(iv) && iv.Length < 8)
            {
                throw new ArgumentException("指定的向量长度不能少于8位。");
            }
            _serviceProvider = new TripleDESCryptoServiceProvider();
            _serviceProvider.Mode = CipherMode.CBC;
            _serviceProvider.Padding = PaddingMode.PKCS7;
            _serviceProvider.Key = Encoding.UTF8.GetBytes(key);
            _serviceProvider.IV = Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(iv) ? key.Substring(0, 8) : iv);
        }

        /// <summary>
        /// 字符串加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>加密后的字符串</returns>
        public string Encrypt(string plainText)
        {
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
            ICryptoTransform decryptor = _serviceProvider.CreateDecryptor();
            byte[] buffer = Convert.FromBase64String(cipherText);
            var plainText = System.Text.Encoding.UTF8.GetString(decryptor.TransformFinalBlock(buffer, 0, buffer.Length));
            return plainText;
        }

        /// <summary>
        /// 字节加密
        /// </summary>
        /// <param name="plainData">明文</param>
        /// <returns>加密后的字节数据</returns>
        public byte[] Encrypt(byte[] plainData)
        {
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
            ICryptoTransform decryptor = _serviceProvider.CreateDecryptor();
            var plainData = decryptor.TransformFinalBlock(cipherData, 0, cipherData.Length);
            return plainData;
        }

        public void Dispose()
        {
            if (_serviceProvider != null)
            {
                _serviceProvider.Clear();
                _serviceProvider.Dispose();
            }
        }
    }
}
