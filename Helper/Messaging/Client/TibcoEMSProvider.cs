using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Messaging.Client
{
    public class TibcoEMSProvider : AbsMessagingProvider
    {


        public override Task<bool> PublishMessage(string publishQueue, string replyToQueue, string message)
        {
            return null;
        }
    }
}
