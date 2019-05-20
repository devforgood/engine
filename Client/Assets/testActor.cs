using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testActor : MonoBehaviour
{
    public BEPUphysics.Character.CharacterController cc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cc != null)
        {
            BEPUutilities.Vector2 totalMovement = BEPUutilities.Vector2.Zero;

            if (Input.GetKey(KeyCode.S))
                totalMovement += new BEPUutilities.Vector2(0, 1);
            if (Input.GetKey(KeyCode.W))
                totalMovement += new BEPUutilities.Vector2(0, -1);
            if (Input.GetKey(KeyCode.A))
                totalMovement += new BEPUutilities.Vector2(-1, 0);
            if (Input.GetKey(KeyCode.D))
                totalMovement += new BEPUutilities.Vector2(1, 0);

            if(Input.GetKey(KeyCode.Space))
                cc.Jump();


            if (totalMovement == BEPUutilities.Vector2.Zero)
                cc.HorizontalMotionConstraint.MovementDirection = BEPUutilities.Vector2.Zero;
            else
                cc.HorizontalMotionConstraint.MovementDirection = BEPUutilities.Vector2.Normalize(totalMovement);


            transform.position = new Vector3(cc.Body.position.X, cc.Body.position.Y, cc.Body.position.Z);
        }
    }
}
