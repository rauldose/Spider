//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by Spider Developer at 2020/8/11 15:23:05.
//  Version 1.0
//  Spider Developer
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InSpiderRun
{
    public class Res
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Get(string name)
        {
            return Properties.Resources.ResourceManager.GetString(name, Thread.CurrentThread.CurrentCulture);
        }
    }
}
