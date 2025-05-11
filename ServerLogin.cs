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
            List<User> users = @base.Users("SELECT * FROM user WHERE email = ? AND password = ?", login.Email, login.Password);
            if(users.Count == 0)
            {
                session.Send(new PacketServerToClientLoginError());
                Disconect(session);
                return;
            }
            User user = users[0];
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
                            Disconect(session);
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
    }
}
