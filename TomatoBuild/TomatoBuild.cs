using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TomatoBuild
{
    public class TomatoBuild
    {

        public static Dictionary<string, Project> projects = new Dictionary<string, Project>();

        public static void Main(string[] args)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(currentDirectory, "tomatobuild.json");

            Console.WriteLine("Searching for projects...");

            // load each project
            foreach(string file in files)
            {
                Project project = JsonConvert.DeserializeObject<Project>(File.ReadAllText(file));
                if(project.tomatobuild == 1)
                {
                    project.Init();
                    projects[project.project] = project;
                }
            }

            Console.Clear();

            while (args.Length == 0)
            {
                Console.WriteLine("Please enter a command");
                string command = Console.ReadLine();
                args = Util.SplitCommandLine(command).ToArray();
            }
            switch (args[0])
            {
                case "-p":
                case "--projects":
                    {
                        if (projects.Count == 0)
                        {
                            Console.WriteLine("No projects found");
                        }
                        else
                        {
                            Console.WriteLine("Projects found:");
                            foreach (var entry in projects)
                            {
                                Console.Write(entry.Value.project + " -> ");
                                foreach (var task in entry.Value.tasks)
                                {
                                    Console.Write(task.name + " ");
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                    break;
                case "-v":
                case "--version":
                    {
                        Console.WriteLine("TomatoBuild Version 1");
                    }
                    break;
                case "-wd":
                case "--working-directory":
                    {
                        Console.WriteLine(currentDirectory);
                    }
                    break;
                default:
                    {
                        // project command
                        if(!projects.ContainsKey(args[0]))
                        {
                            if(args.Length == 1 && projects.Count == 1)
                            {
                                string[] tasks = args[0].Split("|");
                                foreach(string task in tasks)
                                {
                                    if (projects.First().Value.HasTask(task))
                                    {
                                        projects.First().Value.RunTask(task);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid task '" + task + "'  in project '" + projects.First().Value.project + "'");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid project '" + args[0] + "'");
                            }
                        }else
                        {
                            // Get the task
                            if(args.Length == 2)
                            {
                                string[] tasks = args[1].Split("|");
                                foreach (string task in tasks)
                                {
                                    if (projects[args[0]].HasTask(task))
                                    {
                                        projects[args[0]].RunTask(task);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid task '" + task + "'  in project '" + args[0] + "'");
                                    }
                                }
                                
                            }
                            else
                            {
                                Console.WriteLine("Please specify a task");
                            }
                        }
                    }
                    break;
            }
        }
    }

}