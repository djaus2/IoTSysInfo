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


namespace SysInfo
{
    public sealed partial class MainPage : Page
    {       
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

    public class Commands
    {
        public Commands(string Name, string Url)
        {
            name = Name;
            url = Url;
        }
        public Commands(string Name, string Url, string Id, string Icon)
        {
            name = Name;
            url = Url;
            if ((Id != null) && (Id != ""))
                id = Id;
            if ((Icon!= null) && (Icon != ""))
                icon = Icon;
        }



        public string name { get; set; }
        public string url { get; set; }

        public string id { get; set; } = ".Name";

        public string icon { get; set; } = "Setting";

        public static List<Commands> CommandsList { get; set; } = new List<Commands>();

    }
}
