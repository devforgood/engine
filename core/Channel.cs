using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon
{
    public enum ChannelState : uint
    {
        CHL_READY = 0,
        CHL_BUSY = 1,
    }

    public class Channel
    {
        public string channel_id;
        public string server_addr;
        public ChannelState channel_state;
        public byte world_id;
        public int user_count;
    }
}
