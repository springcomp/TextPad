using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPad.Model
{
    public sealed class DisplayedItem<T>
    {
        public T Key { get; set; }
        public string Label { get; set; }
    }
}
