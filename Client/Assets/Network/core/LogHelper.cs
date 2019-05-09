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
    /// <summary>
    /// Log level
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Developer logging level, most verbose
        /// </summary>
        Developer,
        /// <summary>
        /// Normal logging level, medium verbose
        /// </summary>
        Normal,
        /// <summary>
        /// Error logging level, very quiet
        /// </summary>
        Error,
        /// <summary>
        /// Nothing logging level, no logging will be done
        /// </summary>
        Nothing
    }

    /// <summary>
    /// Helper class for logging
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Gets the current log level.
        /// </summary>
        /// <value>The current log level.</value>
        public static LogLevel CurrentLogLevel
        {
            get
            {
                return LogLevel.Normal;
            }
        }



#if UNITY
#else
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif 

        public static void LogInfo(string msg)
        {
#if UNITY
            Debug.Log(msg);
#else
            log.Debug(msg);
#endif
        }

        public static void LogWarning(string msg)
        {
#if UNITY
            Debug.LogWarning(msg);
#else
            log.Warn(msg);
#endif
        }
        public static void LogError(string msg)
        {
#if UNITY
            Debug.LogError(msg);
#else
            log.Error(msg);
#endif
        }


    }
}
