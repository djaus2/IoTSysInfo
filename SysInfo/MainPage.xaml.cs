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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
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
            Providers_Aprovider,
            Packages,
            Packages_Apackage,
            /* [1] Adding a new query where the enity is an array
            newquery,
            Newquery_Aquery
            */
        }
        Instate SystemState = Instate.None;


        public MainPage()
        {
            this.InitializeComponent();
            SystemState = Instate.None;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SplitViewButton_Click(object sender, RoutedEventArgs e)
        {
            /*splitView1.IsPaneOpen = !splitView1.IsPaneOpen;
            if (splitView1.IsPaneOpen)
            {
                CommandsTextBlock.Visibility = Visibility.Visible;
                Filler1.Width = 150;
            }
            else
            {
                CommandsTextBlock.Visibility = Visibility.Collapsed;
                Filler1.Width = 10;
            }*/

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

        private void textBoxAPI_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.APIURL = textBoxAPI.Text;
        }

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double timeout = e.NewValue;
            SysInfo.Timeout = (int)timeout;
            timeout = timeout / 1000;
            textBoxTimeout.Text = timeout.ToString();
        }

        private async void NavLinksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem lvi;
            StackPanel sp;
            UIElementCollection tb;
            TextBlock tbx = new TextBlock();
            string Command = "";
            if (NavLinksList.SelectedIndex != -1)
            {
                lvi = (ListViewItem)NavLinksList.SelectedItem;
                sp = (StackPanel)lvi.Content;
                tb = sp.Children;
                foreach (var v in tb)
                {
                    if (v.GetType() == tbx.GetType())
                    {
                        tbx = (TextBlock)v;
                        Command = tbx.Text;
                        break;
                    }
                }
            }

            if (Command != "")
            {
                bool exitNow = false;
                switch (Command)
                {
                    case "target":
                        exitNow=true;
                        break;
                    case "localhost":
                        textBoxDevice.Text = Command;
                        exitNow = true;
                        break;
                    case "minwinpc":
                        textBoxDevice.Text = Command;
                        exitNow = true;
                        break;
                    case "192.168.0.28":
                        textBoxDevice.Text = Command;
                        exitNow = true;
                        break;
                    case "sysinfo":
                        textBoxAPI.Text = Command;
                        break;
                    case "osapi":
                        textBoxAPI.Text = Command;
                        break;
                    case "ipconfig":
                        textBoxAPI.Text = Command;
                        break;
                    case "devices":
                        textBoxAPI.Text = Command;
                        break;
                    case "processes":
                        textBoxAPI.Text = Command;
                        break;
                    case "providers":
                        textBoxAPI.Text = Command;
                        break;
                    case "packages":
                        textBoxAPI.Text = Command;
                        break;
                    case "api":
                        textBoxAPI.Text = Command;
                        break;
                    case "clr api params":
                        textBoxAPI_Params.Text = "";
                        break;
                    /* [2] Add a new query
                    case "newquery":
                        textBoxAPI_Params.Text = "";
                        break;
                   */
                }

                if (exitNow)
                    return;
                string url = "";
                switch (Command)
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
                    case "packages":
                        url = SysInfo.PackagesURL;
                        break;
                    /* [3] Add a new query
                    case "newquery":
                        url = SysInfo.NewqueryURL;
                        break;
                    */
                    }

                    if (url != "")
                    {
                        //Show this in the MainPage URL textbox
                        //API buttomn actions what ever is here.
                        this.textBoxAPI.Text = url;
                        NameValue.NameValues.Clear();
                        DeviceInterfacesOutputList.DataContext = NameValue.NameValues;

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
                            DetailsTextBlock.Text = Command;

                            //If the query response list is from an array simplify by only showing one entry in the list per item
                            //ie Only show the item name/description etc.
                            if (NameValue.NameValues_IsFrom_Array)
                            {
                                switch (Command)
                                {
                                    //ToDo: Could pattern this.
                                    case "ipconfig":
                                        NameValue.NameValuesStack.Push(NameValue.NameValues);
                                        var dt0 = from d0 in NameValue.NameValues where d0.Name.Contains(".Description") select d0;
                                        NameValue.NameValues = dt0.ToList<NameValue>();
                                        SystemState = Instate.Processes;
                                        break;
                                    case "processes":
                                        NameValue.NameValuesStack.Push(NameValue.NameValues);
                                        var dt = from d in NameValue.NameValues where d.Name.Contains(".ImageName") select d;
                                        NameValue.NameValues = dt.ToList<NameValue>();
                                        SystemState = Instate.Processes;
                                        break;
                                    case "devices":
                                        NameValue.NameValuesStack.Push(NameValue.NameValues);
                                        var dt2 = from d2 in NameValue.NameValues where d2.Name.Contains(".Description") select d2;
                                        NameValue.NameValues = dt2.ToList<NameValue>();
                                        SystemState = Instate.Devices;
                                        break;
                                    case "providers":
                                        NameValue.NameValuesStack.Push(NameValue.NameValues);
                                        var dt3 = from d3 in NameValue.NameValues where d3.Name.Contains(".Name") select d3;
                                        NameValue.NameValues = dt3.ToList<NameValue>();
                                        SystemState = Instate.Devices;
                                        break;
                                    case "packages":
                                        NameValue.NameValuesStack.Push(NameValue.NameValues);
                                        var dt4 = from d4 in NameValue.NameValues where d4.Name.Contains(".Name") select d4;
                                        NameValue.NameValues = dt4.ToList<NameValue>();
                                        SystemState = Instate.Packages;
                                        break;
                                    /* [4] Add a new query where top level returns an array of entities
                                    case "newquery:
                                        NameValue.NameValuesStack.Push(NameValue.NameValues);
                                        var dtn = from dn in NameValue.NameValues where dn.Name.Contains(".Name"- or ".Description" etc need the dot) select dn;
                                        NameValue.NameValues = dtn.ToList<NameValue>();
                                        SystemState = Instate.Packages;
                                        break;*/
                                }
                          }
                    }

                    DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
                }
            }


        }
        /// <summary>
        /// Drill into an item to get selected item's properties
        /// </summary>
        private void DeviceInterfacesOutputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceInterfacesOutputList.SelectedIndex == 1)
                return;
            switch (SystemState)
            {
                case Instate.Ipconfig:
                case Instate.Processes:
                case Instate.Devices:
                case Instate.Providers:
                case Instate.Packages:
                /* [5] Add a new query
                case Instate.Newquery:
                */
                    //This HAS been patterned:
                    //Name contains a dotted index. Need to select all such items from original list
                    NameValue nv = (NameValue)DeviceInterfacesOutputList.SelectedItem;
                    string name = nv.Name;
                    int index = name.IndexOf(" ");
                    string indexStr = name.Substring(0, index);
                    NameValue.NameValues = NameValue.NameValuesStack.Pop();
                    var dt = from d in NameValue.NameValues where d.Name.Substring(0, index) == indexStr select d;

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
                        case Instate.Packages:
                            SystemState = Instate.Packages_Apackage;
                            break;
                        /* [6] Add a new query where top level returns an array of entities
                        case Instate.Newquery:
                            SystemState = Instate.Newquery_Anewquery;
                            break;
                        */
                    }
                    DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
                    break;
            }

        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            string command = tb.Text;

            switch (command)
            {
                case "clear":
                    NameValue.ClearList();
                    DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
                    break;
                case "cancel":
                    SysInfo.cts.Cancel();
                    break;
            }
        }


        private void DetailsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NameValue.ClearList();
            DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
        }

        private void SettingsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SP1.Visibility == Visibility.Visible)
            {
                SP1.Visibility = Visibility.Collapsed;

                Filler2.Width = 10;

            }
            else
            {
                SP1.Visibility = Visibility.Visible;
                Filler2.Width = 300;
            }
            SettingsTextBlock.Visibility = SP1.Visibility;
        }

        private void CommandIcon_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            splitView1.IsPaneOpen = !splitView1.IsPaneOpen;
            if (splitView1.IsPaneOpen)
            {
                CommandsTextBlock.Visibility = Visibility.Visible;
                Filler1.Width = 150;
            }
            else
            {
                CommandsTextBlock.Visibility = Visibility.Collapsed;
                Filler1.Width = 10;
            }
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            StackPanel sp = (StackPanel)sender;
            if (sp.DesiredSize.Height - 30 > 0)
            {
                splitView1.Height = sp.DesiredSize.Height - 30;
                SP1.Height = 600;
            }
        }
    }
}
