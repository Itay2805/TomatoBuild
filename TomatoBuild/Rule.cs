using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using TomatoBuild.ErrorMatchers;
using TomatoBuild.RebuildTriggers;

namespace TomatoBuild
{
    public class Rule
    {

        public string input_regex;
        public string output_file;
        public List<Dictionary<string, object>> rebuild_triggers = new List<Dictionary<string, object>>();
        public Dictionary<string, string> variables = new Dictionary<string, string>();
        public Dictionary<string, string> enviroment = new Dictionary<string, string>();
        public string type;
        public string command;
        public bool dir_recursive;
        public bool force_rebuild;
        public string error_matcher;

        private Regex inputRegex;
        private IErrorMatcher errorMatcher;

        public void Init()
        {
            inputRegex = new Regex(input_regex);
            if(!string.IsNullOrEmpty(error_matcher))
            {
                if(!ErrorMatcherManager.matchers.ContainsKey(error_matcher))
                {
                    Console.WriteLine("Invalid error matcher `" + error_matcher + "`");
                    Environment.Exit(0);
                }else
                {
                    errorMatcher = ErrorMatcherManager.matchers[error_matcher];
                }
            }
        }

        public bool Run(Project project)
        {
            List<string> files = new List<string>();

            // TODO: this foreach loop can be done in parallel
            foreach (string file in Directory.GetFiles(project.input_folder, "*", dir_recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                string f = file.Replace("\\", "/");

                if (inputRegex.IsMatch(f))
                {
                    string pathToFile = Path.Combine(project.output_folder, file.Replace(project.input_folder, ""));
                    string newFile = inputRegex.Replace(pathToFile, output_file);

                    // if the file has changed, or force rebuild is set or the output file does not exists it will rebuild
                    if (project.meta.FileChanged(f) || force_rebuild || !File.Exists(newFile))
                    {
                        files.Add(f);
                    }
                    else
                    {
                        foreach(var rebuildTrigger in rebuild_triggers)
                        {
                            string type = rebuildTrigger["type"] as string;
                            if(RebuildTriggerManager.matchers.ContainsKey(type))
                            {
                                if(RebuildTriggerManager.matchers[type].ShouldRebuild(project, this, rebuildTrigger, f))
                                {
                                    // Rebuild trigger activated, add the fileand break (since we don't want to add it multiple times)
                                    files.Add(f);
                                    break;
                                }
                            }else
                            {
                                Console.WriteLine("Invalid rebuild trigger `" + type + "`");
                                Environment.Exit(0);
                            }
                        }
                    }
                }
            }

            switch(type)
            {
                case "per_file":
                    {
                        // TODO: this foreach loop can be done in parallel
                        foreach(string file in files)
                        {
                            string pathToFile = Path.Combine(project.output_folder, file.Replace(project.input_folder, ""));
                            string newFile = inputRegex.Replace(pathToFile, output_file);

                            Directory.CreateDirectory(Path.GetDirectoryName(newFile));

                            variables["input_file"] = file;
                            variables["output_file"] = newFile;

                            string output = Util.RunCommand(Util.Preprocess(command, variables), enviroment);
                            
                            if(errorMatcher != null && errorMatcher.HasError(output))
                            {
                                return false;
                            }else
                            {
                                Console.WriteLine(file + " -> " + newFile);
                                project.meta.UpdateFileLastChange(file);
                            }
                        }
                    } break;
                case "files_as_args":
                    {
                        string args = string.Join(' ', files);
                        variables["input_file"] = args;
                        variables["output_file"] = Path.Combine(project.output_folder, output_file);

                        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(project.output_folder, output_file)));

                        string output = Util.RunCommand(Util.Preprocess(command, variables), enviroment);

                        if(errorMatcher != null && errorMatcher.HasError(output))
                        {
                            return false;
                        }
                        else {
                            foreach(string file in files)
                            {
                                project.meta.UpdateFileLastChange(file);
                            }
                        }
                    }
                    break;
                default:
                    {
                        Console.WriteLine("Invalid rule type: `" + type + "` (avaiable options: `per_file`, `files_as_args`)");
                        return false;
                    }
            }

            return true;
        }

    }
}
