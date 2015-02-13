using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//using TIBCO.EMS;

namespace TibClient
{
    public class HeartbeatPublisher
    {
        //# region Private Properties
        //private TopicConnectionFactory _factory;
        //private TopicConnection _conn;
        //private TopicSession _session;
        //private TopicPublisher _publisher;

        //private CancellationTokenSource _cancTokenSource;
        //# endregion

        //# region constructor
        //public HeartbeatPublisher()
        //{
        //    _factory = new TopicConnectionFactory("tcp://vm-1af2-ae01.nam.nsroot.net:39422");
        //    _conn = _factory.CreateTopicConnection("speedit", "sp33dit");
        //    _session = _conn.CreateTopicSession(false, Session.AUTO_ACKNOWLEDGE);

        //    Topic _t = new Topic("speed.app.heartbeat");
        //    _publisher = _session.CreatePublisher(_t);

        //    _cancTokenSource = new CancellationTokenSource();
        //}
        //# endregion

        //# region 
        //public void StartPublisher()
        //{
        //    Task.Factory.StartNew(() =>
        //    {
        //        while (true)
        //        {
        //            var _m = _session.CreateTextMessage(string.Format("Heartbeat [{0}]", DateTime.Now.Ticks));
        //            _publisher.Publish(_m);
        //        }                
        //    }, _cancTokenSource.Token)
        //    .ContinueWith(_t => 
        //    {

        //    }, TaskContinuationOptions.OnlyOnFaulted);
        //}
        //public void StopPublisher()
        //{
        //    _cancTokenSource.Cancel();
        //    _conn.Close();
        //}
        //# endregion
    }
}
