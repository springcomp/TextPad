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
    /// A page that displays the current text file.
    /// </summary>
    public sealed partial class DocumentView : Page
    {
        private DocumentViewModel document_;

        public DocumentView()
        {
            this.InitializeComponent();
        }

        #region Overrides

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            document_ = e.Parameter as DocumentViewModel;
            System.Diagnostics.Debug.Assert(document_ != null);

            document_.IsModified = false;
            DataContext = document_;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Accepts and displays TAB characters instead of setting focus to another control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewPort_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
                HandleTabKey(e);
        }

        private void HandleTabKey(KeyRoutedEventArgs e)
        {
            e.Handled = true;

            var position = ViewPort.SelectionStart;
            var text = ViewPort.Text;

            // adjust position for newline characters (\r\n) which count as two positions

            var offset = 0;
            for (var index = 0; index < position + offset; index++)
                if (text[index] == '\r') offset += 1;

            var newText = text.Substring(0, position + offset);
            newText += '\t';
            newText += text.Substring(position + offset);

            // move cursor one position

            ViewPort.Text = newText;
            ViewPort.SelectionStart = position + 1;
        }

        /// <summary>
        /// Refresh the state of the command bar buttons when text changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (document_.IsEncodingChanging)
            {
                document_.EndChangeEncoding();
                document_.IsModified = false;
                return;
            }

            document_.IsModified = true;
        }

        #endregion
    }
}
