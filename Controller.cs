using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Win32;
using QuoteExpress.CommonControls.Extensions;
using QuoteExpress.CommonControls.Utilities;
using QuoteSoft.CommonDataModel.Extensions;

namespace Jeremys_project
{
    public class Controller : BindableBase
    {
        public PhilosophyViewModel PhilVm { get; set; }
        public MainWindow MainWin { get; set; }

        public enum EditState { View, Add, Edit }

        private EditState m_state;

        public  EditState State
        {
            get { return m_state; }
            set
            {
                if (m_state != value)
                {
                    m_state = value;
                    RaisePropertyChanged(nameof(State));
                    //PhilVm.ListViewEnabled = (State == EditState.View);


                }
            }
        }

        public void SetState(EditState state)
        {

            State = state;
        }


        public Controller()
        {
            //PhilVm = new PhilosophyViewModel(this);         
            //SaveToFileCommand = new RelayCommand(DialogSave, CanDialogSave);


            //MainWin = new MainWindow(this);
        }

        public void ShowWindow()
        {

            MainWin = new MainWindow(this);
            MainWin.ShowDialog();

        }

        public void DialogSave()
        {

            var dir = FileUtils.AppFolder.GetSubFolder("Data");
            dir.EnsureExists();

            string savePath;
            SaveFileDialog dialogSave = new SaveFileDialog();
            dialogSave.DefaultExt = "txt";
            dialogSave.Filter = "ZIP file (*.zip)|*.zip";
            dialogSave.AddExtension = true;
            dialogSave.RestoreDirectory = true;
            dialogSave.Title = "Where do you want to save the file?";
            var initialDirectory = FileUtils.AppFolder.GetSubFolder("Data");
            initialDirectory.EnsureExists();
            dialogSave.InitialDirectory = Path.GetFullPath(initialDirectory.ToString());
            dialogSave.ShowDialog();
            savePath = dialogSave.FileName;
            if (string.IsNullOrEmpty(savePath)) return;

            SaveToZip(new FileInfo(savePath));
        }

        public bool CanDialogSave()
        {
            if (State == EditState.View)
                return true;
            return false;
        }

        public void LoadFromFile()
        {
            string path;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.Filter = "ZIP file (*.zip)|*.zip";
            var initialDirectory = FileUtils.AppFolder.GetSubFolder("Data");
            openFileDialog.InitialDirectory = Path.GetFullPath(initialDirectory.ToString());
            openFileDialog.ShowDialog();
            path = openFileDialog.FileName;


            if (openFileDialog.FileName != string.Empty)
            {

                if (openFileDialog.FileName.EndsWith(".zip"))
                {

                    using (ZipArchive archive = ZipFile.OpenRead(path))
                    {

                        var entry = archive.GetEntry("data.xml");

                        if (entry != null)
                        {
                            using (var zipEntryStream = entry.Open())
                            {
                                using (StreamReader r = new StreamReader(path))
                                {
                                    XmlSerializer deSerializer =
                                    new XmlSerializer(typeof(ObservableCollection<Philosopher>));
                                    var obj = deSerializer.Deserialize(zipEntryStream) as ObservableCollection<Philosopher>;
                                    MainWin.Pvm.Philosophers.SetValues(obj);

                                }
                            }
                        }
                    }
                }
            }
        }

        public bool CanLoadFromFile()
        {
            if (State == EditState.View)
                return true;
            return false;
        }

        public void SaveToZip(FileInfo saveFile)
        {
            //var initialDirectory = FileUtils.AppFolder.GetSubFolder("Data");
            //initialDirectory.EnsureExists();

            using (var tempDir = new TemporaryDirectory("Jeremy"))
            {
                var temp = tempDir.GetFile("data.xml");

                using (Stream saveStream = new FileStream(temp.FullName, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Philosopher>));
                    serializer.Serialize(saveStream, MainWin.Pvm.m_philosophers);

                    var path = FileUtils.AppFolder.GetSubFolder("Images");
                    var res = path.CopyMeTo(tempDir.Directory.GetSubFolder("Images").FullName);
                    ZipUtility.ZipUpFolder(tempDir.Directory, saveFile);

                    //DirectoryInfo dir2 = new DirectoryInfo(initialDirectory.FullName + @"\" + "data.zip");
                    //Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile();
                    //zip.AddFile(temp.FullName, @"\");
                    //zip.AddDirectory(path.FullName);
                    //zip.Save(dir2.FullName);

                }
            }
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            var path = FileUtils.AppFolder.GetSubFolder("Data").FullName + @"\" + "data.zip";
            SaveToZip(new FileInfo(path));

        }

        public void WindowLoaded()
        {
           
            var initialDirectory = FileUtils.AppFolder.GetSubFolder("Data");
            if (initialDirectory != null)
            {
                DirectoryInfo dir2 = new DirectoryInfo(initialDirectory.FullName + @"\" + "data.zip");
                initialDirectory.EnsureExists();

                initialDirectory.GetFile("data.zip");

                string zipPath = dir2.FullName;
                if (File.Exists(zipPath))
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {

                        var entry = archive.GetEntry("data.xml");

                        if (entry != null)
                        {
                            using (var zipEntryStream = entry.Open())
                            {
                                using (StreamReader r = new StreamReader(dir2.FullName))
                                {
                                    XmlSerializer deSerializer =
                                        new XmlSerializer(typeof(ObservableCollection<Philosopher>));
                                    var obj = deSerializer.Deserialize(zipEntryStream) as ObservableCollection<Philosopher>;
                                    MainWin.Pvm.Philosophers.SetValues(obj);

                                }
                            }
                        }
                    }
                }
            }
           
        }
    }
}
