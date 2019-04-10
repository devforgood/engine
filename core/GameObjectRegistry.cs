using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;

namespace core
{
    public class GameObjectRegistry
    {
        public delegate GameObject GameObjectCreationFunc();

        /// <summary>
        /// Global instance of GameObjectRegistry
        /// </summary>
        public static GameObjectRegistry sInstance = new GameObjectRegistry();

        public static void StaticInit()
        {
            sInstance = new GameObjectRegistry();
        }


        public void RegisterCreationFunction(uint32_t inFourCCName, GameObjectCreationFunc inCreationFunction)
        {
            mNameToGameObjectCreationFunctionMap[inFourCCName] = inCreationFunction;
        }

        public GameObject CreateGameObject(uint32_t inFourCCName)
        {
            //no error checking- if the name isn't there, exception!
            GameObjectCreationFunc creationFunc = mNameToGameObjectCreationFunctionMap[inFourCCName];

            var gameObject = creationFunc();

            //should the registry depend on the world? this might be a little weird
            //perhaps you should ask the world to spawn things? for now it will be like this
            World.sInstance.AddGameObject(gameObject);

            return gameObject;

        }


	    private GameObjectRegistry()
        {

        }

        Dictionary<uint32_t, GameObjectCreationFunc> mNameToGameObjectCreationFunctionMap;
    }
}
