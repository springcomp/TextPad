using TextPad.Services.Interop;

namespace TextPad.Services
{
    public sealed class ServiceRepository
    {
        private static ServiceRepository instance_ = new ServiceRepository();

        private IToolbarStateService toolbarStateService_;

        static ServiceRepository()
        {
        }

        private ServiceRepository()
        {
        }

        public static ServiceRepository Instance
        {
            get { return instance_; }
        }

        public IToolbarStateService ToolbarState
        {
            get
            {
                if (toolbarStateService_ == null)
                    toolbarStateService_ = new ToolbarStateService();
                return toolbarStateService_;
            }
        }
    }
}
