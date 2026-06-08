using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace F1
{
    public class TrackNote //: INotifyPropertyChanged
    {
        private int _id;
        private string _track;
        private string _notes;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                //OnPropertyChanged(); 
            }
        }

        public string Track
        {
            get => _track;
            set
            {
                _track = value; 
                //OnPropertyChanged();
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value; 
                //OnPropertyChanged();
            }
        }
        //public event PropertyChangedEventHandler PropertyChanged;

        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
