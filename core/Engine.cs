using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public class Engine
    {

        bool mShouldKeepRunning;

        /// <summary>
        /// Global instance of Engine
        /// </summary>
        public static readonly Engine sInstance = new Engine();

        protected Engine()
        {
            mShouldKeepRunning = true;


        }



    }
}
