using Assets.Network.Lobby;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Lobby : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GameWebRequest gwr = new GameWebRequest();

        string str = "good";
        byte[] buff = Encoding.UTF8.GetBytes(str);
        gwr.SendMessageAsync(buff);


    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
