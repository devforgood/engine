using UnityEngine;

public class ActorBehaviour : MonoBehaviour
{
    public CActor actor = null;
    public Animator animator;

    private bool is_run = false;

    private Vector3 velocity;

    float local_y;
    float remote_y;
    private Vector3 position;


    // Use this for initialization
    void Start()
    {
        Debug.Log("start location  " + transform.position );
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
            Debug.Log("update() location " + transform.position.y + ", remote " + actor.GetLocation().y);

            InputManager.sInstance.GetState().mYaxis = transform.position.y;

            position = actor.GetLocation();
            position.y = transform.position.y; // y축은 서버 동기화 하지 않는다.

            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10f);
            velocity = Vector3.Lerp(velocity, actor.GetVelocity(), Time.deltaTime * 10f);

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

            //if (actor.mDirection.IsZero())
            //{

            //}
            //else
            //{
            //    Debug.Log("update() mDirection  " + actor.mDirection);
            //    actor.mDirection.y = 0;
            //    transform.rotation = Quaternion.LookRotation(actor.mDirection);
            //}



            if (actor.IsForward && actor.IsRight)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward + Vector3.right);
            }
            else if(actor.IsForward && actor.IsLeft)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward + Vector3.left);
            }
            else if (actor.IsBack && actor.IsRight)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.back + Vector3.right);
            }
            else if (actor.IsBack && actor.IsLeft)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.back + Vector3.left);
            }
            else if(actor.IsForward)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward);
            }
            else if (actor.IsBack)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.back);
            }
            else if (actor.IsLeft)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.left);
            }
            else if (actor.IsRight)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.right);
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
