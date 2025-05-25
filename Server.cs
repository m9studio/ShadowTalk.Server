using System.Net;
using System.Net.Sockets;
using M9Studio.ShadowTalk.Core;
using M9Studio.SecureStream;
using Newtonsoft.Json.Linq;

namespace M9Studio.ShadowTalk.Server
{
    public partial class Server
    {
        public static string GenerateHexString(int byteLength)
        {
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] buffer = new byte[byteLength];
            rng.GetBytes(buffer);
            return BitConverter.ToString(buffer).Replace("-", "").ToLower();
        }


        protected DataBase @base;
        Socket socket;
        TcpServerSecureTransportAdapter adapter;
        SecureChannelManager<IPEndPoint> manager;

        protected Dictionary<SecureSession<IPEndPoint>, int> addresses;
        protected Dictionary<int, SecureSession<IPEndPoint>> sessions;
        protected Dictionary<int, User> users;//TODO нужно ли?


        public Server()
        {
            @base = new DataBase();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 55555));
            socket.Listen(10);

            adapter = new TcpServerSecureTransportAdapter(socket);
            manager = new SecureChannelManager<IPEndPoint>(adapter);
            addresses = new Dictionary<SecureSession<IPEndPoint>, int>();
            sessions = new Dictionary<int, SecureSession<IPEndPoint>>();
            users = new Dictionary<int, User>();


            manager.OnConnected += Connect;
            manager.OnDisconnected += Disconnect;
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
            Disconnect(session);
        }
        protected void Disconnect(SecureSession<IPEndPoint> session)
        {
            if (addresses.ContainsKey(session))
            {
                int id = addresses[session];
                users.Remove(id);
                sessions.Remove(id);
                addresses.Remove(session);
            }
            adapter.Disconect(session.RemoteAddress);
        }
    }
}
