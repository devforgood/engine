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
        GameWebRequest web = new GameWebRequest();

        

        web.SendMessageAsync("Auth/Index", null, (string msg) => 
        {
            var ret = JsonUtility.FromJson<core.Session>(msg);

        });
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
