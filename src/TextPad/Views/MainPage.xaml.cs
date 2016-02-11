using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TextPad.Model;
using TextPad.Services;
using TextPad.Services.Interop;
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
        private ApplicationViewModel application_ = new ApplicationViewModel();
        private DocumentViewModel document_;

        private ISettingsService settingsService_;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = application_;

            SetDocument();
        }

        public Frame View { get { return CurrentFrame; } }

        private void SetDocument(DocumentViewModel document = null)
        {
            document_ = document ?? new DocumentViewModel();
        }

        private async Task SetDefaultCharsetAsync()
        {
            var newCharset = application_.CurrentCharset.Key;
            var currentCharset = settingsService_.DefaultCharset;

            // save new default charset

            if (currentCharset != newCharset)
            {
                // re-interpret the current document with the new charset

                if (!await SetDefaultCharsetAsync(newCharset))
                {
                    // if the document had pending changes and the user
                    // canceled the save operation, we restore the
                    // original charset setting.

                    // make sure to raise a PropertyChanged event
                    // to restore the combo box selection

                    //viewModel_.SetCurrentCharset(currentCharset, true);
                    return;
                }

                // otherwise, we save the new default charset setting

                settingsService_.DefaultCharset = application_.CurrentCharset.Key;
            }
        }

        private async Task<bool> SetDefaultCharsetAsync(Charset charset)
        {
            var encoding = EncodingHelper.GetEncoding(charset);
            return await document_.SetEncodingAsync(encoding);
        }

        private async Task OnOpenCommandAsync()
        {
            // save modified document first

            if (!await document_.SavePendingChangesAsync())
                return;

            if (await document_.OpenAsync() != null)
                View.Navigate(typeof(DocumentView), document_);
        }

        private async Task OnSaveCommandAsync()
        {
            await document_.SaveAsync();
        }

        private async Task OnNewCommandAsync()
        {
            // save modified document first

            if (!await document_.SavePendingChangesAsync())
                return;

            SetDocument();
            View.Navigate(typeof(DocumentView), document_);
        }

        #region Overrides

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (settingsService_ == null)
                settingsService_ = ServiceRepository.Instance.Settings;

            await SetDefaultCharsetAsync();

            var path = e.Parameter as StorageFile;
            if (path == null)
                await document_.CreateAsync();

            else
                await document_.OpenAsync(path);

            View.Navigate(typeof(DocumentView), document_);
        }

        #endregion

        #region Event Handlers

        private async void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            var ctrl = (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            if (e.Key == VirtualKey.N && ctrl)
                await OnNewCommandAsync();

            if (e.Key == VirtualKey.O && ctrl)
                await OnOpenCommandAsync();

            if (e.Key == VirtualKey.S && ctrl)
                await OnSaveCommandAsync();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private async void NewCommand_Click(object sender, RoutedEventArgs e)
        {
            await OnNewCommandAsync();
        }

        private async void OpenCommand_Click(object sender, RoutedEventArgs e)
        {
            await OnOpenCommandAsync();
        }

        private async void SaveCommand_Click(object sender, RoutedEventArgs e)
        {
            await OnSaveCommandAsync();
        }

        private async void DefaultCharset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // request a change of default charset

            var selectedCharset = ((DisplayedItem<Charset>)DefaultCharset.SelectedItem).Key;

            // this asks the currently opened document to re-interpret its contents
            // with the new charset.

            var encoding = EncodingHelper.GetEncoding(selectedCharset);
            var succeeded = await document_.SetEncodingAsync(encoding);

            // if replacement of unrecognized characters occurred during
            // decoding, ask the user whether to keep the document or
            // try another one

            if (!succeeded)
            {
                if (await DialogBox.ConfirmKeepAlternateEncodingAsync() != MessageBox.Result.Yes)
                {
                    // reset the combo box selection
                    // to its original value. Since the data binding for the 
                    // combo box is 'OneWay' the underlying value did not change
                    // when the combo box selection changed.

                    DefaultCharset.SelectedItem = application_.CurrentCharset;
                    return;
                }
            }

            // if that succeeds, we change the underlying settings.
            // because the settings service raises an event, subscribers can
            // be notified of the actual charset change.

            settingsService_.DefaultCharset = selectedCharset;
        }

        #endregion
    }
}
