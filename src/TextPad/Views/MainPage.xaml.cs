using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TextPad.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Context context_ = new Context();

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = context_;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var path = e.Parameter as StorageFile;
            if (path == null)
                return;

            await ReadFileAsync(path);
        }

        private async Task ReadFileAsync(StorageFile path)
        {
            SetTextContent(await ReadTextAsync(path));
        }

        private async Task WriteFileAsync(StorageFile path)
        {
            await WriteTextAsync(path, ViewPort.Text, App.State.Encoding ?? Encoding.UTF8);
        }

        private async Task<String> ReadTextAsync(StorageFile path)
        {
            var buffer = await FileIO.ReadBufferAsync(path);
            using (var reader = DataReader.FromBuffer(buffer))
            {
                var bytes = new Byte[buffer.Length];
                reader.ReadBytes(bytes);

                var encoding = DetectEncoding(bytes);
                return encoding.GetString(bytes, 0, bytes.Length);
            }
        }

        private async Task WriteTextAsync(StorageFile path, string text, Encoding encoding)
        {
            var bytes = encoding.GetBytes(text);
            var buffer = bytes.AsBuffer();

            await FileIO.WriteBufferAsync(path, buffer);
        }

        private Encoding DetectEncoding(byte[] buffer)
        {
            if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                return Encoding.UTF8;
            if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
                return Encoding.Unicode;
            if (buffer.Length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
                return new System.Text.UnicodeEncoding(true, true);

            return Encoding.UTF8;
        }

        private void EnableSaveCommand(bool enabled = true)
        {
            App.State.Modified = enabled;
            context_.SaveCommandEnabled = enabled;
        }

        private void SetTextContent(string text)
        {
            // momentarily disable the TextChanged event

            ViewPort.TextChanged -= ViewPort_TextChanged;

            try
            {
                ViewPort.Text = text;
            }
            finally
            {
                ViewPort.TextChanged += ViewPort_TextChanged;
            }
        }

        /// <summary>
        /// Accepts and displays TAB characters instead of setting focus to another control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewPort_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            var ctrl = (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            if (e.Key == VirtualKey.Tab)
                HandleTabKey(e);

            if (e.Key == VirtualKey.N && ctrl)
                NewCommand_Click(sender, null);

            if (e.Key == VirtualKey.O && ctrl)
                OpenCommand_Click(sender, null);

            if (e.Key == VirtualKey.S && ctrl)
                SaveCommand_Click(sender, null);
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

        private void ViewPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableSaveCommand();
        }

        private async void NewCommand_Click(object sender, RoutedEventArgs e)
        {
            if (App.State.Modified)
            {
                var resources = new ResourceLoader();
                var content = resources.GetString("/Resources/MessageDialog_ConfirmSaveChanges_Content");
                var title = resources.GetString("/Resources/MessageDialog_ConfirmSaveChanges_Title");
                var yes = resources.GetString("/Resources/MessageDialog_Yes");
                var no = resources.GetString("/Resources/MessageDialog_No");

                var result = await MessageBox.ShowAsync(
                      content
                    , title
                    , new UICommand(yes, cmd => { })
                    , new UICommand(no, cmd => { })
                    );

                if (result == MessageBox.Result.Yes)
                    SaveCommand_Click(sender, null);
            }

            App.State.Clear();
            SetTextContent("");

            EnableSaveCommand(false);
        }

        private async void OpenCommand_Click(object sender, RoutedEventArgs e)
        {
            // select source file

            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");
            var path = await picker.PickSingleFileAsync();

            if (path == null)
                return;

            await ReadFileAsync(path);

            App.State.Storage = path;
            App.State.FileName = path.Name;

            EnableSaveCommand(false);
        }

        private async void SaveCommand_Click(object sender, RoutedEventArgs e)
        {
            var resources = new ResourceLoader();

            StorageFile path = null;

            if (App.State.Storage != null && !String.IsNullOrEmpty(App.State.FileName))
                path = App.State.Storage;

            else
            {
                // select target file

                var textDocument = resources.GetString("/Resources/FileSavePicker_FileTypeChoice_TextDocument");

                var picker = new FileSavePicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.DefaultFileExtension = ".txt";
                picker.FileTypeChoices.Add(textDocument, new[] { ".txt" });
                path = await picker.PickSaveFileAsync();

                if (path == null)
                    return;
            }

            // save target file

            // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync. 

            CachedFileManager.DeferUpdates(path);

            await WriteFileAsync(path);

            // Let Windows know that we're finished changing the file so the other app can update the remote version of the file. 
            // Completing updates may require Windows to ask for user input. 

            var status = await CachedFileManager.CompleteUpdatesAsync(path);
            if (status == FileUpdateStatus.Complete)
                EnableSaveCommand(false);

            App.State.FileName = path.Name;
        }
    }

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

    public sealed class MessageBox
    {
        public static async Task<Result> ShowAsync(string content, string caption, UICommand yesCommand, UICommand noCommand, UICommand cancelCommand = null)
        {
            var dialog = new MessageDialog(content);
            dialog.Options = MessageDialogOptions.None;
            dialog.Title = caption;

            dialog.Commands.Add(yesCommand);
            dialog.Commands.Add(noCommand);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            if (cancelCommand != null)
            {
                dialog.Commands.Add(cancelCommand);
                dialog.CancelCommandIndex = 2;
            }
            
            var command = await dialog.ShowAsync();

            if (command == yesCommand)
                return Result.Yes;
            else if (command == noCommand)
                return Result.No;

            return Result.Cancel;
        }

        public enum Result
        {
            OK = 0,
            Yes = 0,
            No = 1,
            Cancel = 2,
        }
    }
}
