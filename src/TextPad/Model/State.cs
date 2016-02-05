using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TextPad.Model
{
    public sealed class State
    {
        public Encoding Encoding { get; set; }
        public String FileName { get; set; }
        public OpenState OpenState { get; set; }
        public StorageFile Storage { get; set; }
        public Boolean Modified { get; set; }

        internal void Clear()
        {
            Encoding = null;
            FileName = null;
            OpenState = OpenState.Opened;
            Storage = null;
            Modified = false;
        }
    }

    public enum OpenState
    {
        Opened = 0,
        Launched = 1,
    }
}
