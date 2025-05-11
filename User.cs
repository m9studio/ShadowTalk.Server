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
    }
}
