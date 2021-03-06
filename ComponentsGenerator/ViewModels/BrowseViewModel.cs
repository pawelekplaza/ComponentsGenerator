﻿using ComponentsGenerator.Serialization;
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
using System.Windows.Input;
using System.Xml.Serialization;

namespace ComponentsGenerator.ViewModels
{
    public class BrowseViewModel : ViewModelBase
    {
        private string currentFilePath = "";

        private string installDirPath;
        public string InstallDirPath
        {
            get { return installDirPath; }
            set { installDirPath = value; RaisePropertyChanged(nameof(InstallDirPath)); }
        }

        private string solutionDirPath;
        public string SolutionDirPath
        {
            get { return solutionDirPath; }
            set { solutionDirPath = value; RaisePropertyChanged(nameof(SolutionDirPath)); }
        }

        private bool shouldPrepareX64Components;
        public bool ShouldPrepareX64Components
        {
            get { return shouldPrepareX64Components; }
            set { shouldPrepareX64Components = value; RaisePropertyChanged(nameof(ShouldPrepareX64Components)); }
        }

        private bool shouldGenerateGuids;
        public bool ShouldGenerateGuids
        {
            get { return shouldGenerateGuids; }
            set { shouldGenerateGuids = value; RaisePropertyChanged(nameof(ShouldGenerateGuids)); }
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

        public ICommand BrowseForSolutiondir { get; private set; }
        public ICommand BrowseForInstalldir { get; private set; }
        public ICommand GenerateXML { get; private set; }

        private string BrowseForDirectory(string currentPath)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            var result = dialog.ShowDialog();

            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Cancel)
                return currentPath;

            return dialog.FileName;
        }

        private bool SerializeXml()
        {            
            try
            {
                var filesTree = Directory.EnumerateFiles(InstallDirPath, "*", SearchOption.AllDirectories).ToList();

                var model = new Wix { Fragment = new FragmentElement() };
                CreateWixComponents(model, filesTree);
                CreateDirectories(model);

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

        private void CreateWixComponents(Wix model, List<string> filesTree)
        {
            try
            {
                model.Fragment.ComponentGroup = new ComponentGroupElement { Id = $"{ Working.RemoveIllegalCharacters(InstallDirPath.Substring(InstallDirPath.LastIndexOf('\\') + 1)) }FileComponents" };
                model.Fragment.ComponentGroup.Components = new List<ComponentElement>();

                var format = Working.GetToStringFormat(filesTree.Count);
                var components = model.Fragment.ComponentGroup.Components;
                foreach (var filePath in filesTree)
                {
                    var i = components.Count + 1;
                    var count = i.ToString(format);
                    var fileName = Path.GetFileName(filePath);
                    var componentDirectory = Working.GetSubDirPath(InstallDirPath, filePath);
                    componentDirectory = componentDirectory.Substring(0, componentDirectory.Length - fileName.Length - 1).Replace('\\', '_');
                    var component = new ComponentElement
                    {
                        Id = Working.RemoveIllegalCharacters($"IDC_{ fileName }_{ count }"),
                        Directory = Working.RemoveIllegalCharacters($"IDD_{ componentDirectory }"),
                        Guid = ShouldGenerateGuids ? Guid.NewGuid().ToString().ToUpper() : "*",
                        Win64 = ShouldPrepareX64Components ? "yes" : "no"
                    };

                    var file = new FileElement
                    {
                        Id = Working.RemoveIllegalCharacters($"IDF_{ fileName }_{ count }"),
                        Name = fileName,
                        DiskId = "1",
                        Source = $"$(var.SolutionDir){ Working.GetSubDirPath(InstallDirPath, filePath) }",
                        KeyPath = "yes"
                    };

                    component.File = file;
                    components.Add(component);
                }
            }
            catch (Exception ex)
            {
                var message = "Exception thrown in CreateWixComponents()\n";
                throw new Exception(message, ex);
            }
        }

        private void CreateDirectories(Wix model)
        {
            var dir = new DirectoryInfo(InstallDirPath);
            model.Fragment.Directories = new List<DirectoryElement>
            {
                CreateSubDir(dir)
            };
        }

        private DirectoryElement CreateSubDir(DirectoryInfo dir)
        {
            var pathFromSourceDirectory = Working.GetSubDirPath(InstallDirPath, dir.FullName);
            var dirElement = new DirectoryElement
            {
                Id = $"IDD_{ Working.RemoveIllegalCharacters(pathFromSourceDirectory) }",
                Name = dir.Name,
                Directories = new List<DirectoryElement>()
            };
            
            foreach (var subDir in dir.GetDirectories())
            {
                dirElement.Directories.Add(CreateSubDir(subDir));
            }

            return dirElement;
        }
    }
}
