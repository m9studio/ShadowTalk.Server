namespace M9Studio.ShadowTalk.Server
{
    public class User
    {
        public string Name;
        public int Id;
        public string Email;
        public string Password;
        public string Salt;
        public string Verifier;
        public bool Is2FA;
        public string RSA;//публичный ключ для сообщений
        public string B;
        public string b;

        public int Port;//udp порт для p2p, не сохранять в бд
    }
}
