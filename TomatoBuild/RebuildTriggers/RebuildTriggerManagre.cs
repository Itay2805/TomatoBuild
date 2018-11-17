using System;
using System.Collections.Generic;
using System.Text;

namespace TomatoBuild.RebuildTriggers
{
    public class RebuildTriggerManager
    {

        public static Dictionary<string, IRebuildTrigger> matchers = new Dictionary<string, IRebuildTrigger>();

        static RebuildTriggerManager()
        {
            matchers["c_header_changed"] = new CHeaderRebuildTrigger();
        }

    }

}
