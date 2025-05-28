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
                    logger.Log($"Daemon [IPEndPoint {session.RemoteAddress}]: {ex.Message}", Logger.Type.Daemon);
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
                    logger.Log($"Daemon [IPEndPoint {session.RemoteAddress}]: Неизвестный пакет ({packet})", Logger.Type.Daemon);
                }
            }
            if (error <= 0)
            {
                logger.Log($"Daemon [IPEndPoint {session.RemoteAddress}]: Нестабильные пакеты", Logger.Type.Daemon);
                Disconnect(session);
            }
            else
            {
                logger.Log($"Daemon [IPEndPoint {session.RemoteAddress}]: Отключился", Logger.Type.Daemon);
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

            Send(session, answer, Logger.Type.Daemon_SearchUser, "SearchUser");
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

            Send(session, answer, Logger.Type.Daemon_GetUser, "GetUser");
        }
        private void SendMessage(SecureSessionLogger session, User user, PacketClientToServerSendMessage packet)
        {
            logger.Log($"Daemon.SendMessage [IPEndPoint {session.RemoteAddress}]: Получен запрос ({packet})", Logger.Type.Daemon_SendMessage);
            if (sessions.ContainsKey(packet.Id))
            {
                logger.Log($"Daemon.SendMessage [IPEndPoint {session.RemoteAddress}]: Получатель в сети, проксируем сообщение через сервер", Logger.Type.Daemon_SendMessage);
                PacketServerToClientSendMessages packet2 = new PacketServerToClientSendMessages()
                {
                    Texts = new string[] { packet.Text },
                    UUIDs = new string[] { packet.UUID },
                    Users = new int[] { user.Id }
                };

                Send(session, sessions[packet.Id], packet2, Logger.Type.Daemon_SendMessage, "SendMessage");

                PacketServerToClientStatusMessages packet3 = new PacketServerToClientStatusMessages()
                {
                    Checks = new int[] { (int)PacketServerToClientStatusMessages.CheckType.VIEWED },
                    UUIDs = new string[] { packet.UUID }
                };

                Send(session, packet3, Logger.Type.Daemon_SendMessage, "SendMessage");
            }
            else
            {
                logger.Log($"Daemon.SendMessage [IPEndPoint {session.RemoteAddress}]: Сохраняем сообщение", Logger.Type.Daemon_SendMessage);
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



        private bool Send(SecureSessionLogger sessionSender, SecureSessionLogger sessionRecipient, PacketStruct packet, Logger.Type logType, string method)
        {
            logger.Log($"Daemon.{method} [IPEndPoint {sessionSender.RemoteAddress} -> {sessionRecipient.RemoteAddress}]: Отправляем пакет с ответом", logType);
            for (int i = 1; i <= 10; i++)
            {
                logger.Log($"Daemon.{method} [IPEndPoint {sessionSender.RemoteAddress} -> {sessionRecipient.RemoteAddress}]: Попытка {i} из 10", logType);
                if (sessionRecipient.Send(packet))
                {
                    logger.Log($"Daemon.{method} [IPEndPoint {sessionSender.RemoteAddress} -> {sessionRecipient.RemoteAddress}]: Пакет отправлен за {i} попыток", logType);
                    return true;
                }
            }
            logger.Log($"Daemon.{method} [IPEndPoint {sessionSender.RemoteAddress} -> {sessionRecipient.RemoteAddress}]: Пакет не удалось отправить", logType);
            return false;
        }

        private bool Send(SecureSessionLogger session, PacketStruct packet, Logger.Type logType, string method)
        {
            logger.Log($"Daemon.{method} [IPEndPoint {session.RemoteAddress}]: Отправляем пакет с ответом", logType);
            for (int i = 1; i <= 10; i++)
            {
                logger.Log($"Daemon.{method} [IPEndPoint {session.RemoteAddress}]: Попытка {i} из 10", logType);
                if (session.Send(packet))
                {
                    logger.Log($"Daemon.{method} [IPEndPoint {session.RemoteAddress}]: Пакет отправлен за {i} попыток", logType);
                    return true;
                }
            }
            logger.Log($"Daemon.{method} [IPEndPoint {session.RemoteAddress}]: Пакет не удалось отправить", logType);
            return false;
        }
    }
}
