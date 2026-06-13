// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

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
