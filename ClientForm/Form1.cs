using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class Form1 : Form
    {
        private static Socket _socket;
        private string Nickname;
        public Form1(Socket socket, string name)
        {
            Nickname = name;
            _socket = socket;
            InitializeComponent();
        }
        internal string MessageReceive()
        {

            byte[] data = new byte[2048]; // буфер для ответа
            StringBuilder builder = new StringBuilder();

            int bytes = 0; // количество полученных байт

            try
            {

                do
                {
                    bytes = _socket.Receive(data, data.Length, 0);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (_socket.Available > 0);
                return builder.ToString();
            }
            catch (SocketException)
            {

                return "";
            }
            catch (ObjectDisposedException)
            {

                return "";
            }


        }

        private void sendMessage_Click(object sender, EventArgs e)
        {

            if (messageBox.Text != "")
            {
                Byte[] bytesSend;
                string message = $"{DateTime.Now:t} {Nickname}: " + messageBox.Text + "\n";
                bytesSend = Encoding.UTF8.GetBytes(message + '\0');
                _socket.Send(BitConverter.GetBytes(bytesSend.Length), sizeof(int), 0);
                _socket.Send(bytesSend, bytesSend.Length, 0);
                bytesSend = null;
                message = null;
                if (TextBox.Text.Length > 2000000)
                {
                    TextBox.Text = $"{null,-20}{DateTime.Now:t} Произошла очистка старых сообщений!\n\n";
                }
                TextBox.Text += $"{DateTime.Now:t} Вы: " + messageBox.Text + "\n";

                messageBox.Text = null;
            }

        }
        private async void Form1_Load(object sender, EventArgs e)
        {

            TextBox.Text = $"{null,-35}Соединение установленно!\n\n";


            while (true)
            {
                string message = await Task.Run(() => MessageReceive());
                if (message == "")
                {
                    TextBox.Text += $"\n{null,-40}Ошибка соединения!";
                    sendButton.Enabled = false;
                    messageBox.Enabled = false;
                    break;
                }
                if (TextBox.Text.Length > 2000000)
                {
                    TextBox.Text = $"{null,-20}{DateTime.Now:t} Произошла очистка старых сообщений!\n\n";
                }
                TextBox.Text += message;

            }

        }

        private void messageBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
            {
                e.SuppressKeyPress = true;
                sendMessage_Click(sender, e);
            }
        }

    }
}
