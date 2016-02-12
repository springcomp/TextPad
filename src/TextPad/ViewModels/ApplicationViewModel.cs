using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TextPad.Model;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;

using TextPad.Services;
using TextPad.Services.Interop;

namespace TextPad.ViewModels
{
    using ObservableCharsetCollection = ObservableCollection<DisplayedItem<Charset>>;

    public sealed class ApplicationViewModel : INotifyPropertyChanged
    {
        private readonly ISettingsService settingsService_;
        private readonly IToolbarStateService toolbarStateService_;

        public event PropertyChangedEventHandler PropertyChanged;

        public ApplicationViewModel()
            : this(
                    ServiceRepository.Instance.Settings
                  , ServiceRepository.Instance.ToolbarState
                  )
        {
        }

        public ApplicationViewModel(
              ISettingsService settingsService
            , IToolbarStateService toolbarStateService
            )
        {
            var resources = new ResourceLoader();

            AvailableCharsets = new ObservableCharsetCollection(new[] {
                    new DisplayedItem<Charset> { Key = Charset.UTF8, Label = resources.GetString("/Resources/Encoding_UTF8"), },
                    new DisplayedItem<Charset> { Key = Charset.UnicodeLe, Label = resources.GetString("/Resources/Encoding_UnicodeLe"), },
                    new DisplayedItem<Charset> { Key = Charset.UnicodeBe, Label = resources.GetString("/Resources/Encoding_UnicodeBe"), },
                    new DisplayedItem<Charset> { Key = Charset.CentralEasternEurope, Label = resources.GetString("/Resources/Encoding_CentralEasternEurope_1250"), },
                    new DisplayedItem<Charset> { Key = Charset.WesternEurope, Label = resources.GetString("/Resources/Encoding_WesternEurope_1252"), },
                    new DisplayedItem<Charset> { Key = Charset.Greece, Label = resources.GetString("/Resources/Encoding_Greece_1253"), },
                });

            // register an event for changes to the DefaultCharset Setting property.

            settingsService_ = settingsService;
            settingsService_.DefaultCharsetChanged += SettingsService__DefaultCharsetChanged;

            // register an event for changes to the SaveCommandEnabled state.

            toolbarStateService_ = toolbarStateService;
            toolbarStateService_.SaveCommandEnabledChanged += ToolbarStateService__SaveCommandEnabledChanged;
            toolbarStateService_.IsPaneOpenChanged += ToolbarStateService__IsPaneOpenChanged;
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
            get
            {
                return AvailableCharsets.Single(c => c.Key == settingsService_.DefaultCharset);
            }
            set
            {
                if (settingsService_.DefaultCharset != value.Key)
                {
                    settingsService_.DefaultCharset = value.Key;
                    RaisePropertyChanged("CurrentCharset");
                }
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
        /// Specifies whether the left NavigationPane is opened.
        /// </summary>
        public Boolean IsPaneOpen
        {
            get { return toolbarStateService_.IsPaneOpen; }
            set
            {
                if (toolbarStateService_.IsPaneOpen != value)
                {
                    toolbarStateService_.IsPaneOpen = value;
                    RaisePropertyChanged("IsPaneOpen");
                    System.Diagnostics.Debug.WriteLine("IsPaneOpen = {0}", toolbarStateService_.IsPaneOpen);
                }
            }
        }

        /// <summary>
        /// Specifies whether the SaveCommand is enabled in the CommandBar.
        /// </summary>
        public Boolean SaveCommandEnabled
        {
            get { return toolbarStateService_.SaveCommandEnabled; }
            set
            {
                if (toolbarStateService_.SaveCommandEnabled != value)
                {
                    toolbarStateService_.SaveCommandEnabled = value;
                    RaisePropertyChanged("SaveCommandEnabled");
                }
            }
        }

        private void SettingsService__DefaultCharsetChanged(object sender, EventArgs e)
        {
            // the default charset setting changed
            // raise a property change to update the
            // combo box in the navigation pane.

            RaisePropertyChanged("CurrentCharset");
        }

        private void ToolbarStateService__IsPaneOpenChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("IsPaneOpen");
        }

        private void ToolbarStateService__SaveCommandEnabledChanged(object sender, EventArgs e)
        {
            // The save command enabled state changed.
            // raise a property change to update the
            // combo box in the toolbar

            RaisePropertyChanged("SaveCommandEnabled");
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
