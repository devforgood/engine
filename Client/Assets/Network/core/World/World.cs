﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public class World
    {
        List<NetGameObject> mGameObjects = new List<NetGameObject>();
        public WorldMap mWorldMap = new WorldMap();


        /// <summary>
        /// Global instance of GameObjectRegistry
        /// </summary>
        public static World sInstance = new World();

        public static void StaticInit()
        {
            sInstance = new World();
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


        public void Update()
        {
            //update all game objects- sometimes they want to die, so we need to tread carefully...

            for (int i = 0, c = mGameObjects.Count; i < c; ++i)
            {
                NetGameObject go = mGameObjects[i];


                if (!go.DoesWantToDie())
                {
                     go.NetUpdate();
                }
                //you might suddenly want to die after your update, so check again
                if (go.DoesWantToDie())
                {
                    RemoveGameObject(go);
                    go.HandleDying();
                    --i;
                    --c;
                }
            }
        }

        public void LateUpdate()
        {
            //update all game objects- sometimes they want to die, so we need to tread carefully...

            for (int i = 0, c = mGameObjects.Count; i < c; ++i)
            {
                NetGameObject go = mGameObjects[i];
                if (!go.DoesWantToDie())
                {
                    go.LateUpdate();
                }
            }
        }

        public List<NetGameObject> GetGameObjects() { return mGameObjects; }


        World()
        {
            
        }


    }
}
