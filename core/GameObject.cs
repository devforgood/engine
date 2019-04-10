using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;
using uint8_t = System.Byte;

namespace core
{
    public class GameObject
    {
        public virtual uint32_t GetClassId() { return 1; } 

        public static GameObject CreateInstance() { return new GameObject(); }


    }
}
