using Assets.Network.Lobby;
using GameService;
using Grpc.Core;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        //web = new GameWebRequest(ServiceUrl);

        //web.SendMessageAsync("Auth/Index", null, (string msg) => 
        //{
        //    var ret = JsonUtility.FromJson<core.Session>(msg);
        //    session_id = ret.session_id;
        //    StartPlay();
        //});

        Login();
        var reply = StartPlay();
        if(reply.Code == GameService.ErrorCode.Success)
        {
            Debug.Log(string.Format("StartPlay success addr {0}, world_id {1}", reply.BattleServerAddr, reply.WorldId));
            SceneManager.LoadScene("test");
            Client.server_addr = reply.BattleServerAddr;
            NetworkManagerClient.sInstance.SetWorldId((byte)reply.WorldId);
        }
        else
        {

        }
    }

    void Login()
    {
        Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

        var client = new GameService.Lobby.LobbyClient(channel);
        LoginReply reply = null;

        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;

        using (var call = client.Login(new LoginRequest { Name = "" }))
        {
            var responseStream = call.ResponseStream;

            while (true)
            {
                Task<bool> t = responseStream.MoveNext(token);
                t.Wait();
                if (t.Result == false)
                {
                    break;
                }
                reply = responseStream.Current;
                session_id = reply.SessionId;
                Debug.Log(string.Format("SessionId {0}", session_id));
            }
        }



        channel.ShutdownAsync().Wait();
    }


    StartPlayReply StartPlay()
    {
        Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

        var client = new GameService.Lobby.LobbyClient(channel);
        StartPlayReply reply = null;

        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;

        using (var call = client.StartPlay(new StartPlayRequest { SessionId = session_id }))
        {
            var responseStream = call.ResponseStream;

            while (true)
            {
                Task<bool> t = responseStream.MoveNext(token);
                t.Wait();
                if (t.Result == false)
                {
                    break;
                }
                reply = responseStream.Current;


            }
        }

        channel.ShutdownAsync().Wait();

        return reply;
    }


    public void StartPlay2()
    {

        var req_msg = new core.RequestMessage();
        req_msg.session_id = session_id;
        web.SendMessageAsync("Match/StartPlay", req_msg, (string msg) =>
        {
            var ret = JsonUtility.FromJson<core.StartPlay>(msg);
            if(ret.is_start)
            {
                is_try_startplay = false;
                Debug.Log(string.Format("StartPlay success addr {0}, world_id {1}", ret.battle_server_addr, ret.world_id));
                SceneManager.LoadScene("test");
                Client.server_addr = ret.battle_server_addr;
                NetworkManagerClient.sInstance.SetWorldId(ret.world_id);
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
        return;
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
