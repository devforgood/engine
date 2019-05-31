using BEPUphysics.Entities.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace core
{
    public class Engine
    {
        public int ServerClientId { get; set; }
        public bool IsServer { get; set; }
        public bool IsClient { get; set; }

        public bool IsRunning
        {
            get { return mShouldKeepRunning; }
        }

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

        public virtual void DoFrame()
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

                Thread.Sleep(1);

            }

            return 0;
        }

    }
}
