using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public class Engine
    {

        bool mShouldKeepRunning;

        /// <summary>
        /// Global instance of Engine
        /// </summary>
        public static Engine sInstance = new Engine();

        protected Engine()
        {
            mShouldKeepRunning = true;


        }
        public void SetShouldKeepRunning(bool inShouldKeepRunning) { mShouldKeepRunning = inShouldKeepRunning; }

        protected virtual void DoFrame()
        {
            World.sInstance.Update();
        }
        public virtual int Run()
        {
            return DoRunLoop();
        }
        private int DoRunLoop()
        {
            // Main message loop
            bool quit = false;

            while (!quit && mShouldKeepRunning)

            {
                Timing.sInstance.Update();
                DoFrame();
            }

            return 0;
        }

    }
}
