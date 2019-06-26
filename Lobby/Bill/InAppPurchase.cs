using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lobby.Bill
{
    public class InAppPurchase
    {
        public virtual bool VerifyReceipt(string receipt, string signature) { return false; }
    }
}
