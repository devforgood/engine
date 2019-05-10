using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeNetwork : MonoBehaviour {

    public CActor actor = null;
    public Animator animator;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(actor != null)
        {
            var last_position = transform.position;
            transform.position = new Vector3(actor.GetLocation().mX, actor.GetLocation().mZ, actor.GetLocation().mY);

            if(last_position != transform.position)
            {
                animator.SetFloat("Speed", 1);
            }
            else
            {
                animator.Play("Locomotion");
            }

            //if(actor.IsLocalPlayer() == false)
            //Debug.Log("Draw Remote Client Location : " + actor.GetLocation() + ", player_id : " + actor.GetPlayerId());

            if (actor.mDirection.mX == 0.0f && actor.mDirection.mY == 0.0f)
            {

            }
            else
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(actor.mDirection.mX, 0, actor.mDirection.mY));
            }

            if (actor.IsLocalPlayer())
            {
                if (Input.GetKeyUp(KeyCode.R))
                {
                    actor.InvokeServerRpc(actor.PingServer, 19);
                }


            }
        }
    }
}
