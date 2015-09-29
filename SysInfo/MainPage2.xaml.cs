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


        public void MainPage2()
        {
        }

        public StorageFile PackageFile { get; set; } = null;
        public StorageFile Certificate { get; set; } = null;
        public List<StorageFile> Dependencies { get; set; } = new List<StorageFile>();

        public string PackageFileStr { get; set; } = "";
        public string CertificateStr { get; set; } = "";
        public List<string> DependenciesStr { get; set; } = new List<string>();

        private async void PickAPackageFileButton_Click(object sender, RoutedEventArgs e)
        {
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
            StorageFile packageFile;
            //Make sure we don't select the app package;
            packageFile = await PickaFile(".appx");
            if (packageFile == null)
                return;

            if (packageFile.Name == PackageFile.Name)
                return;

            Dependencies.Add(packageFile);
            DependenciesStr.Add(encode);


            NameValue nvDepend = new NameValue("Dependency:", packageFile.Name);
            DeviceInterfacesOutputList_DataContextRefresh();
        }

        private void DeviceInterfacesOutputList_DataContextRefresh()
        {
            DeviceInterfacesOutputList.DataContext = null;
            DeviceInterfacesOutputList.DataContext = NameValue.NameValues;
        }

    }
}
