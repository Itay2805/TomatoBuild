using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace TomatoBuild
{
    public class Util
    {
        private static Regex variablesReplace = new Regex(@"\$([a-zA-Z_]+)");

        public static string Preprocess(string text, Dictionary<string, string> variables)
        {
            return variablesReplace.Replace(text, match => variables[match.Groups[1].Value]);
        }

        public const char ESCAPE = '';

        /// <summary>
        /// Super dumb function to print text with ansi color escapes :shrug:
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string PrintAnsiEscaped(string str)
        {
            if(str == null)
            {
                return "";
            }

            bool inEscape = false;
            for(int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if(!inEscape)
                {
                    if (c == ESCAPE)
                    {
                        inEscape = true;
                    }
                    else
                    {
                        Console.Write(str[i]);
                    }
                }else
                {
                    int start = i;

                    string escape = "";
                    while(i < str.Length && (str[i] != 'm' && str[i] != 'K'))
                    {
                        escape += str[i++];
                    }

                    if(i >= str.Length)
                    {
                        return str.Substring(start);
                    }
                    
                    if(str[i] == 'm')
                    {
                        escape = escape.Substring(1);

                        switch (escape)
                        {
                            case "0;30": case "30": Console.ForegroundColor = ConsoleColor.Black; break;
                            case "0;31": case "31": Console.ForegroundColor = ConsoleColor.DarkRed; break;
                            case "0;32": case "32": Console.ForegroundColor = ConsoleColor.DarkGreen; break;
                            case "0;33": case "33": Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                            case "0;34": case "34": Console.ForegroundColor = ConsoleColor.DarkBlue; break;
                            case "0;35": case "35": Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                            case "0;36": case "36": Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                            case "0;37": case "37": Console.ForegroundColor = ConsoleColor.Gray; break;

                            case "01;30": case "1;30": Console.ForegroundColor = ConsoleColor.DarkGray; break;
                            case "01;31": case "1;31": Console.ForegroundColor = ConsoleColor.Red; break;
                            case "01;32": case "1;32": Console.ForegroundColor = ConsoleColor.Green; break;
                            case "01;33": case "1;33": Console.ForegroundColor = ConsoleColor.Yellow; break;
                            case "01;34": case "1;34": Console.ForegroundColor = ConsoleColor.Blue; break;
                            case "01;35": case "1;35": Console.ForegroundColor = ConsoleColor.Magenta; break;
                            case "01;36": case "1;36": Console.ForegroundColor = ConsoleColor.Cyan; break;

                            case "1":
                            case "01":
                            case "01;37": case "1;37": Console.ForegroundColor = ConsoleColor.White; break;

                            case "0;40": case "40": Console.BackgroundColor = ConsoleColor.Black; break;
                            case "0;41": case "41": Console.BackgroundColor = ConsoleColor.DarkRed; break;
                            case "0;42": case "42": Console.BackgroundColor = ConsoleColor.DarkGreen; break;
                            case "0;43": case "43": Console.BackgroundColor = ConsoleColor.DarkYellow; break;
                            case "0;44": case "44": Console.BackgroundColor = ConsoleColor.DarkBlue; break;
                            case "0;45": case"45": Console.BackgroundColor = ConsoleColor.DarkMagenta; break;
                            case "0;46": case "46": Console.BackgroundColor = ConsoleColor.DarkCyan; break;
                            case "0;47": case "47": Console.BackgroundColor = ConsoleColor.Gray; break;

                            case "01;40": case "1;40": Console.BackgroundColor = ConsoleColor.DarkGray; break;
                            case "01;41": case "1;41": Console.BackgroundColor = ConsoleColor.Red; break;
                            case "01;42": case "1;42": Console.BackgroundColor = ConsoleColor.Green; break;
                            case "01;43": case "1;43": Console.BackgroundColor = ConsoleColor.Yellow; break;
                            case "01;44": case "1;44": Console.BackgroundColor = ConsoleColor.Blue; break;
                            case "01;45": case "1;45": Console.BackgroundColor = ConsoleColor.Magenta; break;
                            case "01;46": case "1;46": Console.BackgroundColor = ConsoleColor.Cyan; break;
                            case "01;47": case "1;47": Console.BackgroundColor = ConsoleColor.White; break;

                            case "":
                            case "0": Console.ResetColor(); break;
                            
                            default: Console.Write(ESCAPE + "[" + escape + "m"); break;
                        }

                    }else if(str[i] == 'K')
                    {
                        // TODO: wtf is that
                    }

                    inEscape = false;
                }
            }

            Console.WriteLine();

            return "";
        }

        public static string RunCommand(string command, Dictionary<string, string> enviroment)
        {
            Process process = new Process();
            foreach(var variable in enviroment)
            {
                process.StartInfo.EnvironmentVariables[variable.Key] = variable.Value;
            }
            process.StartInfo.FileName = "bash";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            string output = "";
            string lastOut = "";
            string lastError = "";
            process.OutputDataReceived += (sender, data) => {
                if (data.Data == null) return;
                lastOut = PrintAnsiEscaped(lastOut + data.Data);
                output += data.Data;
            };
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += (sender, data) => {
                if (data.Data == null) return;
                lastError = PrintAnsiEscaped(lastError + data.Data);
                output += data.Data;
            };
            
            command = command.Replace("\"", "\"\"");
            process.StartInfo.Arguments = "-c \"" + command + "\"";

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return output;
        }

        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;

            return commandLine.Split(c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            }).Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
              .Where(arg => !string.IsNullOrEmpty(arg));
        }


    }
}
