using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Win32;
using Newtonsoft.Json;
using QuoteSoft.CommonDataModel.Extensions;
using QuoteSoft.CommonDataModel.Utilities;
using Image = System.Drawing.Image;
using System.IO.Compression;
using Ionic.Zip;
using QuoteExpress.CommonControls.Utilities;
using QuoteExpress.CommonControls.Extensions;
using Infragistics.Windows.DataPresenter;

namespace Jeremys_project
{

    public class PhilosophyViewModel : BindableBase
    {

        public Controller Controller { get; set; }

        public EditControlsViewModel EditControls { get; set; }

        private bool m_listViewEnabled;

        public bool ListViewEnabled
        {
            get { return m_listViewEnabled; }
            set
            {
                m_listViewEnabled = value;
                RaisePropertyChanged(nameof(ListViewEnabled));
            }
        }

        public ObservableCollection<Philosopher> m_philosophers;

        public ObservableCollection<Philosopher> Philosophers
        {
            get { return m_philosophers ?? (m_philosophers = new ObservableCollection<Philosopher>()); }
            set
            {
                m_philosophers = value;
                RaisePropertyChanged(nameof(Philosophers));
            }
        }


        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand LoadCommand { get; private set; }
        public RelayCommand SaveToFileCommand { get; private set; }
        public RelayCommand WindowLoadedCommand { get; private set; }




        //private Philosopher selectedPhilosophers;

        public Philosopher SelectedPhilosopher
        {
            get { return EditControls.ActiveRecord; }
            set
            {
                EditControls.ActiveRecord = value;
                RaisePropertyChanged(nameof(SelectedPhilosopher));

                DeleteCommand.RaiseCanExecuteChanged();
                AddCommand.RaiseCanExecuteChanged();
                SaveToFileCommand.RaiseCanExecuteChanged();
                LoadCommand.RaiseCanExecuteChanged();


            }

        }

        public Philosopher.Books SelectedBook
        {
            get { return EditControls.ActiveBook; }
            set
            {
                EditControls.ActiveRecord = value;
                RaisePropertyChanged(nameof(SelectedBook));

            }

        }

        public PhilosophyViewModel(Controller controller)
        {
            Controller = controller;
            EditControls = new EditControlsViewModel(this);
            ListViewEnabled = true;
            Philosophers = new ObservableCollection<Philosopher>();

            Philosophers.Add(new Philosopher()
            {
                Id = 0,
                LogoPath = "",
                Name = "Frederich Nietzsche",
                Subject = "Genealogy of Morality",
                Era = 1880,
                Influential = true,
                Book = new ObservableCollection<Philosopher.Books>
                {
                    new Philosopher.Books() {BookId = 0, Title = "Beyond Good and Evil", Year = 1886},
                    new Philosopher.Books() {BookId = 1, Title = "The Will to Power", Year = 1901},
                    new Philosopher.Books() {BookId = 2, Title = "On the Genealogy of Morality", Year = 1887}
                }
            });

            Philosophers.Add(new Philosopher()
            {
                Id = 1,
                LogoPath = "",
                Name = "John Paul Sartre",
                Subject = "Existentialism",
                Era = 1960,
                Influential = true,
                Book = new ObservableCollection<Philosopher.Books>
                {
                    new Philosopher.Books() {BookId = 3, Title = "Existensialism and Human Emotions", Year = 1957},
                    new Philosopher.Books() {BookId = 4, Title = "Being and Nothingness", Year = 1943},
                    new Philosopher.Books() {BookId = 5, Title = "The Age of Reason", Year = 1945}
                }
            });

            Philosophers.Add(new Philosopher()
            {
                Id = 2,
                LogoPath = "",
                Name = "Michel Foucault",
                Subject = "Social Constructionism/Power Structures",
                Era = 1968,
                Influential = true,
                Book = new ObservableCollection<Philosopher.Books>
                {
                    new Philosopher.Books() {BookId = 6, Title = "Madness and Civilization", Year = 1961},
                    new Philosopher.Books() {BookId = 7, Title = "Aesthetics, method, and epistemology", Year = 1984},
                    new Philosopher.Books() {BookId = 8, Title = "The Birth of Biopolitics", Year = 2004}
                }
            });

            DeleteCommand = new RelayCommand(DeletePhilosopher, CanDeletePhilosopher);
            AddCommand = new RelayCommand(AddPhilosopher, CanAddPhilosopher);
            LoadCommand = new RelayCommand(controller.LoadFromFile, controller.CanLoadFromFile);
            SaveToFileCommand = new RelayCommand(controller.DialogSave, controller.CanDialogSave);
            WindowLoadedCommand = new RelayCommand(controller.WindowLoaded);

        }

        private void AddPhilosopher()
        {
            ListViewEnabled = false;
            EditControls.IsNewMode = true;
            Controller.State = Controller.EditState.Add;
            SelectedPhilosopher = new Philosopher();
            if (Controller.State == Controller.EditState.Add)
            {

                EditControls.ButtonText = "Save";
                EditControls.ButtonClickCommand = EditControls.SaveCommand;
                EditControls.ActiveRecord.Changed += SelectedPhilosopher_Changed;

            }
            //IsNewMode = true;

        }

        private bool CanAddPhilosopher()
        {

            if (Controller.State == Controller.EditState.View) return true;
            if (Controller.State == Controller.EditState.Add) return false;
            if (SelectedPhilosopher != null) return true;

            return false;

        }

        private void DeletePhilosopher()
        {
            Philosophers.Remove(SelectedPhilosopher);

        }

        private bool CanDeletePhilosopher()
        {
            if (Controller.State == Controller.EditState.Add) return false;
            //if (EditControls.State == EditState.View) return false;


            return SelectedPhilosopher != null;
        }

        private void SelectedPhilosopher_Changed(object sender, EventArgs e)
        {

            EditControls.SaveCommand.RaiseCanExecuteChanged();
        }

        //private void WindowLoaded()
        //{

        //    var initialDirectory = FileUtils.AppFolder.GetSubFolder("Data");
        //    DirectoryInfo dir2 = new DirectoryInfo(initialDirectory.FullName + @"\" + "data.zip");
        //    initialDirectory.EnsureExists();

        //    if (dir2.FullName.EndsWith(".zip"))
        //    {

        //        string zipPath = dir2.FullName;
        //        if (dir2.Exists)
        //        {
        //            using (ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
        //            {

        //                var entry = archive.GetEntry("data.xml");

        //                if (entry != null)
        //                {
        //                    using (var zipEntryStream = entry.Open())
        //                    {
        //                        using (StreamReader r = new StreamReader(dir2.FullName))
        //                        {
        //                            XmlSerializer deSerializer =
        //                            new XmlSerializer(typeof(ObservableCollection<Philosopher>));
        //                            var obj = deSerializer.Deserialize(zipEntryStream) as ObservableCollection<Philosopher>;
        //                            Philosophers.SetValues(obj);

        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private bool CanSaveToFile()
        //{
        //    if (State == EditState.View)
        //        return true;
        //    return false;
        //}



        //public void SaveToFile()
        //{

        //    var dir = FileUtils.AppFolder.GetSubFolder("Data");
        //    dir.EnsureExists();

        //    string savePath;
        //    SaveFileDialog dialogSave = new SaveFileDialog();
        //    // Default file extension
        //    dialogSave.DefaultExt = "txt";
        //    dialogSave.Filter = "ZIP file (*.zip)|*.zip";
        //    // Available file extensions
        //    // Adds a extension if the user does not
        //    dialogSave.AddExtension = true;
        //    // Restores the selected directory, next time
        //    dialogSave.RestoreDirectory = true;
        //    // Dialog title
        //    dialogSave.Title = "Where do you want to save the file?";
        //    // Startup directory
        //    var initialDirectory = FileUtils.AppFolder.GetSubFolder("Data");
        //    initialDirectory.EnsureExists();
        //    dialogSave.InitialDirectory = Path.GetFullPath(initialDirectory.ToString());
        //    dialogSave.ShowDialog();
        //    savePath = dialogSave.FileName;
        //    if (string.IsNullOrEmpty(savePath)) return;

        //    SaveToZip(new FileInfo(savePath));
        //}

        //    ////if (dialogSave.FileName.EndsWith(".xml"))
        ////{
        //var temp = FileUtils.TempFolder.GetFile("data.xml");
        //using (Stream saveStream = new FileStream(temp.FullName, FileMode.Create))
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Philosopher>));
        //    serializer.Serialize(saveStream, m_philosophers);

        //    var path = FileUtils.AppFolder.GetSubFolder("Images");
        //    DirectoryInfo dir2 = new DirectoryInfo(savePath);

        //    Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile();
        //    zip.AddFile(temp.FullName, @"\");
        //    zip.AddDirectory(path.FullName);
        //    zip.Save(dir2.FullName);

        //}
        //}

        //else
        //{
        //    JsonSerializer serializer2 = new JsonSerializer();
        //    using (StreamWriter sw = new StreamWriter(savePath))
        //    using (JsonWriter writer = new JsonTextWriter(sw))
        //    {
        //        serializer2.Serialize(writer, m_philosophers);

        //        string path = @"C:\Users\jeremy.greenwood\Documents\jeremytraining\Jeremy's project\Jeremy's project\bin\Debug\Images";
        //        Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile();
        //        zip.AddFile(dialogSave.FileName, @"\");
        //        zip.AddDirectory(path);
        //        zip.Save(@"C:\Users\jeremy.greenwood\Documents\jeremytraining\Jeremy's project\Jeremy's project\bin\Debug\Data\that.zip");
        //    }

        //}
        //}

        //private bool CanLoadFromFile()
        //{
        //    if (State == EditState.View)
        //        return true;
        //    return false;
        //}

        //private void LoadFromFile()
        //{

        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.CheckFileExists = true;
        //    openFileDialog.Filter = "ZIP file (*.zip)|*.zip";
        //    var initialDirectory = FileUtils.AppFolder.GetSubFolder("Data");
        //    openFileDialog.InitialDirectory = Path.GetFullPath(initialDirectory.ToString());
        //    openFileDialog.ShowDialog();


        //    if (openFileDialog.FileName != string.Empty)
        //    {

        //        if (openFileDialog.FileName.endsWith(".zip"))
        //        {

        //            string zipPath = openFileDialog.FileName;
        //            using (ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
        //            {

        //                var entry = archive.GetEntry("data.xml");

        //                if (entry != null)
        //                {
        //                    using (var zipEntryStream = entry.Open())
        //                    {
        //                        using (StreamReader r = new StreamReader(openFileDialog.FileName))
        //                        {
        //                            XmlSerializer deSerializer = new XmlSerializer(typeof(ObservableCollection<Philosopher>));
        //                            var obj = deSerializer.Deserialize(zipEntryStream) as ObservableCollection<Philosopher>;
        //                            Philosophers.SetValues(obj);

        //                        }
        //                    }
        //                }
        //            }

        //            string zPath = openFileDialog.FileName;
        //            using (ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(zPath))
        //            {

        //                var entry = archive.GetEntry("a.json");
        //                if (entry != null)
        //                {
        //                    using (var zipEntryStream = entry.Open())
        //                    {
        //                        using (StreamReader r = new StreamReader(zipEntryStream))
        //                        {
        //                            string json = r.ReadToEnd();
        //                            Philosopher[] deSerialize = JsonConvert.DeserializeObject<Philosopher[]>(json);
        //                            Philosophers.SetValues(deSerialize);

        //                        }
        //                    }
        //                }

        //            }
        //        }


        //        using (StreamReader r = new StreamReader(initialDirectory + " a.json"))
        //            {
        //                string json = r.ReadToEnd();
        //                Philosopher[] deSerialize = JsonConvert.DeserializeObject<Philosopher[]>(json);
        //                Philosophers.SetValues(deSerialize);


        //            }

        //            using (StreamReader r = new StreamReader(openFileDialog.FileName))
        //            {
        //                XmlSerializer deSerializer =
        //                new XmlSerializer(typeof(ObservableCollection<Philosopher>));
        //                var obj = deSerializer.Deserialize(r) as ObservableCollection<Philosopher>;
        //                Philosophers.SetValues(obj);
        //            }

        //    }
        //}



        private int m_id;

        public int Id
        {
            get { return m_id; }
            set
            {
                m_id = value;
                RaisePropertyChanged(nameof(Id));
            }

        }

        private string m_name;

        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        private string m_subject;

        public string Subject
        {
            get { return m_subject; }
            set
            {
                if (!string.Equals(m_subject, value))
                {
                    m_subject = value;
                    RaisePropertyChanged(nameof(Subject));
                }
            }

        }

        private int m_era;

        public int Era
        {
            get { return m_era; }
            set
            {
                if (!string.Equals(m_era, value))
                {
                    m_era = value;
                    RaisePropertyChanged(nameof(Era));
                }
            }
        }

    

        //private void SaveToZip(FileInfo saveFile)
        //{

        //    //var initialDirectory = FileUtils.AppFolder.GetSubFolder("Data");
        //    //initialDirectory.EnsureExists();

        //    using (var tempDir = new TemporaryDirectory("Jeremy"))
        //    {
        //        var temp = tempDir.GetFile("data.xml");

        //        using (Stream saveStream = new FileStream(temp.FullName, FileMode.Create))
        //        {
        //            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Philosopher>));
        //            serializer.Serialize(saveStream, m_philosophers);

        //            var path = FileUtils.AppFolder.GetSubFolder("Images");
        //            var res = path.CopyMeTo(tempDir.Directory.GetSubFolder("Images").FullName);
        //            ZipUtility.ZipUpFolder(tempDir.Directory, saveFile);

        //            //DirectoryInfo dir2 = new DirectoryInfo(initialDirectory.FullName + @"\" + "data.zip");
        //            //Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile();
        //            //zip.AddFile(temp.FullName, @"\");
        //            //zip.AddDirectory(path.FullName);
        //            //zip.Save(dir2.FullName);

        //        }
        //    }             

        //}

        //public void OnWindowClosing(object sender, CancelEventArgs e)
        //{
        //    var path = FileUtils.AppFolder.GetSubFolder("Data").FullName + @"\" + "data.zip";
        //    Controller.SaveToZip(new FileInfo(path));

        //}
    }
}
