using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteCameraController : MonoBehaviour
{
    public GameObject player = null;       //Public variable to store a reference to the player game object
    public Vector3 offset = new Vector3(0, 14, -10);         //Private variable to store the offset distance between the player and camera

    public void SetPlayer(GameObject p)
    {
        player = p;
        //offset = transform.position - player.transform.position;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
            transform.position = player.transform.position + offset;
    }
}
