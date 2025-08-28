//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by Spider Developer at 2020/3/29 11:05:05.
//  Version 1.0
//  Spider Developer
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cdy.Link.Mqtt.Develop
{
    public class Res
    {
        public static string Get(string name)
        {
            return Develop.Properties.Resources.ResourceManager.GetString(name, Thread.CurrentThread.CurrentUICulture);
        }
    }
}
