using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPad.ViewModels
{
    public sealed class Context : INotifyPropertyChanged
    {
        private bool saveCommandEnabled_;

        public event PropertyChangedEventHandler PropertyChanged;

        public Boolean SaveCommandEnabled
        {
            get { return saveCommandEnabled_; }
            set
            {
                if (value != saveCommandEnabled_)
                {
                    saveCommandEnabled_ = value;
                    RaisePropertyChanged("SaveCommandEnabled");
                }
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
