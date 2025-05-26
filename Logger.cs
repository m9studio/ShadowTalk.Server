namespace M9Studio.ShadowTalk.Server
{
    public class Logger
    {
        private List<Type> logged = new List<Type>();

        public void Log(string log, Type type)
        {
            if (logged.Contains(type))
            {

            }
        }

        public enum Type
        {
            //SecureStream
            SecureStream_Send,
            SecureStream_Receive,
            SecureStream_Error,
            SecureStream_Connect,
            SecureStream_Disconnect,

            //SecureSession
            SecureSession_Send,
            SecureSession_Receive,
            SecureSession_Error,
            SecureSession_Disconnect,
            SecureSession_Connect,

            //Login
            Login_Login,
            Login_LoginSuccess,
            //SRP
            SRP_NewSRP,
            SRP_CheckSRP,
            //Daemon
            Daemon_Error,
            Daemon_GetUser,
            Daemon_SearchUser,
            Daemon_SendMessage,
            Daemon_ConnectP2P,
        }

        public void UpdateAddress(int i)
        {

        }
        public void UpdateUser(int i)
        {

        }
        public void UpdateUserOnline(int i)
        {

        }
        public void UpdateMessage(int i)
        {

        }
        public void UpdateMessageWaiting(int i)
        {

        }
        public void UpdateMessageDeleted(int i)
        {

        }
    }
}
