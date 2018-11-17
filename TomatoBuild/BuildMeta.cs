using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TomatoBuild
{
    public class BuildMeta
    {

        public Dictionary<string, DateTime> lastModifyTable = new Dictionary<string, DateTime>();

        public bool FileChanged(string path)
        {
            path = Path.GetFullPath(path);
            if (!lastModifyTable.ContainsKey(path))
            {
                return true;
            }
            DateTime lastModify = File.GetLastWriteTime(path);
            return lastModifyTable[path] < lastModify;
        }

        public void UpdateFileLastChange(string path)
        {
            lastModifyTable[Path.GetFullPath(path)] = File.GetLastWriteTime(path);
        }

        // TODO: JsonH looks promising to reduce some of the file size

        public static BuildMeta Load(Project project)
        {
            BuildMeta meta = new BuildMeta();

            string filemodifyjson = Path.Combine(project.output_folder, "tomatobuild.filemodify.json");
            if (File.Exists(filemodifyjson))
            {
                meta.lastModifyTable = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(File.ReadAllText(filemodifyjson));
            }

            return meta;
        }

        public void Save(Project project)
        {
            if(Directory.Exists(project.output_folder))
            {
                string filemodifyjson = Path.Combine(project.output_folder, "tomatobuild.filemodify.json");
                File.WriteAllText(filemodifyjson, JsonConvert.SerializeObject(lastModifyTable));
            }
        }

    }
}
