using System;
using System.Collections.Generic;
using System.Text;

namespace MinerProxy2.Interfaces
{
    interface ICoinHandlerClient
    {
        void OnDisconnect();
        void OnConnect();
        void OnError();
        void OnShareSubmitted();
        void OnClientData();
        void Send();
        
    }
}
