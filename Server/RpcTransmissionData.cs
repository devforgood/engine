﻿using core;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class RpcTransmissionData : TransmissionData
    {
        public NetOutgoingMessage mOutputStream;

        public override void HandleDeliveryFailure(DeliveryNotificationManager inDeliveryNotificationManager)
        {

        }

        public override void HandleDeliverySuccess(DeliveryNotificationManager inDeliveryNotificationManager)
        {

        }

    }
}