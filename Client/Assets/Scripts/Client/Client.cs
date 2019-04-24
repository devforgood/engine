using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {


    public string server_ip_address = "127.0.0.1";
    public int server_port = 65000;
    public string user_id = "test";

	// Use this for initialization
	void Start () {
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kRoboCat, RoboCatClient.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kMouse, MouseClient.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kYarn, YarnClient.StaticCreate);

        var addr = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(server_ip_address), server_port);

        NetworkManagerClient.StaticInit(addr, user_id);
    }

    void FixedUpdate()
    {
        core.Timing.sInstance.Update();
        InputManager.sInstance.Update();

        NetworkManagerClient.sInstance.ProcessIncomingPackets();


        core.Engine.sInstance.DoFrame();

 
    }
    // Update is called once per frame
    void Update ()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Pressed, 'a');
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Pressed, 'd');
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Pressed, 'w');
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Pressed, 's');
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Pressed, 'k');
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Released, 'a');
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Released, 'd');
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Released, 'w');
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Released, 's');
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            InputManager.sInstance.HandleInput(core.EInputAction.EIA_Released, 'k');
        }

        NetworkManagerClient.sInstance.SendOutgoingPackets();

    }
}
