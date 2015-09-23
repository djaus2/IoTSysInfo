using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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

        Commands CurrentCmd { get; set; } = null;

        public MainPage()
        {
            this.InitializeComponent();
            CurrentCmd = null;
            GetCommands();
            NavLinksList.DataContext = Commands.CommandsList;

        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

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
            TextBox tb = (TextBox)sender;
            if (tb != null)
            {
                if (tb.Name == "textBoxAPI")
                    SysInfo.APIURL = textBoxAPI.Text;
                else if (tb.Name == "textBoxAPI_Params")
                    SysInfo.API_Params = textBoxAPI_Params.Text;
            }
        }

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double timeout = e.NewValue;
            SysInfo.Timeout = (int)timeout;
            timeout = timeout / 1000;
            textBoxTimeout.Text = timeout.ToString();
        }

        private async void NavLinksList_Tapped(object sender, TappedRoutedEventArgs e)
        {

            if (NavLinksList.SelectedIndex == -1)
                return;

            Commands cmd = null;
            string Command = "";
            //Get the command from the list item binding
            cmd = (Commands)NavLinksList.SelectedItem;
            if (cmd == null)
                return;
            Command = cmd.name;
            if (Command == "")
                return;

            bool exitNow = false;
                

            switch (Command)
            {
                case "localhost":
                case "minwinpc":
                case "192.168.0.28":
                    textBoxDevice.Text = Command;
                    exitNow = true;
                    break;
                case "api params clr":
                    textBoxAPI.Text = "";
                    textBoxAPI_Params.Text = "";
                    textBoxAppRelativeID.Text = "";
                    textBoxAppFullName.Text = "";
                    exitNow = true;
                    break;
                case "clear details":
                    NameValue.ClearList();
                    DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
                    exitNow = true;
                    break;
                case "cancel":
                    SysInfo.cts.Cancel();
                    exitNow = true;
                    break;
                case "stopapp":
                    DialogResult dr1 =  await ShowDialog("Stop App", "Do you wish to force the selecetded app to stop?", new List<DialogResult> { DialogResult.Yes, DialogResult.No,DialogResult.Cancel });
                    if (dr1 == DialogResult.Yes)
                        SysInfo.ForceStop = true;
                    else if (dr1 == DialogResult.No)
                        SysInfo.ForceStop = false;
                    else
                        exitNow = true;
                    break;
                case "shutdown":
                    DialogResult dr2 = await ShowDialog("Shutdown", "Do you wish to shutdown the system?", new List<DialogResult> { DialogResult.Yes, DialogResult.Cancel });
                    if (dr2 == DialogResult.Yes)
                    { }
                    else
                        exitNow = true;
                    break;
                case "reboot":
                    DialogResult dr3 = await ShowDialog("Reboot", "Do you wish reboot the system?", new List<DialogResult> { DialogResult.Yes,  DialogResult.Cancel });
                    if (dr3 == DialogResult.Yes)
                    { }
                    else
                        exitNow = true;
                    break;
            }
            if (exitNow)
                return;

            textBoxAPI.Text = Command;
            CurrentCmd = cmd;

            if (cmd.name == "api")
                cmd.url = textBoxAPI_Params.Text;


            //Show this in the MainPage URL textbox
            //API buttomn actions what ever is here.
            this.textBoxAPI.Text = cmd.url;
                NameValue.NameValues.Clear();
                DeviceInterfacesOutputList.DataContext = NameValue.NameValues;

                //Do the REST query and JSON parsing
                bool res = await SysInfo.DoQuery(cmd);

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
                    //This HAS been patterned (Version 2.2)
                    NameValue.NameValuesStack.Push(NameValue.NameValues);
                    string identity = cmd.id;
                                
                    //Get only the identity (name) record for each item
                    var nameValuesIds = from nv in NameValue.NameValues where nv.Name.Contains(identity) select nv;
                    NameValue.NameValues = nameValuesIds.ToList<NameValue>();
                }
            }

            DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
                
            


        }
        /// <summary>
        /// Drill into an item to get selected item's properties
        /// </summary>
        private void DeviceInterfacesOutputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (DeviceInterfacesOutputList.SelectedIndex == -1)
                return;
            
            //Name contains a dotted index. Need to select all such items from original list
            NameValue nv = (NameValue)DeviceInterfacesOutputList.SelectedItem;

            Copy(nv);

            string name = nv.Name;
            int index = name.IndexOf(" ");
            string indexStr = name.Substring(0, index);
            NameValue.NameValues = NameValue.NameValuesStack.Pop();
            var dt = from d in NameValue.NameValues where d.Name.Substring(0, index) == indexStr select d;

            //Could implement back button. ToDo would use:
            NameValue.NameValuesStack.Push(NameValue.NameValues);

            NameValue.NameValues = dt.ToList<NameValue>();
            DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
            if (name.Contains("InstalledPackages.Name"))
            {
                textBoxAppRelativeID.Text = NameValue.NameValues[0].Value;
                textBoxAppFullName.Text = NameValue.NameValues[3].Value;
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
           
            //StackPanel sp = (StackPanel)sender;
            //if (sp.DesiredSize.Height - 30 > 0)
            //{
            //    splitView1.Height = sp.DesiredSize.Height - 30;
            //    SP1.Height = 600;
            //}
        }

        private void Copy(NameValue nv)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage dp =
                new Windows.ApplicationModel.DataTransfer.DataPackage();
            dp.SetText(nv.Value);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
            textBoxAPI_Params.Text = nv.Value;
        }

        private void textBoxAppRelativeID_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.RelAppId = textBoxAppRelativeID.Text;
        }

        private void textBoxAAppFullName_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.FullAppName = textBoxAppFullName.Text;
        }

        enum DialogResult
        {
            Yes,No,OK,Cancel
        }

        private async Task<DialogResult> ShowDialog(string Title, string Message, List<DialogResult> buttons)
        {
            DialogResult res = DialogResult.OK;

            var dialog = new MessageDialog(Message);
            dialog.Title = Title;
            if (buttons.Contains(DialogResult.Yes))
                dialog.Commands.Add(new UICommand { Label = "Yes", Id = DialogResult.Yes });
            if (buttons.Contains(DialogResult.No))
                dialog.Commands.Add(new UICommand { Label = "No", Id = DialogResult.No });
            if (buttons.Contains(DialogResult.OK))
                dialog.Commands.Add(new UICommand { Label = "OK", Id = DialogResult.OK });
            if (buttons.Contains(DialogResult.Cancel))
                dialog.Commands.Add(new UICommand { Label = "Cancel", Id = DialogResult.Cancel });

            var rebootRes = await dialog.ShowAsync();

            res = (DialogResult) rebootRes.Id;

            
            return res;
        }
    }
}
