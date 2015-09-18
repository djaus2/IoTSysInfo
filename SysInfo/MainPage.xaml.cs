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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SysInfo
{
    /// <summary>
    /// Based upon Bruce Eitman's blog:
    /// http://embedded101.com/BruceEitman/entryid/676/Windows-10-IoT-Core-Getting-the-MAC-Address-from-Raspberry-Pi.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        ///Used for drilling into list items.
        ///Some query response lists are simplified by only showing one item per entity.
        ///Eg Processes returns multiple properties for a process
        ///Only show the ImageName/Decription/Name propertty then allow to drill in for other properties by clicking on the item.
        /// </summary>
        private enum Instate
        {
            None,
            Ipconfig, //eg. Have listed network devices
            Ipconfig_Aninterface, //eg. Have selected a network device and displayed its properties
            Processes,
            Processes_Aprocess,
            Devices,
            Devices_Adevice,
            Providers,
            Providers_Aprovider
        }
        Instate SystemState = Instate.None;

        
        public MainPage()
        {
            this.InitializeComponent();
            SystemState = Instate.None;
        }

        private void buttonSelectTarget_Click(object sender, RoutedEventArgs e)
        {
            Button buttonTemp = (Button)sender;
            textBoxDevice.Text = buttonTemp.Content.ToString();
        }

        private void textBoxTargetDeviceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.Device = textBoxDevice.Text;
        }

        private void textBoxPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.Port = textBoxPort.Text;
        }

        private void textBoxUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.Admin = textBoxUser.Text;
        }

        private void textBoxPwd_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.Pwd = textBoxPwd.Text;
        }

        private string lastButton = "";
        /// <summary>
        /// Action a command
        /// </summary>
        /// <param name="sender">The command button. Its content(text) is the actual parameter</param>
        private async void button_Click_1(object sender, RoutedEventArgs e)
        {
            //Get the "actual" parameter from button content
            Button button = (Button)sender;
            string buttonText = ((string)button.Content).ToLower();

            if (buttonText == "clear params")
            {
                textBoxAPI_Params.Text="";
                return;
            }

            if (buttonText != lastButton)
            {
                //Do anything required when a different command is pressed ??
                lastButton = buttonText;
            }

            //New command so clear everything
            SystemState = Instate.None;
            NameValue.NameValuesStack.Clear();
            DeviceInterfacesOutputList.SelectedIndex = -1;
            //Th erest commands can use parameters
            SysInfo.API_Params = textBoxAPI_Params.Text;

            //Lookup the command's REST URL
            string url = "";
            switch (buttonText)
            {
                case "ipconfig":
                    url = SysInfo.IpConfigURL;
                    break;
                case "sysinfo":
                    url = SysInfo.SysInfoURLRL;
                    break;
                case "api":
                    url = SysInfo.APIURL;
                    break;
                case "osapi":
                    url = SysInfo.OSAPILRL;
                    break;
                case "devices":
                    url = SysInfo.DevicesURL;
                    break;
                case "processes":
                    url = SysInfo.ProcessesURL;
                    break;
                case "default app":
                    url = SysInfo.DefaulAppURL;
                    break;
                case "providers":
                    url = SysInfo.ProvidersURL;
                    break;
            }

            //Show this in the MainPage URL textbox
            //API buttomn actions what ever is here.
            this.textBoxAPI.Text = url;

            //Do the REST query and JSON parsing
            bool res = await SysInfo.DoQuery(url);

            //If not OK then only show a generic error message
            if (!res)
            {
                NameValue.ClearList();
                NameValue nv = new NameValue("Error:", "Target not found, timeout or processing error.");
            }
            else
            {
                //If the query response list is from an array simplify by only showing one entry in the list per item
                //ie Only show the item name/description etc.
                if (NameValue.NameValues_IsFrom_Array)
                {
                    switch (buttonText)
                    {
                        //ToDo: Could pattern this.
                        case "ipconfig":
                            NameValue.NameValuesStack.Push(NameValue.NameValues);
                            var dt0 = from d0 in NameValue.NameValues where d0.Name.Contains("Description") select d0;
                            NameValue.NameValues = dt0.ToList<NameValue>();
                            SystemState = Instate.Processes;
                            break;
                        case "processes":
                            NameValue.NameValuesStack.Push(NameValue.NameValues);
                            var dt = from d in NameValue.NameValues where d.Name.Contains("ImageName") select d;
                            NameValue.NameValues = dt.ToList<NameValue>();
                            SystemState = Instate.Processes;
                            break;
                        case "devices":
                            NameValue.NameValuesStack.Push(NameValue.NameValues);
                            var dt2 = from d2 in NameValue.NameValues where d2.Name.Contains("Description") select d2;
                            NameValue.NameValues = dt2.ToList<NameValue>();
                            SystemState = Instate.Devices;
                            break;
                        case "providers":
                            NameValue.NameValuesStack.Push(NameValue.NameValues);
                            var dt3 = from d3 in NameValue.NameValues where d3.Name.Contains("Name") select d3;
                            NameValue.NameValues = dt3.ToList<NameValue>();
                            SystemState = Instate.Devices;
                            break;
                    }
                }
            }

            DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
        }

        /// <summary>
        /// Can provide parameters to URL
        /// </summary>
        private void textBoxAPI_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.APIURL = textBoxAPI.Text;
        }

        /// <summary>
        /// Slider sets the query timeout 0.5 to 20sec.
        /// </summary>
        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double timeout = e.NewValue;
            SysInfo.Timeout = (int) timeout;
            timeout = timeout / 1000;
            textBoxTimeout.Text = timeout.ToString();
        }

        /// <summary>
        /// Cancel the query
        /// </summary>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            SysInfo.cts.Cancel();
        }

        /// <summary>
        /// Drill into an item to get selected item's properties
        /// </summary>
        private void DeviceInterfacesOutputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (SystemState)
            {
                case Instate.Ipconfig:
                case Instate.Processes:
                case Instate.Devices:
                case Instate.Providers:
                    //This HAS been patterned:
                    //Name contains a dotted index. Need to select all such items from original list
                    NameValue nv = (NameValue)DeviceInterfacesOutputList.SelectedItem;
                    string name = nv.Name;
                    int index = name.IndexOf(" ");
                    string indexStr = name.Substring(0,index);
                    NameValue.NameValues = NameValue.NameValuesStack.Pop();
                    var dt = from d in NameValue.NameValues where d.Name.Substring(0,index) == indexStr select d;
                    
                    //Could implement back button. ToDo would use:
                    NameValue.NameValuesStack.Push(NameValue.NameValues);

                    NameValue.NameValues = dt.ToList<NameValue>();
                    switch (SystemState)
                    {
                        case Instate.Ipconfig:
                            SystemState = Instate.Ipconfig_Aninterface;
                            break;
                        case Instate.Processes:
                            SystemState = Instate.Processes_Aprocess;
                            break;
                        case Instate.Devices:
                            SystemState = Instate.Devices_Adevice;
                            break;
                        case Instate.Providers:
                            SystemState = Instate.Providers_Aprovider;
                            break;
                    }
                    DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
                    break;
            }
        }
    }
}
