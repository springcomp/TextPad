using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;

namespace TextPad.ViewModels
{
    public sealed class DocumentViewModel : INotifyPropertyChanged
    {
        private string text_;
        private bool saveCommandEnabled_ = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SaveCommandEnabledChanged;

        public bool IsModified { get; set; }

        public string Text
        {
            get { return text_; }
            set
            {
                if (Text != value)
                {
                    text_ = value;
                    RaisePropertyChanged("Text");

                    IsModified = true;
                    SaveCommandEnabled = true;
                }
            }
        }

        public bool SaveCommandEnabled
        {
            get { return saveCommandEnabled_; }
            private set
            {
                if (saveCommandEnabled_ != value)
                {
                    saveCommandEnabled_ = value;
                    RaisePropertyChanged("SaveCommandEnabled");
                    RaiseSaveCommandEnabledChanged();
                }
            }
        }

        #region Operations

        public async Task CreateAsync()
        {
            App.State.Clear();
            SetTextContent("");

            EnableSaveCommand(false);

            await Task.FromResult(0);
        }

        private void SetTextContent(string text)
        {
            if (Text != text)
            {
                Text = text;
                IsModified = true;
            }
        }

        public async Task<StorageFile> OpenAsync()
        {
            // select source file

            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");
            var path = await picker.PickSingleFileAsync();

            if (path == null)
                return null;

            await OpenAsync(path);

            return path;
        }

        public async Task OpenAsync(StorageFile path)
        {
            await ReadFileAsync(path);

            App.State.Storage = path;
            App.State.FileName = path.Name;

            EnableSaveCommand(false);
        }

        public async Task<StorageFile> SaveAsync()
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
                    return null;
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

            return path;
        }

        #endregion

        #region Implementation

        private async Task ReadFileAsync(StorageFile path)
        {
            Text = (await ReadTextAsync(path));
        }

        private async Task WriteFileAsync(StorageFile path)
        {
            await WriteTextAsync(path, Text, App.State.Encoding ?? Encoding.UTF8);
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
            IsModified = enabled;
            SaveCommandEnabled = enabled;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaiseSaveCommandEnabledChanged()
        {
            var handler = SaveCommandEnabledChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion
    }
}
