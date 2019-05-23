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
        public BEPUphysics.Space space { get; set; }
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
            space = new BEPUphysics.Space();


            // floor
            space.Add(new Box(new BEPUutilities.Vector3(0, -1.0f, 0), 30, 1, 30));
            // temp box
            space.Add(new Box(new BEPUutilities.Vector3(0, 0, -1.0f), 1, 1, 1));

            var vertices = new BEPUutilities.Vector3[]
            {
                new BEPUutilities.Vector3(-0.454f, 0.087f, -0.413f),
                new BEPUutilities.Vector3(-0.454f, 0.087f, 0.29f),
                new BEPUutilities.Vector3(-0.454f, 0.79f, -0.413f),
                new BEPUutilities.Vector3(0.454f, 0.79f, -0.413f),
                new BEPUutilities.Vector3(0.454f, 0.087f, 0.29f),
                new BEPUutilities.Vector3(0.454f, 0.087f, -0.413f),
                new BEPUutilities.Vector3(-0.333f, 1f, -0.5f),
                new BEPUutilities.Vector3(-0.333f, 0f, 0.5f),
                new BEPUutilities.Vector3(0.333f, 1f, -0.5f),
                new BEPUutilities.Vector3(0.333f, 0f, 0.5f),
                new BEPUutilities.Vector3(-0.333f, 1f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 0f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 1f, -0.5f),
                new BEPUutilities.Vector3(0.5f, 0f, -0.5f),
                new BEPUutilities.Vector3(0.333f, 1f, -0.5f),
                new BEPUutilities.Vector3(0.5f, 1f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 1f, -0.5f),
                new BEPUutilities.Vector3(-0.474f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(-0.355f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 0f, -0.5f),
                new BEPUutilities.Vector3(-0.333f, 0f, 0.5f),
                new BEPUutilities.Vector3(-0.5f, 0f, 0.5f),
                new BEPUutilities.Vector3(0.333f, 0f, 0.5f),
                new BEPUutilities.Vector3(0.5f, 0f, -0.5f),
                new BEPUutilities.Vector3(0.5f, 0f, 0.5f),
                new BEPUutilities.Vector3(0.355f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(0.474f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(0.355f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(0.474f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(-0.355f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(-0.474f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(-0.474f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(-0.355f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(0.333f, 1f, -0.5f),
                new BEPUutilities.Vector3(0.474f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(0.5f, 1f, -0.5f),
                new BEPUutilities.Vector3(0.355f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(0.474f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(0.5f, 0f, 0.5f),
                new BEPUutilities.Vector3(0.5f, 1f, -0.5f),
                new BEPUutilities.Vector3(0.474f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(0.333f, 0f, 0.5f),
                new BEPUutilities.Vector3(0.5f, 0f, 0.5f),
                new BEPUutilities.Vector3(0.474f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(0.355f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(0.355f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(0.355f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(0.333f, 1f, -0.5f),
                new BEPUutilities.Vector3(0.333f, 0f, 0.5f),
                new BEPUutilities.Vector3(-0.333f, 0f, 0.5f),
                new BEPUutilities.Vector3(-0.474f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(-0.5f, 0f, 0.5f),
                new BEPUutilities.Vector3(-0.355f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(-0.474f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(-0.474f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 1f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 0f, 0.5f),
                new BEPUutilities.Vector3(-0.355f, 0.058f, 0.5f),
                new BEPUutilities.Vector3(-0.333f, 0f, 0.5f),
                new BEPUutilities.Vector3(-0.333f, 1f, -0.5f),
                new BEPUutilities.Vector3(-0.355f, 1.058f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 0.049f, -0.451f),
                new BEPUutilities.Vector3(-0.454f, 0.79f, -0.413f),
                new BEPUutilities.Vector3(-0.5f, 0.882f, -0.451f),
                new BEPUutilities.Vector3(-0.454f, 0.087f, -0.413f),
                new BEPUutilities.Vector3(-0.5f, 0.049f, 0.382f),
                new BEPUutilities.Vector3(-0.5f, 0.882f, -0.451f),
                new BEPUutilities.Vector3(-0.454f, 0.79f, -0.413f),
                new BEPUutilities.Vector3(-0.454f, 0.087f, 0.29f),
                new BEPUutilities.Vector3(-0.5f, 0.049f, -0.451f),
                new BEPUutilities.Vector3(-0.5f, 0.049f, 0.382f),
                new BEPUutilities.Vector3(-0.454f, 0.087f, 0.29f),
                new BEPUutilities.Vector3(-0.454f, 0.087f, -0.413f),
                new BEPUutilities.Vector3(0.5f, 0.049f, -0.451f),
                new BEPUutilities.Vector3(0.5f, 0.882f, -0.451f),
                new BEPUutilities.Vector3(0.454f, 0.79f, -0.413f),
                new BEPUutilities.Vector3(0.454f, 0.087f, -0.413f),
                new BEPUutilities.Vector3(0.454f, 0.087f, 0.29f),
                new BEPUutilities.Vector3(0.5f, 0.049f, -0.451f),
                new BEPUutilities.Vector3(0.454f, 0.087f, -0.413f),
                new BEPUutilities.Vector3(0.5f, 0.049f, 0.382f),
                new BEPUutilities.Vector3(0.5f, 0.049f, 0.382f),
                new BEPUutilities.Vector3(0.454f, 0.79f, -0.413f),
                new BEPUutilities.Vector3(0.5f, 0.882f, -0.451f),
                new BEPUutilities.Vector3(0.454f, 0.087f, 0.29f),
                new BEPUutilities.Vector3(-0.5f, 0f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 0.049f, -0.451f),
                new BEPUutilities.Vector3(-0.5f, 0.882f, -0.451f),
                new BEPUutilities.Vector3(-0.5f, 0.049f, 0.382f),
                new BEPUutilities.Vector3(-0.5f, 1f, -0.5f),
                new BEPUutilities.Vector3(-0.5f, 0f, 0.5f),
                new BEPUutilities.Vector3(0.5f, 0.882f, -0.451f),
                new BEPUutilities.Vector3(0.5f, 0.049f, -0.451f),
                new BEPUutilities.Vector3(0.5f, 0f, -0.5f),
                new BEPUutilities.Vector3(0.5f, 0.049f, 0.382f),
                new BEPUutilities.Vector3(0.5f, 1f, -0.5f),
                new BEPUutilities.Vector3(0.5f, 0f, 0.5f),

            };

            int[] indices = new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 8, 7, 9, 10, 11, 12, 10, 13, 11, 10, 14, 13, 13, 14, 15, 10, 16, 17, 17, 18, 10, 19, 20, 21, 22, 20, 19, 19, 23, 22, 22, 23, 24, 25, 26, 27, 26, 25, 28, 29, 30, 31, 31, 32, 29, 33, 34, 35, 33, 36, 34, 37, 38, 39, 37, 39, 40, 41, 42, 43, 43, 44, 41, 45, 46, 47, 47, 48, 45, 49, 50, 51, 49, 52, 50, 53, 54, 55, 55, 56, 53, 57, 58, 59, 57, 59, 60, 61, 62, 63, 61, 64, 62, 65, 66, 67, 67, 68, 65, 69, 70, 71, 71, 72, 69, 73, 74, 75, 75, 76, 73, 77, 78, 79, 78, 77, 80, 81, 82, 83, 81, 84, 82, 85, 86, 87, 88, 86, 85, 85, 87, 89, 89, 87, 88, 85, 90, 88, 89, 88, 90, 91, 92, 93, 93, 92, 94, 93, 95, 91, 94, 91, 95, 93, 94, 96, 95, 96, 94
            };

            var stair = new BEPUphysics.BroadPhaseEntries.StaticMesh(vertices, indices, new BEPUutilities.AffineTransform(new BEPUutilities.Vector3(0.5f, 0.5f, 0.5f), BEPUutilities.Quaternion.Identity, new BEPUutilities.Vector3(0, 0, 0)));
            space.Add(stair);


            space.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);

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
