using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;

namespace core
{
    class LinkingContext
    {
        Dictionary<uint32_t, GameObject> mNetworkIdToGameObjectMap = new Dictionary<uint32_t, GameObject>();
        Dictionary< GameObject, uint32_t > mGameObjectToNetworkIdMap = new Dictionary<GameObject, uint32_t>();

	    uint32_t mNextNetworkId;

        public LinkingContext()
        {
            mNextNetworkId = 0;
        }
        public uint32_t GetNetworkId(GameObject inGameObject, bool inShouldCreateIfNotFound)
        {
            uint32_t newNetworkId;
            if (mGameObjectToNetworkIdMap.TryGetValue(inGameObject, out newNetworkId) == true)
            {
                return newNetworkId;
            }
            else if (inShouldCreateIfNotFound)
            {
                newNetworkId = mNextNetworkId++;
                AddGameObject(inGameObject, newNetworkId);
                return newNetworkId;
            }
            else
            {
                return 0;
            }
        }

        public GameObject GetGameObject(uint32_t inNetworkId)
        {
            GameObject inGameObject = null;
            if (mNetworkIdToGameObjectMap.TryGetValue(inNetworkId, out inGameObject) == false)
            {
                return inGameObject;
            }
            else
            {
                return null;
            }
        }

        public void AddGameObject(GameObject inGameObject, uint32_t inNetworkId)
        {
            mNetworkIdToGameObjectMap[inNetworkId] = inGameObject;
            mGameObjectToNetworkIdMap[inGameObject] = inNetworkId;
        }

        public void RemoveGameObject(GameObject inGameObject)
        {
            uint32_t networkId = mGameObjectToNetworkIdMap[inGameObject];
            mGameObjectToNetworkIdMap.Remove(inGameObject);
            mNetworkIdToGameObjectMap.Remove(networkId);
        }

    }
}
