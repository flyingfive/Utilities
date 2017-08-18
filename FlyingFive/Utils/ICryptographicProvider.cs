using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Utils
{
    /// <summary>
    /// 表示提供加解密能力的工具
    /// </summary>
    public interface ICryptographicProvider
    {
        /// <summary>
        /// 字符串加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="password">密码</param>
        /// <returns>加密后的字符串</returns>
        string Encrypt(string plainText, string password);
        /// <summary>
        /// 字符串解密
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="password">密码</param>
        /// <returns>解密后的字符串</returns>
        string Decrypt(string cipherText, string password);
        /// <summary>
        /// 字节加密
        /// </summary>
        /// <param name="plainData">明文</param>
        /// <param name="password">密码</param>
        /// <returns>加密后的字节数据</returns>
        byte[] Encrypt(byte[] plainData, string password);
        /// <summary>
        /// 字节解密
        /// </summary>
        /// <param name="cipherData">密文</param>
        /// <param name="password">密码</param>
        /// <returns>解密后的字节数据</returns>
        byte[] Decrypt(byte[] cipherData, string password);
    }
}
