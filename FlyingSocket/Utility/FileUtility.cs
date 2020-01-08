using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyingSocket.Utility
{
    public class FileUtility
    {
        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;
            FileStream fs = null;
            try
            {
                try
                {
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    inUse = false;
                }
                catch
                {
                    inUse = true;
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return inUse;
        }
    }
}
