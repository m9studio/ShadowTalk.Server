using System.Windows.Forms;

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

        private void button1_Click(object sender, EventArgs e)
        {
            listBoxLog.Items.Clear();
        }




        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxLog.SelectedItem != null)
            {
                string fullText = listBoxLog.SelectedItem.ToString();

                // —оздаем и показываем новое окно
                Form detailForm = new Form();
                detailForm.Text = "ѕолный текст";

                TextBox textBox = new TextBox();
                textBox.Multiline = true;
                textBox.ReadOnly = true;
                textBox.Text = fullText;
                textBox.Dock = DockStyle.Fill;
                textBox.ScrollBars = ScrollBars.Vertical;

                detailForm.Controls.Add(textBox);
                detailForm.Size = new Size(400, 300);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.Show(); // или Show(), если не нужно блокировать основное окно
            }
        }

    }
}
