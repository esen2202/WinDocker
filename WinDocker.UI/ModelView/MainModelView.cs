using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDocker.UI.ModelView
{
    public class MainModelView : INotifyPropertyChanged
    {
        #region Commands
        ICommand RightClickItem { get; set; }
        ICommand LeftClickItem { get; set; }
        ICommand OpenMenu { get; set; }
        ICommand AddItem { get; set; }
        ICommand RemoveItem { get; set; }
        ICommand ClosePopup { get; set; }

        #endregion

        #region Properties
        private int itemCount;

        public int ItemCount
        {
            get { return itemCount; }
            set
            {
                itemCount = value;
                OnPropertyChanged("ItemCount");
            }
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }


    }
}
