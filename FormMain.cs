namespace M9Studio.ShadowTalk.Server
{
    public partial class FormMain : Form
    {
        private Server server;
        private Logger logger;
        public FormMain()
        {
            InitializeComponent();
            this.Shown += post_init;
        }

        private void post_init(object sender, EventArgs e)
        {
            logger = new Logger(
                labelAddress,
                labelUser,
                labelUserOnline,
                labelMessage,
                labelMessageWaiting,
                labelMessageDeleted,
                listBoxLog);

            server = new Server(logger);

        }


    }
}
