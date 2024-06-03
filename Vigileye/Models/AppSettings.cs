using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigileye.Models
{
    public class AppSettings : INotifyPropertyChanged, IDataErrorInfo
    {
        private int _parcId;
        private int _roomId;
        private string _url;

        public int ParcId
        {
            get { return _parcId; }
            set
            {
                if (_parcId != value)
                {
                    _parcId = value;
                    OnPropertyChanged("ParcId");
                }
            }
        }

        public int RoomId
        {
            get { return _roomId; }
            set
            {
                if (_roomId != value)
                {
                    _roomId = value;
                    OnPropertyChanged("RoomId");
                }
            }
        }

        public string URL
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnPropertyChanged("URL");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "RoomId")
                {
                    if (RoomId < 0)
                    {
                        result = "RoomId doit être un entier positif.";
                    }
                }

                if (columnName == "ParcId")
                {
                    if (RoomId < 0)
                    {
                        result = "ParcId doit être un entier positif.";
                    }
                }
                return result;
            }
        }
    }
}
