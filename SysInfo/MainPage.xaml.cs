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
            GetCommands("Commands");
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
                case "":
                    exitNow = true;
                    break;
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
                    DeviceInterfacesOutputList_DataContextRefresh();
                    exitNow = true;
                    break;
                case "cancel":
                    SysInfo.cts.Cancel();
                    exitNow = true;
                    break;
                case "startapp":
                    break;
                case "stopapp":
                    if (((bool)checkBoxAppForceStop.IsChecked) || (SysInfo.IsOSVersuion10_0_10531))
                    {
                        DialogResult dr0 = await ShowDialog("Stop App", "Do you wish to stop the selected app?", new List<DialogResult> { DialogResult.Yes, DialogResult.Cancel });
                        if (dr0 == DialogResult.Yes)
                        { }
                        else
                            exitNow = true;
                    }
                    else
                    {
                        DialogResult dr1 = await ShowDialog("Stop App", "Do you wish to force the seleceted app to stop?", new List<DialogResult> { DialogResult.Yes, DialogResult.No, DialogResult.Cancel });
                        if (dr1 == DialogResult.Yes)
                            SysInfo.ForceStop = true;
                        else if (dr1 == DialogResult.No)
                            SysInfo.ForceStop = false;
                        else
                            exitNow = true;
                    }
                    break;
                case "sysinfo":
                    break;
                case "shutdown":
                    DialogResult dr2 = await ShowDialog("Shutdown", "Do you wish shutdown the system?", new List<DialogResult> { DialogResult.Yes, DialogResult.Cancel });
                    if (dr2 == DialogResult.Yes)
                    { }
                    else
                        exitNow = true;
                    break;
                case "reboot":
                    DialogResult dr3 = await ShowDialog("Reboot", "Do you wish reboot the system?", new List<DialogResult> { DialogResult.Yes, DialogResult.Cancel });
                    if (dr3 == DialogResult.Yes)
                    { }
                    else
                        exitNow = true;
                    break;
                case "default_app":
                    //url changed between versions
                    break;
                case "packages":
                    //url changed between versions
                    break;           
                case "packageUninstall":
                    //url changed between versions
                    DialogResult dr4 = await ShowDialog("Uninstall package", "Do you wish to uninstall the select package?", new List<DialogResult> { DialogResult.Yes, DialogResult.Cancel });
                    if (dr4 == DialogResult.Yes)
                    {
                    }
                    else
                        exitNow = true;
                    break;
                case "packageInstall":
                    DialogResult dr7 = await ShowDialog("Install package", "This command not yet implemented", new List<DialogResult> { DialogResult.OK });
                    exitNow = true;
                    break;
                case "packageInstallSelect":
                    PickAPackageFileButton_Click(null, null);
                    exitNow = true;
                    break;
                case "packageInstallAddDependency":
                    AddAPackageDependencyFileButton_Click(null, null);
                    exitNow = true;
                    break;
                case "renamesys":
                    if (!SysInfo.IsOSVersuion10_0_10531)
                        exitNow = true;
                    else
                    {
                        DialogResult dr5 = await ShowDialog("Rename System", "Do you wish to rename the device?", new List<DialogResult> { DialogResult.Yes, DialogResult.Cancel });
                        if (dr5 == DialogResult.Yes)
                        { }
                        else
                            exitNow = true;
                    }
                        break;
                case "setpwd":
                    if (!SysInfo.IsOSVersuion10_0_10531)
                        exitNow = true;
                    else
                    {
                        DialogResult dr6 = await ShowDialog("Set Admin Pwd", "Do you wish to reset the admin password?", new List<DialogResult> { DialogResult.Yes, DialogResult.Cancel });
                        if (dr6 == DialogResult.Yes)
                        { }
                        else
                            exitNow = true;
                    }
                    break;

            }
            if (exitNow)
                return;


            

            if (cmd.name == "api")
            {
                //cmd = new Commands(cmd.name, textBoxAPI.Text, "", "");
                cmd.url = textBoxAPI.Text;
            }
            else
                textBoxAPI.Text = Command;
            CurrentCmd = cmd;

            //Show this in the MainPage URL textbox
            //API buttomn actions what ever is here.
            this.textBoxAPI.Text = cmd.url;
            NameValue.NameValues.Clear();
            DeviceInterfacesOutputList_DataContextRefresh();

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
                if (cmd.name == "renamesys")
                {
                        DialogResult dr7 = await ShowDialog("Renamed the device OK", "You will now need to run the command [Reboot].", new List<DialogResult> { DialogResult.OK });
                }



                DetailsTextBlock.Text = Command;

                //If the query response list is from an array simplify by only showing one entry in the list per item
                //ie Only show the item name/description etc.
                if (NameValue.NameValues_IsFrom_Array)
                {
                    NameValue.NameValuesStack.Push(NameValue.NameValues);
                    string identity = cmd.id;

                    //Get only the identity (name) record for each item
                    var nameValuesIds = from nv in NameValue.NameValues where nv.Name.Contains(identity) select nv;
                    NameValue.NameValues = nameValuesIds.ToList<NameValue>();
                }
            }

            DeviceInterfacesOutputList_DataContextRefresh();
        }
        /// <summary>
        /// Drill into an item to get selected item's properties
        /// </summary>
        private void DeviceInterfacesOutputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceInterfacesOutputList.SelectedIndex == -1)
                return;

            //Onlythese commands can be drilled into isarray
            if (!NameValue.NameValues_IsFrom_Array)
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
            DeviceInterfacesOutputList_DataContextRefresh();
            if (name.Contains("InstalledPackages.Name"))
            {
                

                if (NameValue.NameValues.Count>0)
                    textBoxAppRelativeID.Text = NameValue.NameValues[0].Value;
                if (NameValue.NameValues.Count > 3)
                    textBoxAppFullName.Text = NameValue.NameValues[3].Value;
                //if (NameValue.NameValues.Count > 4)
                textBoxPackage.Text =  "undefined"; // NameValue.NameValues[4].Value;
            }
        }

        private void DetailsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NameValue.ClearList();
            DeviceInterfacesOutputList_DataContextRefresh();
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
            Yes, No, OK, Cancel
        }

        private async Task<DialogResult> ShowDialog(string Title, string Message, List<DialogResult> buttons)
        {
            DialogResult res = DialogResult.Yes;
            

            try
            {
                MessageDialog dialog = new MessageDialog(Message);
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

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            return res;
        }

        private void textBoxNewSystemName_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.NewSystemName = textBoxNewSystemName.Text;
        }

        private void textBoxNewPwd_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.NewPassword = textBoxNewPwd.Text;
        }

        private void checkBoxIsV10_0_10531_Unchecked(object sender, RoutedEventArgs e)
        {
            SysInfo.IsOSVersuion10_0_10531 = false;
            GetCommands("Commands");
            NavLinksList.DataContext = Commands.CommandsList;
        }

        private void checkBoxIsV10_0_10531_Checked(object sender, RoutedEventArgs e)
        {
            SysInfo.IsOSVersuion10_0_10531 = true;
            GetCommands("CommandsV2");
            NavLinksList.DataContext = Commands.CommandsList;
        }

        private void textBoxPackage_TextChanged(object sender, TextChangedEventArgs e)
        {
            SysInfo.PackageName = textBoxPackage.Text;
        }
    }
}
