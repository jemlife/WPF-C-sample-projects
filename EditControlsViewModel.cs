using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using QuoteExpress.CommonControls;
using QuoteExpress.CommonControls.Extensions;
using QuoteExpress.CommonControls.Utilities;
using static Jeremys_project.PhilosophyViewModel;
using Button = System.Windows.Forms.Button;
using Label = System.Windows.Forms.Label;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Forms.TextBox;

namespace Jeremys_project
{
    public class EditControlsViewModel : BindableBase
    {
        private PhilosophyViewModel PhilVM { get; set; }

        public RelayCommand RevertCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand SetLogoCommand { get; private set; }
        public RelayCommand ClearImageCommand { get; private set; }
        public RelayCommand DeleteBookCommand { get; private set; }
        public RelayCommand AddBookCommand { get; private set; }
        public RelayCommand SaveBookCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public EditControlsViewModel(PhilosophyViewModel philVm)
        {

            PhilVM = philVm;

            RevertCommand = new RelayCommand(OnRevert, CanRevert);
            EditCommand = new RelayCommand(EditPhilosopher, CanEditPhilosopher);
            SaveCommand = new RelayCommand(SavePhilosopher, CanSavePhilosopher);
            SetLogoCommand = new RelayCommand(SetLogo, CanSetLogo);
            ClearImageCommand = new RelayCommand(ClearImage, CanClearImage);
            DeleteBookCommand = new RelayCommand(DeleteBook, CanDeleteBook);
            AddBookCommand = new RelayCommand(AddBook, CanAddBook);
            SaveBookCommand = new RelayCommand(SaveBook);
            CancelCommand = new RelayCommand(CancelDialog);
        }

        private void CancelDialog()
        {
            IsVisiblePane = Visibility.Collapsed;

        }
     
        private bool CanAddBook()
        {
            //if (State == Controller.EditState.View) return false;
            //if (State == Controller.EditState.Add) return true;
            return true;
            return false;
        }

        private bool CanDeleteBook()
        {
            //if (ActiveBook != null)
                return true;
            return false;
        }

        private bool m_isNewMode;

        public bool IsNewMode
        {
            get { return m_isNewMode; }
            set
            {
                m_isNewMode = value;
                RaisePropertyChanged(nameof(IsNewMode));
            }
        }

        private string m_buttonText;

        public string ButtonText
        {
            get { return m_buttonText ?? (m_buttonText = "Edit"); }
            set
            {
                m_buttonText = value;
                RaisePropertyChanged(nameof(ButtonText));
            }
        }

        public ICommand m_buttonClickCommand { get; private set; }
        public ICommand ButtonClickCommand
        {
            get { return m_buttonClickCommand ?? (m_buttonClickCommand = EditCommand); }
            set
            {
                m_buttonClickCommand = value;
                RaisePropertyChanged(nameof(ButtonClickCommand));
            }
        }

        private Philosopher originalValue;

        private void EditPhilosopher()
        {
            PhilVM.Controller.State = Controller.EditState.Edit;

            originalValue = Philosopher.CopyFrom(ActiveRecord);
            ActiveRecord = Philosopher.CopyFrom(ActiveRecord);
            ActiveBook = (Philosopher.Books)(Philosopher.CopyFrom(ActiveBook));
            ActiveRecord.Changed += SelectedPhilosopher_Changed;
            ButtonText = "Save";
            ButtonClickCommand = SaveCommand;
            IsNewMode = true;


        }

        private bool CanEditPhilosopher()
        {
            //if (PhilVM.Controller.State == Controller.EditState.View) return false;
            if (ActiveRecord != null || PhilVM.Controller.State == Controller.EditState.Edit)
                return true;
            return false;

        }

        //public Controller.EditState State { get; set; }


        private void SelectedPhilosopher_Changed(object sender, EventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void SavePhilosopher()
        {
           
            if (ActiveRecord.IsChanged)
            {
                if (PhilVM.Controller.State == Controller.EditState.Edit)
                {
                    var editPhil = Philosopher.CopyFrom(ActiveRecord);
                    var prev = PhilVM.Philosophers.FirstOrDefault(p => p.Id == originalValue.Id);
                    if (prev != null) PhilVM.Philosophers.Remove(prev);
                    PhilVM.Philosophers.Add(editPhil);

                    if (!string.IsNullOrEmpty(editPhil.LogoPath))
                    {
                        var filePath = FileUtils.AppFolder.GetSubFolder("Images");
                        var file = new FileInfo(editPhil.LogoPath);
                        var destFile = filePath.GetFile(file.Name);
                        file.CopyMeTo(destFile);
                    }

                    else if (!string.IsNullOrEmpty(originalValue.LogoPath))
                    {
                        var filePath = FileUtils.AppFolder.GetSubFolder("Images");
                        var file = new FileInfo(originalValue.LogoPath);
                        var destFile = filePath.GetFile(file.Name);
                        File.Delete(destFile.FullName);

                    }
                  

                }

                PhilVM.ListViewEnabled = true;
            }


            if (PhilVM.Controller.State == Controller.EditState.Add)
            {

                ButtonText = "Save";
                PhilVM.Philosophers.Add(ActiveRecord);
                IsNewMode = false;
            }

            //if (PhilVM.Controller.State == Controller.EditState.Edit)
            //{
            //    PhilVM.Controller.SetState(Controller.EditState.View);
            //}

            IsNewMode = false;
            ButtonText = "Edit";
            ButtonClickCommand = EditCommand;
            PhilVM.Controller.SetState(Controller.EditState.View);
            RaisePropertyChanged(nameof(CanRevert));
            RaisePropertyChanged(nameof(CanSavePhilosopher));



        }

        private bool CanSavePhilosopher()
        {
            //PhilVM.Controller.SetState(Controller.EditState.View);
            if (PhilVM.Controller.State == Controller.EditState.View) return false;
            return (!ActiveRecord.HasErrors && ActiveRecord.IsChanged);
                return true;         
            return false;

        }


        private bool CanRevert()
        {

            return (PhilVM.Controller.State == Controller.EditState.Add ||
                    PhilVM.Controller.State == Controller.EditState.Edit);
            if (PhilVM.Controller.State == Controller.EditState.View) return false;
            return false;

        }

        private void OnRevert()
        {

            PhilVM.Controller.State = Controller.EditState.View;
            ActiveRecord = Philosopher.CopyFrom(originalValue);

            if (ActiveRecord != null)
            {
                ActiveRecord.Changed += SelectedPhilosopher_Changed;
            }

            if (PhilVM.Controller.State == Controller.EditState.View)
            {
                IsNewMode = false;
                PhilVM.ListViewEnabled = true;
            }
        }

        private Philosopher m_activeRecord;

        public Philosopher ActiveRecord
        {
            get { return m_activeRecord; }
            set
            {
                m_activeRecord = value;
                RaisePropertyChanged(null);
                //IsNewMode = false;
                EditCommand.RaiseCanExecuteChanged();
                RevertCommand.RaiseCanExecuteChanged();
                SetLogoCommand.RaiseCanExecuteChanged();
                ClearImageCommand.RaiseCanExecuteChanged();
                ButtonText = "Edit";
                ButtonClickCommand = EditCommand;


            }
        }

        private Philosopher.Books m_activeBook;
        public Philosopher.Books ActiveBook
        {
            get { return m_activeBook; }
            set
            {
                m_activeBook = value;
                RaisePropertyChanged(nameof(ActiveBook));


            }
        }

        private void DeleteBook()
        {
            ActiveRecord.Book.Remove(ActiveBook);
        }

        private void AddBook()
        {
            PhilVM.Controller.State = Controller.EditState.Add;
            ActiveBook = new Philosopher.Books();
            IsVisiblePane = Visibility.Visible;

        }

        private void SaveBook()
        {
            

            ActiveRecord.Book.Add(ActiveBook);
            IsVisiblePane = Visibility.Hidden;

        }


        private bool CanSetLogo()
        {

            if (PhilVM.Controller.State == Controller.EditState.Edit || PhilVM.Controller.State == Controller.EditState.Add)
                return true;
            return false;
        }

        private void SetLogo()
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPG file (*.jpg)|*.jpg";
            DirectoryInfo dir = new DirectoryInfo(Environment.SpecialFolder.MyPictures.ToString());
            //DirectoryInfo dir = new DirectoryInfo(Path.Combine(@"C:\", "Users", "jeremy.greenwood", "Documents", "jeremytraining", "Jeremy's project", "Jeremy's project", "Images"));
            ofd.InitialDirectory = dir.ToString();
            ofd.ShowDialog();

            if (File.Exists(ofd.FileName))
            {

                //string sourcePath = 
                //    @"C:\Users\jeremy.greenwood\Documents\jeremytraining\Jeremy's project\Jeremy's project\Images";
                var filePath = FileUtils.AppFolder.GetSubFolder("Images");
                filePath.EnsureExists();

                var sourceFile = new FileInfo(ofd.FileName);
                //Path.Combine(sourcePath, Path.GetFileName(ofd.FileName));
                var destFile = filePath.GetFile(sourceFile.Name);
                //Path.Combine(filePath.ToString(), Path.GetFileName(ofd.FileName));

                //sourceFile.CopyMeTo(destFile);
                //File.Copy(ofd.FileName, destFile, true);

                ActiveRecord.LogoPath = sourceFile.FullName;
                RaisePropertyChanged(null);


            }
        }

        private bool CanClearImage()
        {

            if (PhilVM.Controller.State == Controller.EditState.Edit)
                return true;
            return false;

        }

        private void ClearImage()
        {

            string path = ActiveRecord.LogoPath;
            ActiveRecord.LogoPath = "";
            RaisePropertyChanged(null);
            //File.Delete(path);
            //SelectedPhilosopher.LogoPath = null;

        }

        private Visibility _isVisiblePane = Visibility.Hidden;
        public Visibility IsVisiblePane
        {
            get
            {
                return _isVisiblePane;
            }
            set
            {

                _isVisiblePane = value;
                RaisePropertyChanged(nameof(IsVisiblePane));
            }
        }

    }

}
