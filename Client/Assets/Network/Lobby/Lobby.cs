using Assets.Network.Lobby;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    //public string ServiceUrl  = "https://localhost:44326/";
    public string ServiceUrl = "http://172.25.51.101/";

    GameWebRequest web;

    string session_id;

    bool is_try_startplay = false;
    int wait_time_sec = 0;

    float current_wait_time = 0.0f;



    // Start is called before the first frame update
    void Start()
    {
        web = new GameWebRequest(ServiceUrl);

        web.SendMessageAsync("Auth/Index", null, (string msg) => 
        {
            var ret = JsonUtility.FromJson<core.Session>(msg);
            session_id = ret.session_id;
            StartPlay();
        });
    }

    public void StartPlay()
    {

        var req_msg = new core.RequestMessage();
        req_msg.session_id = session_id;
        web.SendMessageAsync("Match/StartPlay", req_msg, (string msg) =>
        {
            var ret = JsonUtility.FromJson<core.StartPlay>(msg);
            if(ret.is_start)
            {
                is_try_startplay = false;
                Debug.Log("StartPlay success");
                SceneManager.LoadScene("test");
                Client.server_addr = ret.battle_server_addr;
            }
            else
            {
                is_try_startplay = true;
                wait_time_sec = ret.wait_time_sec;
            }
        });
    }




    // Update is called once per frame
    void Update()
    {
        web.Update();
        
        if(is_try_startplay)
        {
            current_wait_time += Time.deltaTime;
            if((int)current_wait_time >= wait_time_sec)
            {
                current_wait_time = 0.0f;
                StartPlay();
            }
        }
    }
}
