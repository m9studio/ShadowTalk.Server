using System.Net;
using System.Net.Sockets;
using M9Studio.ShadowTalk.Core;
using M9Studio.SecureStream;
using Newtonsoft.Json.Linq;

namespace M9Studio.ShadowTalk.Server
{
    public partial class Server
    {
        protected DataBase @base;
        Socket socket;
        TcpServerSecureTransportAdapter adapter;
        SecureChannelManager<IPEndPoint> manager;

        private Dictionary<int, SecureSession<IPEndPoint>> sessions;
        private Dictionary<int, User> users;//TODO нужно ли?


        public Server()
        {
            @base = new DataBase();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 55555));
            socket.Listen(10);

            adapter = new TcpServerSecureTransportAdapter(socket);
            manager = new SecureChannelManager<IPEndPoint>(adapter);
            sessions = new Dictionary<int, SecureSession<IPEndPoint>>();
            users = new Dictionary<int, User>();

            manager.OnSecureSessionEstablished += Connect;
        }
        private void Connect(SecureSession<IPEndPoint> session)
        {
            JObject packet = session.ReceiveJObject();

            try
            {
                CheckSRP(session, PacketStruct.Parse<PacketClientToServerReconectSRP>(packet));
                return;
            }
            catch (Exception ex) { }

            try
            {
                Login(session, PacketStruct.Parse<PacketClientToServerLogin>(packet));
                return;
            }
            catch (Exception ex) { }
            Disconect(session);
        }
        protected void LoginSuccess(SecureSession<IPEndPoint> session)
        {
            session.Send(new PacketServerToClientSendMessages());
            session.Send(new PacketServerToClientStatusMessages());
        }
        protected void Disconect(SecureSession<IPEndPoint> session) => adapter.Disconect(session.RemoteAddress);
    }
}
