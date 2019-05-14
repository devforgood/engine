﻿using UnityEngine;

public class ActorBehaviour : MonoBehaviour
{
    public CActor actor = null;
    public Animator animator;

    private bool is_run = false;

    // Use this for initialization
    void Start()
    {

    }

    public void PlayAnimation(string name)
    {
        animator.Play(name);
    }

    // Update is called once per frame
    void Update()
    {
        if (actor != null)
        {
            var last_position = transform.position;
            transform.position = new Vector3(actor.GetLocation().mX, actor.GetLocation().mY, actor.GetLocation().mZ);


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

            //Debug.Log("Client speed : " + actor.GetVelocity().magnitude + ", player_id : " + actor.GetPlayerId());

            if (actor.mDirection.IsZero())
            {

            }
            else
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(actor.mDirection.mX, actor.mDirection.mY, actor.mDirection.mZ));
            }

            if (actor.IsLocalPlayer())
            {
                if (Input.GetKeyUp(KeyCode.R))
                {
                    actor.InvokeServerRpc(actor.PingServer, 19);
                }

                //if (Input.GetKeyUp(KeyCode.F))
                //{
                //    animator.Play("Bomb");
                //}


            }
        }
    }
}
