using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeNetwork : MonoBehaviour {

    public RoboCatClient robo = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(robo != null)
        {
            transform.position = new Vector3(robo.GetLocation().mX, robo.GetLocation().mY, robo.GetLocation().mZ);
            if(robo.IsLocalPlayer() == false)
                Debug.Log("Draw Remote Client Location : " + robo.GetLocation() + ", player_id : " + robo.GetPlayerId());
        }
    }
}
