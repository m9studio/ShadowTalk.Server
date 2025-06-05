namespace M9Studio.ShadowTalk.Server
{
    public class Message
    {
        public int Sender;//отправитель
        public int Recipient;//получатель
        public string UUID;
        public string Text;
        public int Date = 0;
        public int Type = 0;
    }
}
