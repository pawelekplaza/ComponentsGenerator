using ComponentsGenerator.Serialization;
using ComponentsGenerator.Utils;
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

        private string solutionDirPath;
        public string SolutionDirPath
        {
            get { return solutionDirPath; }
            set { solutionDirPath = value; RaisePropertyChanged("SolutionDirPath"); }
        }


        public BrowseViewModel()
        {
            BrowseForInstalldir = new RelayCommand(() =>
            {
                InstallDirPath = BrowseForDirectory(InstallDirPath);
            });

            BrowseForSolutiondir = new RelayCommand(() =>
            {
                SolutionDirPath = BrowseForDirectory(SolutionDirPath);
            });

            GenerateXML = new RelayCommand(() =>
            {
                if (!ValidatePaths())
                    return;

                if (!SerializeXml())
                    Working.ShowMessage("Something went wrong! Check Components.wxs file for details.");                    
            });
        }

        public RelayCommand BrowseForSolutiondir { get; set; }
        public RelayCommand BrowseForInstalldir { get; set; }
        public RelayCommand GenerateXML { get; set; }

        private string BrowseForDirectory(string currentPath)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();

            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Cancel)
                return currentPath;

            return dialog.FileName;            
        }

        private bool SerializeXml()
        {
            var currentFilePath = "";
            try
            {
                var filesTree = Directory.EnumerateFiles(SolutionDirPath, "*", SearchOption.AllDirectories).ToList();
                var directoryTree = Directory.EnumerateDirectories(InstallDirPath, "*", SearchOption.AllDirectories).ToList();

                var model = new Wix { Fragment = new FragmentElement() };
                model.Fragment.ComponentGroup = new ComponentGroupElement { Id = $"{ Working.RemoveBlanks(InstallDirPath.Substring(InstallDirPath.LastIndexOf('\\') + 1)) }FileComponents" };
                model.Fragment.ComponentGroup.Components = new List<ComponentElement>();
                
                var format = Working.GetToStringFormat(filesTree.Count);
                var components = model.Fragment.ComponentGroup.Components;                
                foreach (var filePath in filesTree)
                {
                    currentFilePath = filePath;
                    var i = components.Count + 1;                                        
                    var count = i.ToString(format);
                    var fileName = Path.GetFileName(filePath);                    
                    var componentDirectory = Working.GetSubDirPath(InstallDirPath, filePath);
                    componentDirectory = componentDirectory.Substring(0, componentDirectory.Length - fileName.Length - 1);
                    var component = new ComponentElement
                    {
                        Id = Working.RemoveBlanks($"IDC_{ fileName }_{ count }"),
                        Directory = Working.RemoveBlanks($"IDD_{ componentDirectory }"),
                        Guid = "*"
                    };

                    var file = new FileElement
                    {
                        Id = Working.RemoveBlanks($"IDF_{ fileName }_{ count }"),
                        Name = Working.RemoveBlanks(fileName),
                        DiskId = "1",
                        Source = $"$(var.SolutionDir){ Working.GetSubDirPath(SolutionDirPath, filePath) }",
                        KeyPath = "yes"
                    };

                    component.File = file;
                    components.Add(component);
                }

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
                    writer.WriteLine();

                    writer.WriteLine("StackTrace:");
                    writer.WriteLine(ex.StackTrace);
                    writer.WriteLine();

                    writer.WriteLine("Source:");
                    writer.WriteLine(ex.Source);
                    writer.WriteLine();

                    var info =
                        "Source files directory: " + SolutionDirPath + "\n" +
                        "INSTALLDIR: " + InstallDirPath + "\n" +
                        "Current file path: " + currentFilePath;

                    writer.WriteLine("Info: ");
                    writer.WriteLine(info);
                }
                return false;
            }
        }
        
        private bool ValidatePaths()
        {
            if (Directory.Exists(SolutionDirPath) == false)
            {
                Working.ShowMessage("Source files directory doesn't exists!\nCheck the path again.");
                return false;
            }

            if (Directory.Exists(InstallDirPath) == false)
            {
                Working.ShowMessage("InstallDir directory doesn't exists!\nCheck the path again.");
                return false;
            }

            if (Working.IsSubdirectory(SolutionDirPath, InstallDirPath) == false)
            {
                Working.ShowMessage("INSTALLDIR is not inside source files dir!\nCheck your paths again.");
                return false;
            }

            return true;
        }
    }
}
