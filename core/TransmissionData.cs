using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public abstract class TransmissionData
    {
        public abstract void HandleDeliveryFailure(DeliveryNotificationManager inDeliveryNotificationManager);
        public abstract void HandleDeliverySuccess(DeliveryNotificationManager inDeliveryNotificationManager);
    }
}
