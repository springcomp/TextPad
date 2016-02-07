using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TextPad.Model;
using TextPad.Utils;
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

        public Encoding Encoding { get; private set; }

        public StorageFile Path { get; private set; }

        public String Filename { get; private set; }

        /// <summary>
        /// Set to True when the text is about to change
        /// due to selecting a new Charset. In that case
        /// we really don't want the document state to
        /// be modified.
        /// </summary>
        public bool IsMakingTechnicalChanges { get; internal set; }

        public bool IsModified { get; set; }

        public string Text
        {
            get { return text_; }
            set { SetTextContent(value); }
        }

        public bool SaveCommandEnabled
        {
            get { return saveCommandEnabled_; }
            set
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

        public void StopMakingTechnicalChanges()
        {
            IsMakingTechnicalChanges = false;
        }

        public async Task CreateAsync()
        {
            Reset();

            await Task.FromResult(0);
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
            SetTextContent(await ReadTextAsync(path), false);

            Path = path;
            Filename = path.Name;

            EnableSaveCommand(false);
        }

        public async Task<StorageFile> SaveAsync()
        {
            var resources = new ResourceLoader();

            var path = Path;

            if (path == null || String.IsNullOrEmpty(Filename))
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

            await SaveAsync(path);

            return path;
        }

        private async Task SaveAsync(StorageFile path)
        {
            // save target file

            // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync. 

            CachedFileManager.DeferUpdates(path);

            await WriteTextAsync(path, Text);

            // Let Windows know that we're finished changing the file so the other app can update the remote version of the file. 
            // Completing updates may require Windows to ask for user input. 

            var status = await CachedFileManager.CompleteUpdatesAsync(path);
            if (status == FileUpdateStatus.Complete)
                EnableSaveCommand(false);

            Path = path;
            Filename = path.Name;
        }

        public async Task<bool> SavePendingChangesAsync()
        {
            if (IsModified)
            {
                if (await DialogBox.ConfirmSaveChangesDialogAsync() == MessageBox.Result.No)
                    return true;

                var path = await SaveAsync();
                if (path == null)
                    return false;

                Path = path;
                Filename = Path.Name;

                return true;
            }

            return true;
        }

        public async Task<bool> SetEncodingAsync(Encoding encoding)
        {
            // re-interpret existing file with the specified encoding

            if (Path != null)
            {
                if (! await SavePendingChangesAsync())
                    return false;

                var text = await ReadTextAsync(Path, encoding);

                // since we re-interpret the text
                // the document would be "modified" whereas
                // it semantically contains the same text
                // 

                // setting IsMakingTechnicalChanges instructs
                // the view to ignore the modifications

                IsMakingTechnicalChanges = true;

                text_ = text;

                // only notify PropertyChanged on "Text"

                RaisePropertyChanged("Text");

                // force the modified state to false

                IsModified = false;
                SaveCommandEnabled = false;
            }

            // store the current encoding for later use

            Encoding = encoding;

            return true;
        }

        #endregion

        #region Implementation

        private void Reset()
        {
            Encoding = Encoding.UTF8;

            SetTextContent("", false);
            EnableSaveCommand(false);
        }

        private void SetTextContent(string text, bool raisePropertyChanged = true)
        {
            if (text_ != text)
            {
                text_ = text;

                if (raisePropertyChanged)
                {
                    RaisePropertyChanged("Text");
                    IsModified = true;
                    SaveCommandEnabled = true;
                }
            }
        }

        private async Task WriteTextAsync(StorageFile path, String text)
        {
            await WriteTextAsync(path, text, Encoding ?? Encoding.UTF8);
        }

        private async Task<String> ReadTextAsync(StorageFile path, Encoding encoding = null)
        {
            var buffer = await FileIO.ReadBufferAsync(path);
            using (var reader = DataReader.FromBuffer(buffer))
            {
                var bytes = new Byte[buffer.Length];
                reader.ReadBytes(bytes);

                // use the specified encoding for subsequent operations

                Encoding = encoding ?? DetectEncoding(bytes);
                return Encoding.GetString(bytes, 0, bytes.Length);
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
            // detect popular Byte Order Mark (BOM) sequences

            if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                return Encoding.UTF8;
            if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
                return Encoding.Unicode;
            if (buffer.Length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
                return new System.Text.UnicodeEncoding(true, true);

            // if no BOM is present, just use the default user-specified encoding

            var settings = Settings.Load();
            return EncodingFactory.GetEncoding(settings.DefaultCharset);
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
