using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public class World
    {
        List<GameObject> mGameObjects = new List<GameObject>();


        /// <summary>
        /// Global instance of GameObjectRegistry
        /// </summary>
        public static World sInstance = new World();

        public static void StaticInit()
        {
            sInstance = new World();
        }

        public void AddGameObject(GameObject inGameObject)
        {
            mGameObjects.Add(inGameObject);
            inGameObject.SetIndexInWorld(mGameObjects.Count - 1);

        }
        public void RemoveGameObject(GameObject inGameObject)
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
        }


        public void Update()
        {
            //update all game objects- sometimes they want to die, so we need to tread carefully...

            for (int i = 0, c = mGameObjects.Count; i < c; ++i)
            {
                GameObject go = mGameObjects[i];


                if (!go.DoesWantToDie())
                {
                    go.Update();
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

        public List<GameObject> GetGameObjects() { return mGameObjects; }


        World()
        {

        }


    }
}
