﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FlyingFive.Utils
{
    public class FileHelper
    {
        /// <summary>
        /// 目录复制
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="destDir"></param>
        /// <param name="ignoreDirNames"></param>
        public static void CopyDirectory(string srcDir, string destDir, params string[] ignoreDirNames)
        {
            if (!Directory.Exists(srcDir)) { return; }
            if (!Directory.Exists(destDir)) { Directory.CreateDirectory(destDir); }
            foreach (var sourceDir in Directory.GetDirectories(srcDir))
            {
                if (ignoreDirNames != null && ignoreDirNames.Any(d => string.Equals(d, new DirectoryInfo(sourceDir).Name, StringComparison.CurrentCulture))) { continue; }
                var targetDir = Path.Combine(destDir, new DirectoryInfo(sourceDir).Name);
                CopyDirectory(sourceDir, targetDir, ignoreDirNames);
            }
            foreach (var srcFile in Directory.GetFiles(srcDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(srcFile));
                System.IO.File.Copy(srcFile, destFile, true);
            }
        }

        /// <summary>
        /// 获取文件大小（带一位字母后缀：B、K、M、G）
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileLength(string file)
        {
            return new FileInfo(file).GetFileLength();
        }

        /// <summary>
        /// 计算文件MD5值
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileMD5(string file)
        {
            return new FileInfo(file).MD5File();
        }
    }
}
