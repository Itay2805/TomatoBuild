using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TomatoBuild.ErrorMatchers;

namespace TomatoBuild.RebuildTriggers
{
    public class CHeaderRebuildTrigger : IRebuildTrigger
    {

        public static Regex cleanPath = new Regex("\\.+\\s+");

        public bool ShouldRebuild(Project project, Rule rule, Dictionary<string, object> options, string file)
        {
            string flags = options["gcc_flags"] as string;
            string output = Util.RunCommand("gcc -H -fsyntax-only " + Util.Preprocess(flags, rule.variables) + " " + file, new Dictionary<string, string>(), false);
            string[] paths = output.Split("\n");
            foreach(string p in paths)
            {
                // we do not care for anything after this
                if (p == "Multiple include guards may be useful for:") return false;
                string path = cleanPath.Replace(p, "");
                // ignore anything under usr, since it should be read-only
                if (path.StartsWith("/usr"))
                {
                    continue;
                }
                Console.WriteLine("Checking header " + path);
                if(project.meta.FileChanged(path))
                {
                    project.meta.UpdateFileLastChange(path);
                    return true;
                }
            }
            return false;
        }
    }
}
