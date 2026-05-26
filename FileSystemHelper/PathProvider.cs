using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSSHDSever
{
    public class PathProvider
    {

        public static string GetWinAppPath()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;

            return appPath;
        }

    }
}
