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
        public DocumentView()
        {
            this.InitializeComponent();
            SetDataContext();
        }

        private DocumentViewModel Document
        {
            get { return DataContext as DocumentViewModel; }
        }

        private void SetDataContext(DocumentViewModel document = null)
        {
            DataContext = document ?? new DocumentViewModel();
        }

        #region Overrides

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var document = e.Parameter as DocumentViewModel;
            System.Diagnostics.Debug.Assert(document != null);

            SetDataContext(document);
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

        #endregion

        private void ViewPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Document.IsMakingTechnicalChanges)
            {
                Document.StopMakingTechnicalChanges();
                return;
            }

            // only use this event to refresh the state of command bar buttons

            Document.IsModified = true;
            Document.SaveCommandEnabled = true;
        }
    }
}
