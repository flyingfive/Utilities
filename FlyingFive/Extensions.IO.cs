using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FlyingFive
{
    public static partial class Extensions
    {

        /// <summary>  
        /// 计算文件MD5值（大文件请合理调整bufferSize参数）
        /// </summary>  
        /// <param name="fileInfo">文件地址</param>  
        /// <param name="bufferSize">自定义缓冲区大小,单位：K，默认1M</param>  
        /// <returns>MD5Hash</returns>  
        public static string MD5File(this FileInfo fileInfo, int bufferSize = 1024)
        {
            if (!fileInfo.Exists) { throw new ArgumentException(string.Format("文件<{0}>, 不存在", fileInfo)); }
            if (bufferSize <= 0) { bufferSize = 1024; }
            bufferSize = 1024 * bufferSize;             //自定义缓冲区大小
            byte[] buffer = new byte[bufferSize];
            using (Stream inputStream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var hashAlgorithm = new MD5CryptoServiceProvider())
            {
                int readLength = 0;//每次读取长度  
                var output = new byte[bufferSize];
                while ((readLength = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //计算MD5  
                    hashAlgorithm.TransformBlock(buffer, 0, readLength, output, 0);
                }
                //完成最后计算，必须调用(由于上一部循环已经完成所有运算，所以调用此方法时后面的两个参数都为0)  
                hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
                string md5 = BitConverter.ToString(hashAlgorithm.Hash);
                hashAlgorithm.Clear();
                inputStream.Close();
                md5 = md5.Replace("-", "");
                return md5;
            }
        }
    }
}
