using System;
using System.Collections.Generic;
using System.Text;

namespace MinerProxy2.Interfaces
{
    interface ICoinHandler
    {
        void SendToAllMiners();
        void SendToMiner();
        void OnDisconnect();
        void OnConnect();
        void OnError();
        void OnShareRejected();
        void OnShareSubmitted();
        void OnDataReceived();
        void Send();
        
    }
}
