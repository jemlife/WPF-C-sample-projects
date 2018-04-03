using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Newtonsoft.Json;
using QuoteExpress.CommonControls.Utilities;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;


namespace Jeremys_project
{
    [Serializable]
    public class PhilosophyModel
    {

        public BitmapImage Logo { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public int Era { get; set; }
        public bool Influential { get; set; }
        public List<Philosopher> Philosophers { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public Philosopher Philosopher { get; set; }
        //public ObservableCollection<Philosopher.Books> PhilBooks { get; set; }





        public override string ToString()
        {
            return Name;
        }

    }

    [Serializable]
    public class Philosopher : BindableBase, INotifyDataErrorInfo
    {

        public event EventHandler Changed;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
      

        private ObservableCollection<Books> m_book;

        public ObservableCollection<Books> Book
        {
            get { return m_book; }
            set
            {
                m_book = value;
                RaisePropertyChanged(nameof(Book));
            }
        }

        private int m_id;

        [Required]
        public int Id
        {
            get { return m_id; }
            set
            {
                ValidateProperty("Id", value);
                m_id = value;
                IsChanged = true;
                RaisePropertyChanged(nameof(Id));

            }
        }

        private string m_name;

        [Required]
        public string Name
        {
            get { return m_name; }
            set
            {
                ValidateProperty("Name", value);
                m_name = value;
                IsChanged = true;
                RaisePropertyChanged(nameof(Name));

            }
        }

        private string m_subject;

        [Required]
        public string Subject
        {
            get { return m_subject; }
            set
            {
                ValidateProperty("Subject", value);
                m_subject = value;
                IsChanged = true;
                RaisePropertyChanged(nameof(Subject));
            }
        }

        private int m_era;

        [Required]
        public int Era
        {
            get { return m_era; }
            set
            {
                ValidateProperty("Era", value);
                m_era = value;
                IsChanged = true;
                RaisePropertyChanged(nameof(Era));
            }
        }

        private string m_logoPath;

        public string LogoPath
        {
            get { return m_logoPath; }
            set
            {
                m_logoPath = value;
                IsChanged = true;

            }
        }

        [XmlIgnore, JsonIgnore]
        public BitmapImage LogoImage
        {

            get
            {
                if (string.IsNullOrEmpty(m_logoPath))
                    return null;

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                Uri imageSource = new Uri(LogoPath);
                image.UriSource = imageSource;
                image.EndInit();
                return image;

            }

        }


        //public string LogoPath { get; set; }
        public bool Influential { get; set; }

        private bool m_isChanged;

        [XmlIgnore, JsonIgnore]

        public bool IsChanged
        {
            get { return m_isChanged; }
            set
            {
                m_isChanged = value;
                OnChanged();
            }
        }



        public override string ToString()
        {
            return Name;

        }

        public static Philosopher CopyFrom(Philosopher philosopher)
        {
            if (philosopher == null) return null;

            return new Philosopher()
            {
                Id = philosopher.Id,
                LogoPath = philosopher.LogoPath,
                Name = philosopher.Name,
                Subject = philosopher.Subject,
                Era = philosopher.Era,
                Influential = philosopher.Influential,
                IsChanged = false,
                Book = philosopher.Book
                


            };
        }

        private void OnChanged() => Changed?.Invoke(this, new EventArgs());

        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public IEnumerable GetErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
                return _errors[propertyName];
            else
                return null;
        }

        public bool HasErrors
        {
            get { return _errors.Count > 0; }
        }

        protected override void SetProperty<T>(ref T member, T val, [CallerMemberName] string propertyName = null)
        {
            base.SetProperty(ref member, val, propertyName);
            ValidateProperty(propertyName, val);
        }

        private void ValidateProperty<T>(string propertyName, T value)
        {
            var results = new List<ValidationResult>();
            ValidationContext context = new ValidationContext(this);
            context.MemberName = propertyName;
            Validator.TryValidateProperty(value, context, results);

            if (results.Any())
            {
                _errors[propertyName] = results.Select(c => c.ErrorMessage).ToList();
            }
            else
            {
                _errors.Remove(propertyName);

            }
            if (ErrorsChanged != null) ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public class Books : Philosopher
        {

         
            private int m_bookId;
           
            public int BookId
            {
                get { return m_bookId; }
                set
                {
                    m_bookId = value;
                    RaisePropertyChanged(nameof(BookId));

                }
            }

            private string m_title;
            [Required]
            public string Title
            {
                get { return m_title; }
                set
                {
                    ValidateProperty("Title", value);
                    m_title = value;
                    RaisePropertyChanged(nameof(Title));
                }
            }

            private int m_year;
            [Required]
            public int Year
            {
                get { return m_year; }
                set
                {
                    ValidateProperty("Year", value);
                    m_year = value;
                    RaisePropertyChanged(nameof(Year));
                }
            }

           

        }
    }
}