using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeNetwork : MonoBehaviour {

    public CActor actor = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(actor != null)
        {
            transform.position = new Vector3(actor.GetLocation().mX, actor.GetLocation().mY, actor.GetLocation().mZ);
            if(actor.IsLocalPlayer() == false)
                Debug.Log("Draw Remote Client Location : " + actor.GetLocation() + ", player_id : " + actor.GetPlayerId());


            if(actor.IsLocalPlayer())
            {
                if (Input.GetKeyUp(KeyCode.R))
                {
                    actor.InvokeServerRpc(actor.PingServer, 19);
                }


            }
        }
    }
}
