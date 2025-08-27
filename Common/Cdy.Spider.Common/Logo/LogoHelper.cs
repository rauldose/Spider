//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by Spider Developer at 2020/5/26 12:44:42.
//  Version 1.0
//  Spider Developer
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cdy.Spider
{
    public static class LogoHelper
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// 
        /// </summary>
        public static void Print()
        {
            //"Cdy.Tag.Common.Logo.Logo.txt"
            Console.WriteLine(new StreamReader(typeof(LogoHelper).Assembly.GetManifestResourceStream("Cdy.Spider.Common.Logo.Logo.txt")).ReadToEnd());
        }

        /// <summary>
        /// 
        /// </summary>
        public static void PrintAuthor()
        {
            Console.WriteLine(new StreamReader(typeof(LogoHelper).Assembly.GetManifestResourceStream("Cdy.Spider.Common.Logo.Author.txt")).ReadToEnd());
            Console.WriteLine("Created by chongdaoyang.Powered by dotnet core.");
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
