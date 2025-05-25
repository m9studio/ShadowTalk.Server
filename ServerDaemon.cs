using M9Studio.SecureStream;
using M9Studio.ShadowTalk.Core;
using Newtonsoft.Json.Linq;
using System.Net;

namespace M9Studio.ShadowTalk.Server
{
    public partial class Server
    {
        protected void Daemon(SecureSession<IPEndPoint> session, User user)
        {
            while (true)
            {
                JObject packet = session.ReceiveJObject();
                //todo обработка

            }
        }
        private void SearchUser(SecureSession<IPEndPoint> session, PacketClientToServerSearchUser packet)
        {

        }
        private void SendMessage(SecureSession<IPEndPoint> session, PacketClientToServerSendMessage packet)
        {

        }
        private void GetUser(SecureSession<IPEndPoint> session, PacketClientToServerGetUser packet)
        {

        }
        private void ConnectP2P(SecureSession<IPEndPoint> session, PacketClientToServerConnectP2P packet)
        {

        }
    }
}
