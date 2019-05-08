using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RpcTransmissionData : core.TransmissionData
{
    public NetOutgoingMessage mOutputStream;

    public override void HandleDeliveryFailure(core.DeliveryNotificationManager inDeliveryNotificationManager)
    {

    }

    public override void HandleDeliverySuccess(core.DeliveryNotificationManager inDeliveryNotificationManager)
    {

    }
}
