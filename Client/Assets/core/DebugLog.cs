using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if UNITY
using UnityEngine;
#else

#endif

namespace core
{
    public class Net
    {
#if UNITY
#else
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif 

        public static void Log(string msg)
        {
#if UNITY
            Debug.Log(msg);
#else
            log.Debug(msg);
#endif
        }

    }
}
