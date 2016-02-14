using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            if (noCommand != null && cancelCommand != null)
            {
                // Windows Phone Devices only support two commands
                // so we use "Cancel" instead of "No" in that case

                var deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
                if (deviceFamily == "Windows.Mobile")
                    noCommand = null;
            }

            if (noCommand != null)
            {
                dialog.Commands.Add(noCommand);
                dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
            }

            if (cancelCommand != null)
            {
                dialog.Commands.Add(cancelCommand);
                dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
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
