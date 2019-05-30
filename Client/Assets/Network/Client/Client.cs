using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {


    public string server_ip_address = "127.0.0.1";
    public int server_port = 65000;
    public string user_id = "test";

    public static string server_addr = null;


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

    // Handles IPv4 and IPv6 notation.
    public static System.Net.IPEndPoint CreateIPEndPoint(string endPoint)
    {
        string[] ep = endPoint.Split(':');
        if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
        System.Net.IPAddress ip;
        if (ep.Length > 2)
        {
            if (!System.Net.IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
        }
        else
        {
            if (!System.Net.IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
        }
        int port;
        if (!int.TryParse(ep[ep.Length - 1], System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.CurrentInfo, out port))
        {
            throw new FormatException("Invalid port");
        }
        return new System.Net.IPEndPoint(ip, port);
    }

    // Use this for initialization
    void Start () {
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kActor, CActor.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kProp, CProp.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kProjectile, CProjectile.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kBomb, CBomb.StaticCreate);

        core.Engine.sInstance.IsClient = true;
        core.Engine.sInstance.IsServer = false;

        System.Net.IPEndPoint addr = null;
        if (server_addr != null)
            addr = CreateIPEndPoint(server_addr);
        else
            addr = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(server_ip_address), server_port);

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
        core.Engine.sInstance.space.Update();
        core.World.sInstance.LateUpdate();

        // 클라이언트는 패킷 처리를 가장 나중에 한다 
        // 가장 마지막에 처리되는 것이 우선순위가 높음
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
            }
            else
            {
                Debug.Log("key up " + k);
                InputManager.sInstance.HandleInput(core.EInputAction.EIA_Released, k);
            }
        }
    }
}
