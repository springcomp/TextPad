using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI.Popups;

namespace TextPad.Utils
{
    public sealed class MessageBox
    {
        public static async Task<Result> ShowAsync(
              string content
            , string caption
            , UICommand yesCommand
            , UICommand noCommand = null
            , UICommand cancelCommand = null
            )
        {
            var dialog = new MessageDialog(content);
            dialog.Options = MessageDialogOptions.None;
            dialog.Title = caption;

            dialog.Commands.Add(yesCommand);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;

            if (noCommand != null)
            {
                dialog.Commands.Add(noCommand);
                dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
            }

            if (cancelCommand != null)
            {
                // Devices with a hardware back button
                // use the hardware button for Cancel.
                // for other devices, show a third option

                if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    // disable the default Cancel command index
                    // so that dialog.ShowAsync() returns null
                    // in that case

                    dialog.CancelCommandIndex = UInt32.MaxValue;
                }
                else
                {
                    dialog.Commands.Add(cancelCommand);
                    dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
                }
            }

            var command = await dialog.ShowAsync();

            if (command == null && cancelCommand != null)
            {
                // back button was pressed
                // invoke the UICommand

                cancelCommand.Invoked(cancelCommand);
                return Result.Cancel;
            }

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
