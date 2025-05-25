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
                try
                {
                    SearchUser(session, PacketStruct.Parse<PacketClientToServerSearchUser>(packet));
                    continue;
                } catch (Exception ex) { }
                try
                {
                    SendMessage(session, PacketStruct.Parse<PacketClientToServerSendMessage>(packet));
                    continue;
                }
                catch (Exception ex) { }
                try
                {
                    GetUser(session, PacketStruct.Parse<PacketClientToServerGetUser>(packet));
                    continue;
                }
                catch (Exception ex) { }
                try
                {
                    ConnectP2P(session, PacketStruct.Parse<PacketClientToServerConnectP2P>(packet));
                    continue;
                }
                catch (Exception ex) { }
                //Console.WriteLine("Error");
            }
        }
        private void SearchUser(SecureSession<IPEndPoint> session, PacketClientToServerSearchUser packet)
        {
            List<User> users = @base.Users("SELECT * FROM users where name LIKE LIMIT 5", $"%{packet.Name}%");
            PacketServerToClientAnswerOnSearchUser answer = new PacketServerToClientAnswerOnSearchUser()
            {
                Ids = users.Select(x => x.Id).ToArray(),
                Names = users.Select(x => x.Name).ToArray(),
                RSAs = users.Select(x =>x.RSA).ToArray()
            };
            session.Send(answer);
        }
        private void GetUser(SecureSession<IPEndPoint> session, PacketClientToServerGetUser packet)
        {
            List<User> users = @base.Users("SELECT * FROM users where id = ?", packet.Id);
            PacketServerToClientAnswerOnSearchUser answer = new PacketServerToClientAnswerOnSearchUser()
            {
                Ids = users.Select(x => x.Id).ToArray(),
                Names = users.Select(x => x.Name).ToArray(),
                RSAs = users.Select(x => x.RSA).ToArray()
            };
            session.Send(answer);
        }
        private void SendMessage(SecureSession<IPEndPoint> session, PacketClientToServerSendMessage packet)
        {

        }
        private void ConnectP2P(SecureSession<IPEndPoint> session, PacketClientToServerConnectP2P packet)
        {

        }
    }
}
