using System;
using System.Collections.Generic;
using System.Text;

namespace core
{
    public class ResponseMessage
    {

    }

    public class Session
    {
        public string session_id;
        public string remote_endpoint;
        public long user_no;
        public long rating;
    }

    public class StartPlay
    {
        public bool is_start;
        public int wait_time_sec;
        public string battle_server_addr;
    }
}
