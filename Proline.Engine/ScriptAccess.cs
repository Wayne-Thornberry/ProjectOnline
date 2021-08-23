﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proline.Engine
{
   internal static class ScriptAccess
    {
        internal static void StartStartupScripts()
        {
            var sm = InternalManager.GetInstance();
            foreach(ScriptPackage package in sm.GetPackages())
            {
                package.StartStartupScripts();
            }
        }
    }
}
