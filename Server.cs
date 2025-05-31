using System.Net;
using System.Net.Sockets;
using M9Studio.ShadowTalk.Core;
using M9Studio.SecureStream;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;

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
        protected Logger logger;

        public readonly IPEndPoint Address;

        private Socket socket;
        private TcpServerSecureTransportAdapter adapter;
        private SecureStreamLogger adapterLogger;
        private SecureChannelManager<IPEndPoint> manager;

        protected Dictionary<SecureSessionLogger, int> addresses;
        protected Dictionary<int, SecureSessionLogger> sessions;
        protected Dictionary<int, User> users;



        public Server(Logger logger)
        {
            @base = new DataBase();
            this.logger = logger;


            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Address = new IPEndPoint(IPAddress.Any, 55555);

            socket.Bind(Address);
            socket.Listen(10);

            adapter = new TcpServerSecureTransportAdapter(socket);
            //Для логов
            adapterLogger = new SecureStreamLogger(adapter, logger);
            manager = new SecureChannelManager<IPEndPoint>(adapterLogger);
            //manager = new SecureChannelManager<IPEndPoint>(adapter);
            addresses = new Dictionary<SecureSessionLogger, int>();
            sessions = new Dictionary<int, SecureSessionLogger>();
            users = new Dictionary<int, User>();


            logger.UpdateAddress(allIP(55555));
            logger.UpdateUser(@base.Count("SELECT COUNT(*) AS num FROM users"));
            logger.UpdateUserOnline(0);
            logger.UpdateMessage(@base.Count("SELECT COUNT(*) AS num FROM messages"));
            logger.UpdateMessageWaiting(@base.Count("SELECT COUNT(*) AS num FROM messages where type = ?", (int)PacketServerToClientStatusMessages.CheckType.AWAITING));
            logger.UpdateMessageDeleted(@base.Count("SELECT COUNT(*) AS num FROM messages where type = ?", (int)PacketServerToClientStatusMessages.CheckType.DELETED));


            manager.OnConnected += session => Connect(new SecureSessionLogger(session, logger));
            manager.OnDisconnected += session => Disconnect(new SecureSessionLogger(session, logger));
        }


        static string allIP(int port)
        {
            string hostName = Dns.GetHostName();
            var ipAddresses = Dns.GetHostEntry(hostName).AddressList;

            string ret = "";
            ret += "0.0.0.0:" + port + "\n";
            ret += "127.0.0.1:" + port + "\n";
            foreach (var ip in ipAddresses)
            {
                ret += ip.ToString() + ":" + port + "\n";
            }
            return ret;
        }



        private void Connect(SecureSessionLogger session)
        {
            Task.Run(() =>
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
            });
        }
        protected void Disconnect(SecureSessionLogger session)
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
