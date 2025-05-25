using M9Studio.SecureStream;
using M9Studio.ShadowTalk.Core;
using System.Net;

namespace M9Studio.ShadowTalk.Server
{
    public partial class Server
    {
        private Random random = new Random();
        protected void Login(SecureSession<IPEndPoint> session, PacketClientToServerLogin login)
        {
            List<User> users = @base.Users("SELECT * FROM users WHERE email = ? AND password = ?", login.Email, login.Password);
            if(users.Count == 0)
            {
                session.Send(new PacketServerToClientLoginError());
                Disconnect(session);
                return;
            }
            User user = users[0];

            if (sessions.ContainsKey(user.Id))
            {
                session.Send(new PacketServerToClientLoginError());
                Disconnect(session);
                return;
            }


            if (user.Is2FA)
            {
                PacketClientToServer2FA _2fa;
                session.Send(new PacketServerToClientRequestOn2FA());
                string code = random.Next(0, 1000000).ToString("D6");
                //TODO send email code
                for (int i = 1; i <= 3; i++)
                {
                    try
                    {
                        _2fa = PacketStruct.Parse<PacketClientToServer2FA>(session.ReceiveJObject());
                    }
                    catch (Exception ex)
                    {
                        //TODO
                        //log
                        continue;
                    }
                    if (_2fa.Code != code)
                    {
                        if (i < 3)
                        {
                            session.Send(new PacketServerToClient2FAError());
                        }
                        else
                        {
                            session.Send(new PacketServerToClientLoginError());
                            Disconnect(session);
                            return;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            NewSRP(session, user);
        }

        protected void LoginSuccess(SecureSession<IPEndPoint> session, User user)
        {
            sessions[user.Id] = session;
            addresses[session] = user.Id;
            users[user.Id] = user;



            List<Message> me = @base.Messages("SELECT * FROM messages WHERE recipient = ? AND type = ?", user.Id, (int)PacketServerToClientStatusMessages.CheckType.AWAITING);
            @base.Send("UPDATE messages SET type = ? WHERE sender = ? AND type = ?", (int)PacketServerToClientStatusMessages.CheckType.VIEWED, user.Id, (int)PacketServerToClientStatusMessages.CheckType.AWAITING);

            List<Message> status = @base.Messages("SELECT * FROM messages WHERE sender = ?", user.Id);
            @base.Send("DELETE FROM messages WHERE sender = ? AND type != ?", user.Id, (int)PacketServerToClientStatusMessages.CheckType.AWAITING);

            PacketServerToClientSendMessages packetMe = new PacketServerToClientSendMessages()
            {
                Users = me.Select(x => x.Sender).ToArray(),
                UUIDs = me.Select(x => x.UUID).ToArray(),
                Texts = me.Select(x => x.Text).ToArray()
            };
            PacketServerToClientStatusMessages packetStatus = new PacketServerToClientStatusMessages()
            {
                Checks = status.Select(x => x.Type).ToArray(),
                UUIDs = status.Select(x => x.UUID).ToArray()
            };

            session.Send(packetMe);
            session.Send(packetStatus);

            Daemon(session, user);
        }
    }
}
