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

                // Создаем и показываем новое окно
                Form detailForm = new Form();
                detailForm.Text = "Полный текст";

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

        private void button2_Click(object sender, EventArgs e)
        {
            InputForm inputForm = new InputForm();
            inputForm.OnSave = Save;
            inputForm.ShowDialog();
        }
        private void Save(string nick, string login, string password)
        {
            int id = server.@base.Count("SELECT IFNULL(MAX(id), 0) AS num FROM users") + 1;
            string RSA = "<RSAKeyValue><Modulus>t6hxT8GPOvYBdUmtt6M8zVguGu9XTIjG+EO7nWlMNQLFDgU6Aksa8/YTM1puetcr8xsX0EO9ZxJqrlb19JogyEO1W3HmPYd/mI6VfXEYPlAB32XGD3lTb3DjZ+DEJ6HyIT0YsKG9YUztebuUGk6ZvheXOp0fByn1oQbRrIGjJZLh9ZfXeqii0yHiKWyyxoUPqkEvGwNABXgKjvHtYODiyVxOfATFBS+1m6uRPgm5/cDSEBJ72GvMHAwE1bx+UeNDmOvXBP0Kn/xqQwFUQHwcjBdKYw+iwQI0oVkVLJkJn7PU7vEbL7Kp9ed1Rky4OR5zJVIaUT998Rjf8+8iDuUHHQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            server.@base.Send("INSERT INTO users (id, name, email, password, rsa, is2fa) VALUES (?, ?, ?, ?, ?, 0)", id, nick, login, password, RSA);
            server.Update();
        }
    }





    public partial class InputForm : Form
    {
        private TextBox textNick;
        private TextBox textLogin;
        private TextBox textPassword;
        private Button btnSave;

        public Action<string, string, string> OnSave; // делегат для передачи данных

        public InputForm()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Введите данные";
            this.Size = new System.Drawing.Size(300, 230);

            var labelNick = new Label() { Text = "Ник:", Left = 10, Top = 20 };
            textNick = new TextBox() { Left = 120, Top = 20, Width = 150 };

            var labelLogin = new Label() { Text = "Логин:", Left = 10, Top = 60 };
            textLogin = new TextBox() { Left = 120, Top = 60, Width = 150 };
            textLogin.Text = "@com.com";

            var labelPassword = new Label() { Text = "Пароль:", Left = 10, Top = 100 };
            textPassword = new TextBox() { Left = 120, Top = 100, Width = 150, PasswordChar = '*' };
            textPassword.Text = "00000000";

            btnSave = new Button() { Text = "Сохранить", Left = 100, Top = 140, Width = 100 };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] {
            labelNick, textNick, labelLogin, textLogin,
            labelPassword, textPassword, btnSave
        });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string nick = textNick.Text;
            string login = textLogin.Text;
            string password = textPassword.Text;

            OnSave?.Invoke(nick, login, password); // вызываем колбэк
            this.Close(); // закрываем окно
        }
    }
}
