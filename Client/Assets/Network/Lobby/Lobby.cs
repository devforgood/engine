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
    GameWebRequest web = new GameWebRequest();

    // Start is called before the first frame update
    void Start()
    {

        web.SendMessageAsync("Auth/Index", null, (string msg) => 
        {
            var ret = JsonUtility.FromJson<core.Session>(msg);

            var msg2 = new core.RequestMessage();
            msg2.session_id = ret.session_id;


            web.SendMessageAsync("Match/StartPlay", msg2);

        });
    }



    // Update is called once per frame
    void Update()
    {
        web.Update();
        
    }
}
