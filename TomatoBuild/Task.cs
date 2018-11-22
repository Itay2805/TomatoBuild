using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TomatoBuild
{
    public class Task
    {

        public string name;
        
        public Dictionary<string, string> variables = new Dictionary<string, string>();
        public Dictionary<string, string> enviroment = new Dictionary<string, string>();
        public List<string> pre_commands = new List<string>();
        public List<Rule> rules = new List<Rule>();
        public List<string> post_commands = new List<string>();

        public void Init()
        {
            foreach (Rule rule in rules)
            {
                foreach (var entry in variables)
                {
                    rule.variables[entry.Key] = entry.Value;
                }
                foreach (var entry in enviroment)
                {
                    rule.enviroment[entry.Key] = entry.Value;
                }
                rule.variables["task"] = name;

                rule.Init();
            }
        }

        public bool Run(Project project)
        {
            foreach(string pre_command in pre_commands)
            {
                // preprocess command

                string output = Util.RunCommand(Util.Preprocess(pre_command, variables), enviroment, project.path);
                // TODO: Error matching
            }

            foreach (Rule rule in rules)
            {
                if (!rule.Run(project)) return false;
            }

            foreach (string post_command in post_commands)
            {
                string output = Util.RunCommand(Util.Preprocess(post_command, variables), enviroment, project.path);
                // TODO: Error matching
            }

            return true;
        }

    }
}
