using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace TextPad.Triggers
{
    public class UserInteractionModeTrigger : StateTriggerBase
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="UserInteractionModeTrigger" /> class.
        /// </summary>
        public UserInteractionModeTrigger()
        {
            Initialize();
        }

        /// <summary>
        /// Get or set the current UserInteractionMode, i.e. Touch or Mouse.
        /// </summary>
        public string CurrentUserInteractionMode
        {
            get { return (string)GetValue(UIModeProperty); }
            set { SetValue(UIModeProperty, value); }
        }

        public static readonly DependencyProperty UIModeProperty =
            DependencyProperty.Register("UIMode"
                , typeof(string)
                , typeof(UserInteractionModeTrigger),
                new PropertyMetadata("")
                );

        private void Initialize()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                WindowActivatedEventHandler windowactivated = null;
                windowactivated = (s, e) =>
                {
                    Window.Current.Activated -= windowactivated;
                    Current_SizeChanged(this, null);
                };

                Window.Current.Activated += windowactivated;
                Window.Current.SizeChanged += Current_SizeChanged;
            }
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var currentUIMode = UIViewSettings.GetForCurrentView().UserInteractionMode.ToString();
            SetActive(currentUIMode == CurrentUserInteractionMode);
        }
    }
}