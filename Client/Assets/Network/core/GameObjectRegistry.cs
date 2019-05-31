using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uint32_t = System.UInt32;

namespace core
{
    public class GameObjectRegistry
    {
        public delegate NetGameObject GameObjectCreationFunc(byte worldId);

        Dictionary<uint32_t, GameObjectCreationFunc> mNameToGameObjectCreationFunctionMap = new Dictionary<uint32_t, GameObjectCreationFunc>();


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

        public NetGameObject CreateGameObject(uint32_t inFourCCName, byte worldId)
        {
            //no error checking- if the name isn't there, exception!
            GameObjectCreationFunc creationFunc = mNameToGameObjectCreationFunctionMap[inFourCCName];

            var gameObject = creationFunc(worldId);

            //should the registry depend on the world? this might be a little weird
            //perhaps you should ask the world to spawn things? for now it will be like this
            World.Instance(worldId).AddGameObject(gameObject);

            return gameObject;

        }


	    private GameObjectRegistry()
        {

        }

    }
}
