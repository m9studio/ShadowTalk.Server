using M9Studio.SecureStream;
using M9Studio.ShadowTalk.Core;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace M9Studio.ShadowTalk.Server
{
    public class SecureSessionLogger
    {
        private SecureSession<IPEndPoint> session;
        private Logger logger;

        public IPEndPoint RemoteAddress => session.RemoteAddress;
        public bool IsLive => session.IsLive;

        public SecureSessionLogger(SecureSession<IPEndPoint> session, Logger logger)
        {
            this.session = session;
            this.logger = logger;
        }

        public byte[] Receive()
        {
            byte[] buffer;
            logger.Log($"SecureSession.Receive() [IPEndPoint {RemoteAddress}]: Ожидаем пакет", Logger.Type.SecureSession_Receive);
            try
            {
                buffer = session.Receive();
            }
            catch (Exception ex)
            {
                logger.Log($"SecureSession.Receive() [IPEndPoint {RemoteAddress}]: Ошибка при получении: {ex.Message}", Logger.Type.SecureSession_Receive);
                throw new Exception(ex.Message);
            }
            logger.Log($"SecureSession.Receive() [IPEndPoint {RemoteAddress}]: Получен пакет ({buffer})", Logger.Type.SecureSession_Receive);
            return buffer;
        }
        public string ReceiveString()
        {
            logger.Log($"SecureSession.ReceiveString() [IPEndPoint {RemoteAddress}]: Ожидаем пакет", Logger.Type.SecureSession_Receive);
            string result =  Encoding.UTF8.GetString(Receive());
            logger.Log($"SecureSession.ReceiveString() [IPEndPoint {RemoteAddress}]: Получен пакет ({result})", Logger.Type.SecureSession_Receive);
            return session.ReceiveString();
        }
        public JObject ReceiveJObject()
        {
            logger.Log($"SecureSession.ReceiveJObject() [IPEndPoint {RemoteAddress}]: Ожидаем пакет", Logger.Type.SecureSession_Receive);
            JObject result = JObject.Parse(ReceiveString());
            logger.Log($"SecureSession.ReceiveJObject() [IPEndPoint {RemoteAddress}]: Получен пакет ({result})", Logger.Type.SecureSession_Receive);
            return result;
        }

        public bool Send(byte[] buffer)
        {
            bool result;
            logger.Log($"SecureSession.Send(byte[] {buffer.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Пытаемся отправить пакет ({buffer})", Logger.Type.SecureStream_Send);
            try
            {
                result = session.Send(buffer);
            }
            catch (Exception ex)
            {
                logger.Log($"SecureSession.Send(byte[] {buffer.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Ошибка при отправке: {ex.Message}", Logger.Type.SecureStream_Send);
                throw new Exception(ex.Message);
            }
            logger.Log($"SecureSession.Send(byte[] {buffer.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Пакет {(result ? "" : "не")} отправлен", Logger.Type.SecureStream_Send);
            return result;
        }
        public bool Send(string data)
        {
            logger.Log($"SecureSession.Send(string {data.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Пытаемся отправить пакет ({data})", Logger.Type.SecureStream_Send);
            bool result = Send(Encoding.UTF8.GetBytes(data));
            logger.Log($"SecureSession.Send(string {data.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Пакет {(result ? "" : "не")} отправлен", Logger.Type.SecureStream_Send);
            return result;
        }
        public bool Send(JObject data)
        {
            logger.Log($"SecureSession.Send(JObject {data.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Пытаемся отправить пакет ({data})", Logger.Type.SecureStream_Send);
            bool result = Send(data.ToString());
            logger.Log($"SecureSession.Send(JObject {data.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Пакет {(result ? "" : "не")} отправлен", Logger.Type.SecureStream_Send);
            return result;
        }
        public bool Send(PacketStruct data)
        {
            logger.Log($"SecureSession.Send(PacketStruct {data.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Пытаемся отправить пакет ({data})", Logger.Type.SecureStream_Send);
            bool result = Send(data.ToJObject());
            logger.Log($"SecureSession.Send(PacketStruct {data.GetHashCode()}) [IPEndPoint {RemoteAddress}]: Пакет {(result ? "" : "не")} отправлен", Logger.Type.SecureStream_Send);
            return result;
        }


        public override int GetHashCode()
        {
            return RemoteAddress.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is SecureSessionLogger)
            {
                return RemoteAddress == ((SecureSessionLogger)obj).RemoteAddress;
            }
            return false;
        }
    }
}
