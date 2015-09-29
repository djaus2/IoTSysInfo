using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Data.Json;

/// <summary>
/// The Commands class and the MainPage functions relating to it
/// </summary>
namespace SysInfo
{
    public class Commands
    {
        /// <summary>
        /// Create a new command with name and url only
        /// </summary>
        /// <param name="Name">As displayed on the command button</param>
        /// <param name="Url">Relative URL passed to web portal</param>
        public Commands(string Name, string Url)
        {
            name = Name;
            url = Url;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">As displayed on the command button</param>
        /// <param name="Url">Relative URL passed to web portal</param>
        /// <param name="Id">The property name used as ID at first level display when an array of items is returned. Default is .Name</param>
        /// <param name="Icon">The name of the icon to use in the menu. Defaults to Settings</param>
        public Commands(string Name, string Url, string Id, string Icon)
        {
            name = Name;
            url = Url;
            if ((Id != null) && (Id != ""))
                id = Id;
            if ((Icon!= null) && (Icon != ""))
                icon = Icon;
        }


        /// <summary>
        /// As displayed on the command button
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Relative URL passed to web portal. Appended to http://(device):8080
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// >The property name used as ID at first level display when an array of items is returned. Default is .Name
        /// </summary>
        public string id { get; set; } = ".Name";

        /// <summary>
        /// The name of the icon to use in the menu. Defaults to Settings.
        /// From the Windows.UI.Xaml.Controls.Symbol class
        /// </summary>
        public string icon { get; set; } = "Setting";

        /// <summary>
        /// The list of commands that is the DataContext for the commnds menu
        /// </summary>
        public static List<Commands> CommandsList { get; set; } = new List<Commands>();

    }

    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Called at startup to load the commands.
        /// Also called when tegh OS version changes
        /// </summary>
        /// <param name="CommandsVersion">Either Commands (Default) or CommandsV2(New version of OS)</param>
        private void GetCommands(string CommandsVersion)
        {
            JsonObject ResultData = null;
            using (StreamReader file = File.OpenText(".\\commands.json"))
            {
                String JSONData;

                //Get the stream as text.
                JSONData = file.ReadToEnd();

                //Convert to JSON object
                ResultData = (JsonObject)JsonObject.Parse(JSONData);
            }

            if (ResultData != null)
            {


                var commandsQry = from p in ResultData.GetNamedArray(CommandsVersion)
                                  select new Commands(
                                      p.GetObject().GetNamedString("command"),
                                      p.GetObject().GetNamedString("url"),
                                      p.GetObject().GetNamedString("id"),
                                      p.GetObject().GetNamedString("icon")
                                  );
                Commands.CommandsList = commandsQry.ToList<Commands>();

                //Some earlier tries at doin this:
                //==================================
                //var CommandsArray = ResultData.GetNamedArray("Commands");
                //for (int i = 0; i < CommandsArray.Count; i++)
                //{
                //    var oCommand = CommandsArray[i].GetObject();
                //    string command = oCommand.GetNamedString("command");
                //    string url = oCommand.GetObject().GetNamedString("url");
                //    string id = oCommand.GetObject().GetNamedString("id");

                //    Commands cmd = new Commands(command,url,id);
                //    Commands.CommandsList.Add(cmd);
                //}


                // var qw = ResultData.GetNamedArray("Commands");
                // for (int i = 0; i < qw.Count; i++)
                // {
                //     JsonObject jo = (JsonObject)qw[i];
                // }
                //// foreach (JsonObject jcmd in ResultData)
                // foreach (KeyValuePair<string, IJsonValue> jcmd in ResultData)
                // {
                //     Commands cmd = new Commands(jcmd);
                // }



            }

        }
    }

    }
