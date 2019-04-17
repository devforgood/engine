using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {

	// Use this for initialization
	void Start () {
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kRoboCat, RoboCatClient.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kMouse, MouseClient.StaticCreate);
        core.GameObjectRegistry.sInstance.RegisterCreationFunction((uint)core.GameObjectClassId.kYarn, YarnClient.StaticCreate);
    }

    void FixedUpdate()
    {

        InputManager.Instance.Update();

        core.Engine.sInstance.DoFrame();

        NetworkManagerClient.sInstance.ProcessIncomingPackets();

    }
    // Update is called once per frame
    void Update ()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Pressed, 'a');
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Pressed, 'd');
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Pressed, 'w');
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Pressed, 's');
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Pressed, 'k');
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Released, 'a');
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Released, 'd');
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Released, 'w');
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Released, 's');
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            InputManager.Instance.HandleInput(core.EInputAction.EIA_Released, 'k');
        }

        NetworkManagerClient.sInstance.SendOutgoingPackets();

    }
}
