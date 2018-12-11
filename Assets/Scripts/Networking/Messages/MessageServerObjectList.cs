using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace MastersOfTempest.Networking
{
    struct MessageServerObjectList
    {
        public int count;                                           // 4 bytes
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public MessageServerObject[] messages;                      // 840 bytes
                                                                    // 1180 bytes
    }
}