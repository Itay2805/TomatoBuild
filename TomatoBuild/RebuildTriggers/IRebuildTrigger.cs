using System;
using System.Collections.Generic;
using System.Text;

namespace TomatoBuild.RebuildTriggers
{
    public interface IRebuildTrigger
    {

        bool ShouldRebuild(Project project, Rule rule, Dictionary<string, object> options, string file);

    }
}
