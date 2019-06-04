using core;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerMonitor
    {
        /// <summary>
        /// Global instance of ChannelMgr
        /// </summary>
        public static ServerMonitor sInstance = new ServerMonitor();


        public ConnectionMultiplexer cache = null;
        public float last_update_time = 0.0f;
        public TimeSpan channel_info_expire = new TimeSpan(0, 1, 0);
        public TimeSpan server_info_expire = new TimeSpan(0, 1, 0);

        ServerCommon.Channel[] channel_list;


        public ServerCommon.ServerInfo server_info = new ServerCommon.ServerInfo();



        public void Init(byte channel_count, string server_addr, string cache_server_addr, string server_name)
        {
            channel_list = new ServerCommon.Channel[channel_count];

            cache = ConnectionMultiplexer.Connect(cache_server_addr);

            var db = cache.GetDatabase();

            for (byte i = 0; i < channel_count; ++i)
            {
                channel_list[i] = new ServerCommon.Channel
                {
                    channel_id = string.Format("channel:{0}", db.StringIncrement("channel_instance_id")),
                    channel_state = ServerCommon.ChannelState.CHL_READY,
                    server_addr = server_addr,
                    world_id = i
                };

                db.HashSet("channel_info", string.Format("{0}:{1}", server_addr, i), JsonConvert.SerializeObject(channel_list[i]));
            }

            //server_info.server_addr = server_addr;
            //server_info.server_name = server_name;
            //server_info.server_id = server_name + ":" + Convert.ToString(db.StringIncrement(ServerCommon.ServerInfoRedisKey.server_instance_id));

            //db.HashSet("server_info", server_info.server_addr, JsonConvert.SerializeObject(server_info));

        }


        public void Update()
        {
            last_update_time += Timing.sInstance.GetDeltaTime();
            if (last_update_time > 10.0f)
            {
                Task.Run(() =>
                {
                    // channel info update
                    var db = cache.GetDatabase();
                    for (byte i = 0; i < channel_list.Length; ++i)
                    {
                        db.StringSet(channel_list[i].channel_id, (int)channel_list[i].channel_state, channel_info_expire);
                    }

                    // server info update
                    //db.StringSet(server_info.server_id, NetworkManagerServer.sInstance.GetPlayerCount(), server_info_expire);
                });

                last_update_time = 0.0f;
            }
        }


        public bool AddUser(byte world_id)
        {
            if (channel_list.Length <= world_id)
                return false;

            ++channel_list[world_id].user_count;

            if (channel_list[world_id].user_count >= 4)
                channel_list[world_id].channel_state = ServerCommon.ChannelState.CHL_BUSY;

            return true;
        }

        public int DelUser(byte world_id)
        {
            --channel_list[world_id].user_count;

            if (channel_list[world_id].user_count == 0)
                channel_list[world_id].channel_state = ServerCommon.ChannelState.CHL_READY;

            return channel_list[world_id].user_count;
        }
    }
}
