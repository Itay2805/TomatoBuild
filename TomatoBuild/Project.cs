using System.Collections.Generic;

namespace TomatoBuild
{
    public class Project
    {

        public string path = "";

        public int tomatobuild;
        public string project;
        public string input_folder;
        public string output_folder;
        public Dictionary<string, string> variables = new Dictionary<string, string>();
        public Dictionary<string, string> enviroment = new Dictionary<string, string>();
        public List<Task> tasks = new List<Task>();

        public BuildMeta meta;

        public void Init()
        {
            foreach (Task task in tasks)
            {
                foreach (var entry in variables)
                {
                    task.variables[entry.Key] = entry.Value;
                }
                foreach (var entry in enviroment)
                {
                    task.enviroment[entry.Key] = entry.Value;
                }
                task.variables["output_folder"] = output_folder;
                task.variables["input_folder"] = input_folder;
                task.variables["project"] = project;
                task.Init();
            }

            meta = BuildMeta.Load(this);
        }

        public bool RunTask(string name)
        {
            foreach (Task task in tasks)
            {
                if (task.name == name)
                {
                    bool result = task.Run(this);
                    meta.Save(this);
                    return result;
                }
            }

            return false;
        }

        public bool HasTask(string name)
        {
            foreach (Task task in tasks)
            {
                if (task.name == name) return true;
            }
            return false;
        }

    }
}
