using BEPUphysics.Entities.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public class World
    {
        List<NetGameObject> mGameObjects;
        public WorldMap mWorldMap;
        public BEPUphysics.Space space { get; set; }


        /// <summary>
        /// Global instance of GameObjectRegistry
        /// </summary>
        private static World[] sInstance = new World [] { new World() };

        public static World Instance(byte worldId)
        {
            return sInstance[worldId];
        }

        public static void StaticInit(byte world_count)
        {
            sInstance = new World [world_count];
            for (int i = 0; i < sInstance.Length; i++) { sInstance[i] = new World(); }
        }

        private World()
        {
            InitializeWrold();
        }

        private void InitializeWrold()
        {
            mGameObjects = new List<NetGameObject>();
            mWorldMap = new WorldMap();

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

        public void AddGameObject(NetGameObject inGameObject)
        {
            mGameObjects.Add(inGameObject);
            inGameObject.SetIndexInWorld(mGameObjects.Count - 1);

        }
        public void RemoveGameObject(NetGameObject inGameObject)
        {
            int index = inGameObject.GetIndexInWorld();

            int lastIndex = mGameObjects.Count - 1;
            if (index != lastIndex)
            {
                mGameObjects[index] = mGameObjects[lastIndex];
                mGameObjects[index].SetIndexInWorld(index);
            }

            inGameObject.SetIndexInWorld(-1);

            mGameObjects.RemoveAt(lastIndex);

            inGameObject.CompleteRemove();
        }


        public static void Update()
        {
            for (byte worldId = 0; worldId < sInstance.Length; ++worldId)
            {
                World world = sInstance[worldId];
                //update all game objects- sometimes they want to die, so we need to tread carefully...

                for (int i = 0, c = world.mGameObjects.Count; i < c; ++i)
                {
                    NetGameObject go = world.mGameObjects[i];


                    if (!go.DoesWantToDie())
                    {
                        go.NetUpdate();
                    }
                    //you might suddenly want to die after your update, so check again
                    if (go.DoesWantToDie())
                    {
                        world.RemoveGameObject(go);
                        go.HandleDying();
                        --i;
                        --c;
                    }
                }
            }
        }

        public static void LateUpdate()
        {
            for (byte worldId = 0; worldId < sInstance.Length; ++worldId)
            {
                World world = sInstance[worldId];

                world.space.Update();


                for (int i = 0, c = world.mGameObjects.Count; i < c; ++i)
                {
                    NetGameObject go = world.mGameObjects[i];
                    if (!go.DoesWantToDie())
                    {
                        go.LateUpdate();
                    }
                }
            }
        }

        public List<NetGameObject> GetGameObjects() { return mGameObjects; }


        public void Clear()
        {
            InitializeWrold();
        }

    }
}
