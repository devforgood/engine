using UnityEngine;

public class ActorBehaviour : MonoBehaviour
{
    public CActor actor = null;
    public Animator animator;

    private bool is_run = false;

    private Vector3 velocity;

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
            var remoteLocation = new Vector3(actor.GetLocation().mX, actor.GetLocation().mY, actor.GetLocation().mZ);
            var remoteVelocity = new Vector3(actor.GetVelocity().mX, actor.GetVelocity().mY, actor.GetVelocity().mZ);

            if (actor.IsLocalPlayer() == false)
            {
                transform.position = Vector3.Lerp(transform.position, remoteLocation, Time.deltaTime * 10f);
                velocity = Vector3.Lerp(velocity, remoteVelocity, Time.deltaTime * 10f);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, remoteLocation, Time.deltaTime * 10f);
                velocity = Vector3.Lerp(velocity, remoteVelocity, Time.deltaTime * 10f);
            }


            var speed = velocity.magnitude;
            animator.SetFloat("Speed", speed);


            if (speed == 0.0f)
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

                if (Input.GetKeyUp(KeyCode.Space))
                {
                    actor.InvokeServerRpc(actor.JumpServer, 2);
                }

                //if (Input.GetKeyUp(KeyCode.F))
                //{
                //    animator.Play("Bomb");
                //}


            }
        }
    }
}
