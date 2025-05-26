using M9Studio.ShadowTalk.Core;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg;
using System.Reflection;

namespace M9Studio.ShadowTalk.Server
{
    public partial class Server
    {
        protected void Daemon(SecureSessionLogger session, User user)
        {
            int error = 10;
            while (session.IsLive && error > 0)
            {
                bool parse = false;
                JObject packet;
                try
                {
                    packet = session.ReceiveJObject();
                }
                catch (Exception ex)
                {
                    error--;
                    logger.Log($"Daemon [IPEndPoint {session.RemoteAddress}]: {ex.Message}", Logger.Type.Daemon_Error);
                    continue;
                }

                if (
                    DaemonMethod<PacketClientToServerSearchUser>(session, user, packet, SearchUser) ||
                    DaemonMethod<PacketClientToServerConnectP2P>(session, user, packet, ConnectP2P) ||
                    DaemonMethod<PacketClientToServerGetUser>(session, user, packet, GetUser) ||
                    DaemonMethod<PacketClientToServerSendMessage>(session, user, packet, SendMessage)
                    )
                {
                    //Прошло всё нормально или почти нормально
                    error = 10;
                }
                else
                {
                    logger.Log($"Daemon [IPEndPoint {session.RemoteAddress}]: Неизвестный пакет ({packet})", Logger.Type.Daemon_Error);
                }
            }
            if (error <= 0)
            {
                logger.Log($"Daemon [IPEndPoint {session.RemoteAddress}]: Не стабильные пакеты", Logger.Type.Daemon_Error);
                Disconnect(session);
            }
        }

        private bool DaemonMethod<T>(SecureSessionLogger session, User user, JObject json, DaemonMethodDelegate<T> method) where T : PacketStruct, new()
        {
            T? packet = null;
            try
            {
                packet = PacketStruct.Parse<T>(json);
            }
            catch (Exception ignore)
            {
                return false;
            }
            try
            {
                method.Invoke(session, user, packet);
            }
            catch (Exception ex)
            {
                logger.Log($"Daemon [IPEndPoint {session.RemoteAddress}]: {ex.Message}", Logger.Type.Daemon_Error);
            }
            return true;
        }

        private delegate void DaemonMethodDelegate<T>(SecureSessionLogger session, User user, T packet) where T : PacketStruct;

        private void SearchUser(SecureSessionLogger session, User user, PacketClientToServerSearchUser packet)
        {
            logger.Log($"Daemon.SearchUser [IPEndPoint {session.RemoteAddress}]: Получен запрос ({packet})", Logger.Type.Daemon_SearchUser);
            List<User> users = @base.Users("SELECT * FROM users where name LIKE LIMIT 5", $"%{packet.Name}%");
            PacketServerToClientAnswerOnSearchUser answer = new PacketServerToClientAnswerOnSearchUser()
            {
                Ids = users.Select(x => x.Id).ToArray(),
                Names = users.Select(x => x.Name).ToArray(),
                RSAs = users.Select(x =>x.RSA).ToArray(),
                Online = users.Select(x => sessions.ContainsKey(x.Id)).ToArray()
            };
            logger.Log($"Daemon.SearchUser [IPEndPoint {session.RemoteAddress}]: Отправляем пакет с ответом", Logger.Type.Daemon_SearchUser);
            for (int i = 1; i <= 10; i++)
            {
                logger.Log($"Daemon.SearchUser [IPEndPoint {session.RemoteAddress}]: Попытка {i} из 10", Logger.Type.Daemon_SearchUser);
                if (session.Send(answer))
                {
                    logger.Log($"Daemon.SearchUser [IPEndPoint {session.RemoteAddress}]: Пакет отправлен за {i} попыток", Logger.Type.Daemon_SearchUser);
                    break;
                }else if(i == 10)
                {
                    logger.Log($"Daemon.SearchUser [IPEndPoint {session.RemoteAddress}]: Пакет не удалось отправить", Logger.Type.Daemon_SearchUser);
                }
            }
        }
        private void GetUser(SecureSessionLogger session, User user, PacketClientToServerGetUser packet)
        {
            logger.Log($"Daemon.GetUser [IPEndPoint {session.RemoteAddress}]: Получен запрос ({packet})", Logger.Type.Daemon_GetUser);
            List<User> users = @base.Users("SELECT * FROM users where id = ?", packet.Id);
            PacketServerToClientAnswerOnSearchUser answer = new PacketServerToClientAnswerOnSearchUser()
            {
                Ids = users.Select(x => x.Id).ToArray(),
                Names = users.Select(x => x.Name).ToArray(),
                RSAs = users.Select(x => x.RSA).ToArray(),
                Online = users.Select(x => sessions.ContainsKey(x.Id)).ToArray()
            };
            logger.Log($"Daemon.GetUser [IPEndPoint {session.RemoteAddress}]: Отправляем пакет с ответом", Logger.Type.Daemon_GetUser);
            for (int i = 1; i <= 10; i++)
            {
                logger.Log($"Daemon.GetUser [IPEndPoint {session.RemoteAddress}]: Попытка {i} из 10", Logger.Type.Daemon_GetUser);
                if (session.Send(answer))
                {
                    logger.Log($"Daemon.GetUser [IPEndPoint {session.RemoteAddress}]: Пакет отправлен за {i} попыток", Logger.Type.Daemon_GetUser);
                    break;
                }
                else if (i == 10)
                {
                    logger.Log($"Daemon.GetUser [IPEndPoint {session.RemoteAddress}]: Пакет не удалось отправить", Logger.Type.Daemon_GetUser);
                }
            }
        }
        private void SendMessage(SecureSessionLogger session, User user, PacketClientToServerSendMessage packet)
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
        private void ConnectP2P(SecureSessionLogger session, User user, PacketClientToServerConnectP2P packet)
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
