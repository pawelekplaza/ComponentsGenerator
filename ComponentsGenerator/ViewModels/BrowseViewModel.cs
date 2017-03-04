using ComponentsGenerator.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace ComponentsGenerator.ViewModels
{
    public class BrowseViewModel : ViewModelBase
    {
        private string installDirPath;    
        public string InstallDirPath
        {
            get { return installDirPath; }
            set { installDirPath = value; RaisePropertyChanged("InstallDirPath"); }
        }

        public BrowseViewModel()
        {
            BrowseForInstalldir = new RelayCommand(() =>
            {
                string path = InstallDirPath;
                var result = BrowseForDirectory(ref path);
                if (result)
                    InstallDirPath = path;
            });

            GenerateXML = new RelayCommand(() =>
            {
                if (!SerializeXml())
                    MessageBox.Show("Something went wrong! Check Components.wxs file for details.");
            });
        }

        public RelayCommand BrowseForInstalldir { get; set; }
        public RelayCommand GenerateXML { get; set; }

        private bool BrowseForDirectory(ref string path)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();

            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Cancel)
                return false;

            path = dialog.FileName;
            return true;
        }

        private bool SerializeXml()
        {
            try
            {
                var model = new Wix();

                var serializer = new XmlSerializer(model.GetType());
                using (var file = new FileStream("Components.wxs", FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(file))
                {
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add("x", @"http://schemas.microsoft.com/wix/2006/wi");
                    serializer.Serialize(writer, model, namespaces);
                }

                return true;
            }
            catch (Exception ex)
            {
                using (var file = new FileStream("Components.wxs", FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(file))
                {
                    writer.WriteLine("Message:");
                    writer.WriteLine(ex.Message);

                    writer.WriteLine("StackTrace:");
                    writer.WriteLine(ex.StackTrace);

                    writer.WriteLine("Source:");
                    writer.WriteLine(ex.Source);
                }
                return false;
            }
        }
    }
}
