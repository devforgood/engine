using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {


    public string server_ip_address = "127.0.0.1";
    public int server_port = 65000;
    public string user_id = "test";


    Dictionary<KeyCode, bool> key_event = new Dictionary<KeyCode, bool>()
    {
        { KeyCode.A , false },
        { KeyCode.D , false },
        { KeyCode.W , false },
        { KeyCode.S , false },
        { KeyCode.K , false },
        { KeyCode.R , false },
        { KeyCode.B , false },
    };



	// Use this for initialization
	void Start () {
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kActor, CActor.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kProp, CProp.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kProjectile, CProjectile.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kBomb, CBomb.StaticCreate);

        core.Engine.sInstance.IsClient = true;
        core.Engine.sInstance.IsServer = false;

        var addr = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(server_ip_address), server_port);

        NetworkManagerClient.StaticInit(addr, user_id);
    }

    void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update ()
    {

        core.Timing.sInstance.Update();
        InputManager.sInstance.Update();
        core.Engine.sInstance.DoFrame();

        NetworkManagerClient.sInstance.ProcessIncomingPackets();

        KeyEvent(KeyCode.A);
        KeyEvent(KeyCode.D);
        KeyEvent(KeyCode.W);
        KeyEvent(KeyCode.S);
        KeyEvent(KeyCode.K);
        KeyEvent(KeyCode.R);
        KeyEvent(KeyCode.B);

    }

    void LateUpdate()
    {
        NetworkManagerClient.sInstance.SendOutgoingPackets();

    }


    void KeyEvent(KeyCode k)
    {
        bool last_key_state = key_event[k];
        if (Input.GetKey(k))
        {
            key_event[k] = true;
        }
        else if (key_event[k] == true)
        {
            key_event[k] = false;
        }

        if (last_key_state != key_event[k])
        {
            if (key_event[k])
            {
                Debug.Log("key down " + k);
                InputManager.sInstance.HandleInput(core.EInputAction.EIA_Pressed, k);
                if(k==KeyCode.R)
                {
                    NetworkManagerClient.sInstance.SendRPCPacket();
                }
            }
            else
            {
                Debug.Log("key up " + k);
                InputManager.sInstance.HandleInput(core.EInputAction.EIA_Released, k);
            }
        }
    }
}
