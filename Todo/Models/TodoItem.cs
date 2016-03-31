using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Todo.Models
{

    //[Serializable]
    class TodoItem : INotifyPropertyChanged
    {
        private bool _completed;
        private double _opacity;
        private Uri _imageUri;
        private string _title;
        private string _id;


        public string id {
            get
            {
                return _id;
            }
        }

        public Uri ImageUri
        {
            get
            {
                return this._imageUri;
            }
            set
            {
                _imageUri = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string title {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
                this.OnPropertyChanged();
            }
        }

        public string description { get; set; }

        public double opacity {
            get {
                return this._opacity;
            }
            set {
                this._opacity = value;
                this.OnPropertyChanged();
            }
        }


        public bool completed {
            get {
                return this._completed;
            }
            set {
                this._completed = value;
                if (this._completed) opacity = 1.0;
                else opacity = 0.0;
                this.OnPropertyChanged();
            }
        }

        public DateTimeOffset date { get; set; }


        public TodoItem(string id, Uri imageUri, string title, string description, DateTimeOffset dateTime)
        {
            this._id = id; //生成id
            this.ImageUri = imageUri;
            this.title = title;
            this.description = description;
            this.date = dateTime;
            this.completed = false; //默认为未完成
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
