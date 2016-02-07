using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextPad.Model;
using Windows.ApplicationModel.Resources;

namespace TextPad.ViewModels
{
    using Windows.ApplicationModel;
    using ObservableCharsetCollection = ObservableCollection<DisplayedItem<Charset>>;

    public sealed class ApplicationViewModel : INotifyPropertyChanged
    {
        private DisplayedItem<Charset> currentCharset_;
        private Boolean saveCommandEnabled_;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler DefaultCharsetChanged;

        public ApplicationViewModel()
        {
            var resources = new ResourceLoader();

            AvailableCharsets = new ObservableCharsetCollection(new[] {
                    new DisplayedItem<Charset> { Key = Charset.UTF8, Label = resources.GetString("/Resources/Encoding_UTF8"), },
                    new DisplayedItem<Charset> { Key = Charset.UnicodeLe, Label = resources.GetString("/Resources/Encoding_UnicodeLe"), },
                    new DisplayedItem<Charset> { Key = Charset.UnicodeBe, Label = resources.GetString("/Resources/Encoding_UnicodeBe"), },
                    new DisplayedItem<Charset> { Key = Charset.Western1252, Label = resources.GetString("/Resources/Encoding_Western1252"), },
                });

            SetDefaultCharset();
        }

        /// <summary>
        /// The list of available <see cref="Charset" />.
        /// </summary>
        public ObservableCollection<DisplayedItem<Charset>> AvailableCharsets { get; private set; }

        /// <summary>
        /// The currently selected <see cref="Charset" />
        /// </summary>
        public DisplayedItem<Charset> CurrentCharset
        {
            get { return currentCharset_; }
            set
            {
                currentCharset_ = value;
                RaisePropertyChanged("CurrentCharset");
                RaiseDefaultCharsetChanged();
            }
        }

        /// <summary>
        /// The current version for this application.
        /// </summary>
        public static string CurrentVersion
        {
            get
            {
                var version = Package.Current.Id.Version;

                return string.Format("{0}.{1}.{2}.{3}"
                    , version.Major
                    , version.Minor
                    , version.Build
                    , version.Revision
                    );
            }
        }

        /// <summary>
        /// Specifies whether the SaveCommand is enabled in the CommandBar.
        /// </summary>
        public Boolean SaveCommandEnabled
        {
            get { return saveCommandEnabled_; }
            set
            {
                saveCommandEnabled_ = value;
                RaisePropertyChanged("SaveCommandEnabled");
            }
        }

        public void SetDefaultCharset()
        {
            var settings = Settings.Load();
            SetCurrentCharset(settings.DefaultCharset, false);
        }

        public void SetCurrentCharset(Charset charset, bool raisePropertyChanged = true)
        {
            var selectedCharset = AvailableCharsets.SingleOrDefault(c => c.Key == charset);
            currentCharset_ = (selectedCharset == null) ? AvailableCharsets[0] : selectedCharset;

            if (raisePropertyChanged)
                RaisePropertyChanged("CurrentCharset");
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaiseDefaultCharsetChanged()
        {
            var handler = DefaultCharsetChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
