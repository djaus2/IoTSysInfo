using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SysInfo
{
    public sealed partial class MainPage : Page
    {
        static StorageFolder RootRemoteCDrive = null;

        public void MainPage2()
        {
        }

        public string InstallDir {  get { return PackageFile.DisplayName + "Install"; } }

        public StorageFile PackageFile { get; set; } = null;
        public StorageFile Certificate { get; set; } = null;
        public List<StorageFile> Dependencies { get; set; } = new List<StorageFile>();

        public string PackageFileStr { get; set; } = "";
        public string CertificateStr { get; set; } = "";
        public List<string> DependenciesStr { get; set; } = new List<string>();

        private static StorageFolder PackageFolder = null;

        private async void PickAPackageFileButton_Click(object sender, RoutedEventArgs e)
        {

            //Get package and cert files and location
            PackageFolder = null;
            Dependencies = new List<StorageFile>();
            PackageFile = await PickaFile(".appx");
            if (PackageFile == null)
                return; ;
            PackageFileStr = encode;
            Certificate = await PickaFile(".cer");
            if (Certificate == null)
                return; ;
            CertificateStr = encode;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                NameValue.NameValues.Clear();
                DeviceInterfacesOutputList.DataContext = null;
                //DeviceInterfacesOutputList.Items.Clear();
                NameValue nvPack = new NameValue("Package:", PackageFile.Name);
                NameValue nvCert = new NameValue("Certificate:", Certificate.Name);
                DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
                //DeviceInterfacesOutputList.UpdateLayout();
            });



            //Create local folder to place AppInstall and package files. Clear it out if it exists
            StorageFolder fldrLocal = ApplicationData.Current.LocalFolder;           
            try
            {
                var fldr = await fldrLocal.GetFolderAsync(PackageFile.Name);
                if (fldr != null)
                {
                    var files = await fldr.GetFilesAsync();
                    foreach (StorageFile fi in files)
                        await fi.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                //Will be folder not found
                string msg = ex.Message;
            }


            //Get package and cert files
            StorageFolder fldrPackage = await fldrLocal.CreateFolderAsync(PackageFile.Name, CreationCollisionOption.OpenIfExists);
            await PackageFile.CopyAsync(fldrPackage, PackageFile.Name, NameCollisionOption.ReplaceExisting);
            await Certificate.CopyAsync(fldrPackage, Certificate.Name, NameCollisionOption.ReplaceExisting);


            //Get this app's installation folder
            StorageFolder fldrApp = Windows.ApplicationModel.Package.Current.InstalledLocation;
            //Get source files folder from <this app install dir>\AppInstall
            StorageFolder fldrAppInstall = await fldrApp.GetFolderAsync("AppInstall");

            
            //Copy AppiNstall.cmd
            StorageFile AppInstall =
                await fldrAppInstall.GetFileAsync("AppInstall.cmd");
            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(AppInstall);
            StorageFile AppInstallOut =
                await fldrPackage.CreateFileAsync("AppInstall.cmd",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteBufferAsync(AppInstallOut, buffer);

            //Modify: AppConfig.cmd
            StorageFile AppxConfig =
                await fldrAppInstall.GetFileAsync("AppxConfig.cmd");
            string AppInstallContents = await Windows.Storage.FileIO.ReadTextAsync(AppxConfig);
            AppInstallContents = AppInstallContents.Replace("MainAppx_1.0.0.0_arm", PackageFile.DisplayName);
            
            AppInstallContents = AppInstallContents.Replace("Microsoft.VCLibs.ARM.14.00 Microsoft.NET.Native.Runtime.1.1 Microsoft.NET.Native.Framework.1.2", " ");

            StorageFile AppxConfigdOut =
                await fldrPackage.CreateFileAsync("AppxConfig.cmd",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(AppxConfigdOut, AppInstallContents);

            //Copy DeployTask.cmd
            StorageFile DeployTask =
                await fldrAppInstall.GetFileAsync("DeployTask.cmd");
            var buffer3 = await Windows.Storage.FileIO.ReadBufferAsync(DeployTask);
            StorageFile DeployTaskOut =
                await fldrPackage.CreateFileAsync("DeployTask.cmd",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteBufferAsync(DeployTaskOut, buffer3);


            //Modify: oemcustomization.cmd  This goes to c$\Windows\system32
            StorageFile oemcustomization =
                    await fldrAppInstall.GetFileAsync("oemcustomization.cmd");
            string oemcustomizationContents = await Windows.Storage.FileIO.ReadTextAsync(oemcustomization);
            oemcustomizationContents = oemcustomizationContents.Replace("INSTALLDIR", InstallDir);

            StorageFile oemcustomizationdOut =
                await fldrPackage.CreateFileAsync("oemcustomization.cmd",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(oemcustomizationdOut, oemcustomizationContents);

            PackageFolder = fldrPackage;

        }

        private string encode = "";

        private async Task<StorageFile> PickaFile(string exten)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            openPicker.FileTypeFilter.Add(exten);
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
                encode = await ReadBytes(file);
            return file;
        }
        private async Task<string> ReadBytes(StorageFile file)
        {
            if (file != null)
            {
                try
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    using (DataReader dataReader = DataReader.FromBuffer(buffer))
                    {
                        Byte[] fileContent = new Byte[buffer.Length];
                        dataReader.ReadBytes(fileContent);
                        return SysInfo.EncodeB(fileContent);
                    }
                }
                catch (FileNotFoundException)
                {
                    return "FileNotFound";
                }
            }
            else
            {
                return "InvalidFilePath";
            }
        }


        private async void AddAPackageDependencyFileButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (PackageFolder == null)
                    return;

                StorageFile packageFile;
                //Make sure we don't select the app package;
                packageFile = await PickaFile(".appx");
                if (packageFile == null)
                    return;

                if (packageFile.Name == PackageFile.Name)
                    return;

                Dependencies.Add(packageFile);
                DependenciesStr.Add(encode);


                await packageFile.CopyAsync(PackageFolder, packageFile.Name, NameCollisionOption.ReplaceExisting);


                //Update config file for this dependancy
                StorageFile AppxConfig =
                     await PackageFolder.GetFileAsync("AppxConfig.cmd");
                string AppInstallContents = await Windows.Storage.FileIO.ReadTextAsync(AppxConfig);
                AppInstallContents = AppInstallContents.Replace("set dependencylist=", "set dependencylist=" + packageFile.DisplayName + " ");
                StorageFile AppxConfigdOut =
                    await PackageFolder.CreateFileAsync("AppxConfig.cmd",
                        Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(AppxConfigdOut, AppInstallContents);


                NameValue nvDepend = new NameValue("Dependency:", packageFile.Name);
                DeviceInterfacesOutputList_DataContextRefresh();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }



        private async void PackageInstallDeploy()
        {
            if (PackageFile == null)
                return;


            //Clipboard "\\\\minwinpc\\c$" setText ??
            //Windows.ApplicationModel.DataTransfer.DataPackage dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
            //dp.SetText("\\\\minwinpc\\c$");

            FolderPicker fp = new FolderPicker();
            fp.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            fp.FileTypeFilter.Add("*");
            fp.CommitButtonText = "Select " + RemoteFileSystem.Text + " only.";
            RootRemoteCDrive = await fp.PickSingleFolderAsync();
            StorageFolder fldrRemotePackage = null;
            StorageFolder RemoteWindows = null;
            StorageFolder System32 = null;


            //Create or clean out remote folder
            try
            {
                fldrRemotePackage = await RootRemoteCDrive.CreateFolderAsync(InstallDir, CreationCollisionOption.OpenIfExists);

                if (fldrRemotePackage != null)
                {
                    var files = await fldrRemotePackage.GetFilesAsync();
                    foreach (StorageFile fi in files)
                        await fi.DeleteAsync();

                    RemoteWindows = await RootRemoteCDrive.GetFolderAsync("Windows");
                    if (RemoteWindows != null)
                    {
                        System32 = await RemoteWindows.GetFolderAsync("System32");
                    }
                }
                if ((fldrRemotePackage == null) || (System32 == null))
                    //Need a popup here
                    return;

            }
            catch (Exception ex)
            {
                //A folder not found
                string msg = ex.Message;
            }


            //Copy files to remote system
            StorageFolder fldrLocal = ApplicationData.Current.LocalFolder;
            try
            {
                var fldr = await fldrLocal.GetFolderAsync(PackageFile.Name);
                if (fldr != null)
                {
                    var files = await fldr.GetFilesAsync();
                    foreach (StorageFile fi in files)
                    {
                        if (fi.Name != "oemcustomization.cmd")
                            await fi.CopyAsync(fldrRemotePackage);
                        else
                            await fi.CopyAsync(System32, "oemcustomization.cmd", NameCollisionOption.ReplaceExisting);
                    }
                }
            }
            catch (Exception ex)
            {
                //Will be folder not found
                string msg = ex.Message;
            }

        }

        private async void PackageInstallCleanUp()
        {
            if (PackageFile == null)
                return;



            StorageFolder fldrRemotePackage = null;
            StorageFolder RemoteWindows = null;
            StorageFolder System32 = null;


            //Create or clean out remote folder
            try
            {
                fldrRemotePackage = await RootRemoteCDrive.GetFolderAsync(InstallDir);

                if (fldrRemotePackage != null)
                {
                    var files = await fldrRemotePackage.GetFilesAsync();
                    foreach (StorageFile fi in files)
                        await fi.DeleteAsync();

                    await fldrRemotePackage.DeleteAsync();

                    RemoteWindows = await RootRemoteCDrive.GetFolderAsync("Windows");
                    if (RemoteWindows != null)
                    {
                        System32 = await RemoteWindows.GetFolderAsync("System32");
                        if (System32 != null)
                        {
                            var file = await System32.GetFileAsync("oemcustomization.cmd");
                            if (file != null)
                                await file.DeleteAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //A folder not found
                string msg = ex.Message;
            }



        }
        private void DeviceInterfacesOutputList_DataContextRefresh()
        {
            DeviceInterfacesOutputList.DataContext = null;
            DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
        }

    }
}
