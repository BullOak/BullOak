using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullOak.Messages
{
    public class Id : IId
    {
        public static bool operator !=(Id foo, Id bar)
        {
            return !(foo.ToString() == bar.ToString());
        }
        public static bool operator ==(Id foo, Id bar)
        {
            return foo.ToString() == bar.ToString();
        }
    }
}
