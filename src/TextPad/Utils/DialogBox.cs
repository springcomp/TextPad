using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;

namespace TextPad.Utils
{
    public sealed class DialogBox
    {
        public static async Task<MessageBox.Result> ErrorInvalidEncodingAsync()
        {
            var resources = new ResourceLoader();

            var content = resources.GetString("/Resources/MessageDialog_Error_NotATextFile_Content");
            var title = resources.GetString("/Resources/MessageDialog_Error_NotATextFile_Title");
            var ok = resources.GetString("/Resources/MessageDialog_OK");

            var result = await MessageBox.ShowAsync(
                  content
                , title
                , new UICommand(ok, cmd => { })
                );

            return result;
        }

        public static async Task<MessageBox.Result> ConfirmKeepAlternateEncodingAsync()
        {
            var resources = new ResourceLoader();

            var content = resources.GetString("/Resources/MessageDialog_Question_KeepAlternateEncoding_Content");
            var title = resources.GetString("/Resources/MessageDialog_Question_KeepAlternateEncoding_Title");
            var yes = resources.GetString("/Resources/MessageDialog_Yes");
            var no = resources.GetString("/Resources/MessageDialog_No");

            var result = await MessageBox.ShowAsync(
                  content
                , title
                , new UICommand(yes, cmd => { })
                , new UICommand(no, cmd => { })
                );

            return result;
        }

        public static async Task<MessageBox.Result> ConfirmSaveChangesDialogAsync()
        {
            var resources = new ResourceLoader();

            var content = resources.GetString("/Resources/MessageDialog_Question_SaveChanges_Content");
            var title = resources.GetString("/Resources/MessageDialog_Question_SaveChanges_Title");
            var yes = resources.GetString("/Resources/MessageDialog_Yes");
            var no = resources.GetString("/Resources/MessageDialog_No");
            var cancel = resources.GetString("/Resources/MessageDialog_Cancel");

            var result = await MessageBox.ShowAsync(
                  content
                , title
                , new UICommand(yes, cmd => { })
                , new UICommand(no, cmd => { })
                , new UICommand(cancel, cmd => { })
                );

            return result;
        }
    }
}
