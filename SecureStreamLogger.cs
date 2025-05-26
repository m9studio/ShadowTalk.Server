using M9Studio.SecureStream;
using M9Studio.ShadowTalk.Core;
using System.Net;

namespace M9Studio.ShadowTalk.Server
{
    public class SecureStreamLogger : ISecureTransportAdapter<IPEndPoint>
    {
        public event Action<IPEndPoint> OnConnected;
        public event Action<IPEndPoint> OnDisconnected;

        private TcpServerSecureTransportAdapter adapter;
        private Logger logger;

        public SecureStreamLogger(TcpServerSecureTransportAdapter adapter, Logger logger)
        {
            this.adapter = adapter;
            this.logger = logger;

            adapter.OnConnected += (address) => {
                logger.Log($"SecureStream [{address}]: Подключился", Logger.Type.SecureStream_Connect);
                OnConnected?.Invoke(address);
            };
            adapter.OnDisconnected += (address) => {
                logger.Log($"SecureStream [{address}]: Отключился", Logger.Type.SecureStream_Disconnect);
                OnDisconnected?.Invoke(address);
            };
        }

        public byte[] ReceiveFrom(IPEndPoint address)
        {
            byte[] buffer;
            logger.Log($"SecureStream.ReceiveFrom(IPEndPoint {address}): Ожидаем зашифрованный пакет", Logger.Type.SecureStream_Receive);
            try
            {
                buffer = adapter.ReceiveFrom(address);
            }
            catch (Exception ex)
            {
                logger.Log($"SecureStream.ReceiveFrom(IPEndPoint {address}): Ошибка при получении: {ex.Message}", Logger.Type.SecureStream_Error);
                throw new Exception(ex.Message);
            }
            logger.Log($"SecureStream.ReceiveFrom(IPEndPoint {address}): Получен зашифрованный пакет ({buffer})", Logger.Type.SecureStream_Receive);
            return buffer;
        }

        public bool SendTo(byte[] buffer, IPEndPoint address)
        {
            bool result;
            logger.Log($"SecureStream.SendTo(byte[] {buffer.GetHashCode()}, IPEndPoint {address}): Пытаемся отправить зашифрованный пакет ({buffer})", Logger.Type.SecureStream_Send);
            try
            {
                result = adapter.SendTo(buffer, address);
            }
            catch (Exception ex)
            {
                logger.Log($"SecureStream.SendTo(byte[] {buffer.GetHashCode()}, IPEndPoint {address}): Ошибка при отправке: {ex.Message}", Logger.Type.SecureStream_Error);
                throw new Exception(ex.Message);
            }
            logger.Log($"SecureStream.SendTo(byte[] {buffer.GetHashCode()}, IPEndPoint {address}): Зашифрованный пакет {(result ? "" : "не")} отправлен", Logger.Type.SecureStream_Send);
            return result;
        }
    }
}
