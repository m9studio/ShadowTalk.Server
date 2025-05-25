using M9Studio.SecureStream;
using M9Studio.ShadowTalk.Core;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
                    SendMessage(session, user, PacketStruct.Parse<PacketClientToServerSendMessage>(packet));
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
                    ConnectP2P(session, user, PacketStruct.Parse<PacketClientToServerConnectP2P>(packet));
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
                RSAs = users.Select(x =>x.RSA).ToArray(),
                Online = users.Select(x => sessions.ContainsKey(x.Id)).ToArray()
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
                RSAs = users.Select(x => x.RSA).ToArray(),
                Online = users.Select(x => sessions.ContainsKey(x.Id)).ToArray()
            };
            session.Send(answer);
        }
        private void SendMessage(SecureSession<IPEndPoint> session, User user, PacketClientToServerSendMessage packet)
        {
            if (sessions.ContainsKey(packet.Id))
            {
                PacketServerToClientSendMessages packet2 = new PacketServerToClientSendMessages()
                {
                    Texts = new string[] { packet.Text},
                    UUIDs = new string[] { packet.UUID },
                    Users = new int[] { user.Id }
                };
                sessions[packet.Id].Send(packet2);
                PacketServerToClientStatusMessages packet3 = new PacketServerToClientStatusMessages()
                {
                    Checks = new int[] { (int)PacketServerToClientStatusMessages.CheckType.VIEWED },
                    UUIDs = new string[] { packet.UUID }
                };
                session.Send(packet3);
            }
            else
            {
                @base.Send("INSERT INTO messages (sender, recipient, uuid, text) VALUES (?, ?, ?, ?)", user.Id, packet.Id, packet.UUID, packet.Text);
            }
        }
        private void ConnectP2P(SecureSession<IPEndPoint> session, User user, PacketClientToServerConnectP2P packet)
        {
            if (!sessions.ContainsKey(packet.UserId)){
                session.Send(new PacketServerToClientErrorP2P());
            }
            else
            {
                string key = GenerateHexString(32);
                
                PacketServerToClientAnswerOnConnectP2P packet2 = new PacketServerToClientAnswerOnConnectP2P()
                {
                    Key = key,
                    Port = users[packet.UserId].Port,
                    Ip = sessions[packet.UserId].RemoteAddress.Address.ToString()
                };
                session.Send(packet2);


                PacketServerToClientRequestOnConnectP2P packet3 = new PacketServerToClientRequestOnConnectP2P()
                {
                    UserId = user.Id,
                    UserName = user.Name,

                    Key = key,
                    Port = user.Port,
                    Ip = session.RemoteAddress.Address.ToString()
                };
                sessions[packet.UserId].Send(packet3);
            }
        }
    }
}
