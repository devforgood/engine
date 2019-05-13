using core;
using Lidgren.Network;

using uint8_t = System.Byte;
using uint32_t = System.UInt32;


public class ReplicationManagerClient
{

    public void Read(NetIncomingMessage inInputStream )
    {
        while (inInputStream.LengthBits - inInputStream.Position >= 32)
        {
            //read the network id...
            int networkId = inInputStream.ReadInt32();

            //only need 2 bits for action...
            core.ReplicationAction action;
            inInputStream.Read(out action);

            switch (action)
            {
                case core.ReplicationAction.RA_Create:
                    ReadAndDoCreateAction(inInputStream, networkId);
                    break;
                case core.ReplicationAction.RA_Update:
                    ReadAndDoUpdateAction(inInputStream, networkId);
                    break;
                case core.ReplicationAction.RA_Destroy:
                    ReadAndDoDestroyAction(inInputStream, networkId);
                    break;
            }

        }

    }

    void ReadAndDoCreateAction(NetIncomingMessage inInputStream, int inNetworkId)
    {
        //need 4 cc
        uint32_t fourCCName = inInputStream.ReadUInt32();

        //we might already have this object- could happen if our ack of the create got dropped so server resends create request 
        //( even though we might have created )
        NetGameObject gameObject = NetworkManagerClient.sInstance.GetGameObject(inNetworkId);
        if (gameObject == null)
        {
            //create the object and map it...
            gameObject = GameObjectRegistry.sInstance.CreateGameObject(fourCCName);
            gameObject.SetNetworkId(inNetworkId);
            NetworkManagerClient.sInstance.AddToNetworkIdToGameObjectMap(gameObject);

            //it had really be the rigth type...
            //Assert(gameObject.GetClassId() == fourCCName);
        }

        //and read state
        gameObject.Read(inInputStream);
        gameObject.CompleteCreate();
    }

    void ReadAndDoUpdateAction(NetIncomingMessage inInputStream, int inNetworkId)
    {
        //need object
        NetGameObject gameObject = NetworkManagerClient.sInstance.GetGameObject(inNetworkId);

        //gameObject MUST be found, because create was ack'd if we're getting an update...
        //and read state
        gameObject.Read(inInputStream);
    }

    void ReadAndDoDestroyAction(NetIncomingMessage inInputStream, int inNetworkId)
    {
        //if something was destroyed before the create went through, we'll never get it
        //but we might get the destroy request, so be tolerant of being asked to destroy something that wasn't created
        NetGameObject gameObject = NetworkManagerClient.sInstance.GetGameObject(inNetworkId);
        if (gameObject != null)
        {
            gameObject.SetDoesWantToDie(true);
            NetworkManagerClient.sInstance.RemoveFromNetworkIdToGameObjectMap(gameObject);
        }
    }
}

