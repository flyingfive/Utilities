using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FlyingFive.Security
{
    /// <summary>
    /// AES加密封装
    /// </summary>
    public class AesCryptographicProvider : ICryptographicProvider, IDisposable
    {
        private RijndaelManaged _rijndaelManaged = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">密钥，长度为16、24或32</param>
        /// <param name="iv">加密向量</param>
        public AesCryptographicProvider(string key, string iv = "")
        {
            //16位长度=AES128，24=192，32位=AES256
            if (key.Length != 16 && key.Length != 24 && key.Length != 32) { throw new ArgumentException("密码长度不正确。必需为：字母、数字、特殊符号三种内容组合。"); }
            if (!string.IsNullOrEmpty(iv) && iv.Length < 16)
            {
                if (iv.Length < 16) throw new ArgumentException("指定的向量长度不能少于16位。");
            }
            _rijndaelManaged = new RijndaelManaged()
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(iv) ? key.Substring(0, 16) : iv)
            };
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) { throw new ArgumentException("参数cipherText不能为空"); }
            var buffer = Convert.FromBase64String(cipherText);
            var data = _rijndaelManaged.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length);
            var plainText = Encoding.UTF8.GetString(data);
            return plainText;
        }

        public byte[] Decrypt(byte[] cipherData)
        {
            if (cipherData == null) { throw new ArgumentNullException("参数cipherData不能为null"); }
            if (cipherData.Length == 0) { return cipherData; }
            var plainData = _rijndaelManaged.CreateDecryptor().TransformFinalBlock(cipherData, 0, cipherData.Length);
            return plainData;
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) { throw new ArgumentException("参数plainText不能为空"); }
            var buffer = Encoding.UTF8.GetBytes(plainText);
            var cipherData = _rijndaelManaged.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length);
            var cipherText = Convert.ToBase64String(cipherData);
            return cipherText;
        }

        public byte[] Encrypt(byte[] plainData)
        {
            if (plainData == null) { throw new ArgumentNullException("参数plainData不能为null"); }
            if (plainData.Length == 0) { return plainData; }
            var cipherData = _rijndaelManaged.CreateEncryptor().TransformFinalBlock(plainData, 0, plainData.Length);
            return cipherData;
        }

        public void Dispose()
        {
            if (_rijndaelManaged != null)
            {
                _rijndaelManaged.Clear();
                _rijndaelManaged.Dispose();
            }
        }
    }
}
