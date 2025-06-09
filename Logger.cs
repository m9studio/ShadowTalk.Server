using System.Net;

namespace M9Studio.ShadowTalk.Server
{
    public class Logger
    {
        private List<Type> logged = new List<Type>();

        private Label address;
        private Label user;
        private Label userOnline;
        private Label message;
        private Label messageWaiting;
        private Label messageDeleted;
        private ListBox list;



        public Logger(Label address,
                      Label user,
                      Label userOnline,
                      Label message,
                      Label messageWaiting,
                      Label messageDeleted,
                      ListBox list)
        {
            this.address = address;
            this.user = user;
            this.userOnline = userOnline;
            this.message = message;
            this.messageWaiting = messageWaiting;
            this.messageDeleted = messageDeleted;
            this.list = list;
        }


        public void Log(string log, Type type)
        {
            //if (logged.Contains(type))
            {
                list.Invoke(() => {
                    list.Items.Add(log);
                });
            }
        }

        public enum Type
        {
            //SecureStream
            SecureStream_Send,
            SecureStream_Receive,
            SecureStream_Connect,
            SecureStream_Disconnect,

            //SecureSession
            SecureSession_Send,
            SecureSession_Receive,
            SecureSession_Disconnect,
            SecureSession_Connect,

            //Login
            Login_Login,
            Login_LoginSuccess,
            //SRP
            SRP_NewSRP,
            SRP_CheckSRP,
            //Daemon
            Daemon,
            Daemon_GetUser,
            Daemon_SearchUser,
            Daemon_SendMessage,
            Daemon_ConnectP2P,
        }

        public void UpdateAddress(string a)
        {
            address.Invoke(() => { 
                address.Text = "Адресса:\n" + a;
            });
        }
        public void UpdateUser(int i)
        {
            user.Invoke(() => {
                user.Text = "Пользователей: " + i;
            });
        }
        public void UpdateUserOnline(int i)
        {
            userOnline.Invoke(() => {
                userOnline.Text = "Пользователей онлайн: " + i;
            });
        }
        public void UpdateMessage(int i)
        {
            message.Invoke(() => {
                message.Text = "Сообщений: " + i;
            });
        }
        public void UpdateMessageWaiting(int i)
        {
            messageWaiting.Invoke(() => {
                messageWaiting.Text = "Сообщений ожидающих получения: " + i;
            });
        }
        public void UpdateMessageDeleted(int i)
        {
            messageDeleted.Invoke(() => {
                messageDeleted.Text = "Удаленных сообщений: " + i;
            });
        }
    }
}
