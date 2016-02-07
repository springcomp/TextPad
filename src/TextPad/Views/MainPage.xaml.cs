using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TextPad.Utils;
using TextPad.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TextPad.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DocumentViewModel document_;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            SetDocument();
        }

        public Frame View { get { return CurrentFrame; } }

        private void SetDocument(DocumentViewModel document = null)
        {
            if (document_ != null)
                document_.SaveCommandEnabledChanged -= Document_SaveCommandEnabledChanged;

            document_ = document ?? new DocumentViewModel();
            document_.SaveCommandEnabledChanged += Document_SaveCommandEnabledChanged;
        }

        private async Task<bool> ConfirmSavePendingChangesAsync()
        {
            if (document_.IsModified)
            {
                if (await DialogBox.ConfirmSaveChangesDialogAsync() == MessageBox.Result.No)
                    return false;

                var path = await document_.SaveAsync();
                if (path == null)
                    return false;
            }

            return true;
        }

        #region Overrides

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var path = e.Parameter as StorageFile;
            if (path == null)
                NewCommand_Click(this, null);

            else
            {
                await document_.OpenAsync(path);
                View.Navigate(typeof(DocumentView), document_);
            }
        }

        #endregion

        #region Event Handlers

        private void Document_SaveCommandEnabledChanged(object sender, EventArgs e)
        {
            SaveCommandEnabled = document_.SaveCommandEnabled;
        }

        private async void NewCommand_Click(object sender, RoutedEventArgs e)
        {
            // save modified document first

            if (!await ConfirmSavePendingChangesAsync())
                return;

            SetDocument();
            View.Navigate(typeof(DocumentView), document_);
        }

        private async void OpenCommand_Click(object sender, RoutedEventArgs e)
        {
            // save modified document first

            if (!await ConfirmSavePendingChangesAsync())
                return;

            if (await document_.OpenAsync() != null)            
                View.Navigate(typeof(DocumentView), document_);
        }

        private async void SaveCommand_Click(object sender, RoutedEventArgs e)
        {
            await document_.SaveAsync();
        }

        #endregion

        public static readonly DependencyProperty SaveCommandEnabledProperty =
            DependencyProperty.RegisterAttached(
                  "SaveCommandEnabled"
                , typeof(Boolean)
                , typeof(MainPage)
                , new PropertyMetadata(false, null))
                ;

        public bool SaveCommandEnabled
        {
            get { return (bool) GetValue(SaveCommandEnabledProperty); }
            set { this.SetValue(SaveCommandEnabledProperty, value); }
        }
    }
}
