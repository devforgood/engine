using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeNetwork : MonoBehaviour {

    public CActor actor = null;
    public Animator animator;

    private bool is_run = false;

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


            animator.SetFloat("Speed", actor.GetVelocity().magnitude);

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.nameHash == Animator.StringToHash("Base Layer.Locomotion"))
            {

            }

            if (actor.GetVelocity().IsZero() == true)
            {
                if (is_run)
                {
                    animator.Play("Locomotion");
                    is_run = false;
                }
            }
            else
            {
                is_run = true;
            }

            Debug.Log("Client speed : " + actor.GetVelocity().magnitude + ", player_id : " + actor.GetPlayerId());

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

                if(Input.GetKeyUp(KeyCode.F))
                {
                    animator.Play("Bomb");
                }


            }
        }
    }
}
