using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class Form2 : Form
    {
        private static Socket _socket;
        private string Nickname;
        public Form2(Socket socket, string name)
        {
            _socket = socket;
            Nickname = name;
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e) { }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void Form2_Load(object sender, EventArgs e)
        {
            label4.Text += Nickname;
            byte[] connMessage;
            string playersLeft; // количество полученных байт
            connMessage = Encoding.UTF8.GetBytes("Connected" + '\0');
            _socket.Send(BitConverter.GetBytes(connMessage.Length), sizeof(int), 0);


            //Принятие сообщения о количестве игроков от сервера
            while (true)
            {            
                playersLeft = await Task.Run(() => MessageReceive());
                label3.Text = $"Осталось игроков: " + playersLeft;
            }
        }

        internal string MessageReceive()
        {
            byte[] data = new byte[sizeof(int)]; // буфер для ответа
            int bytes = 0; // количество полученных байт
            try
            {
                bytes = _socket.Receive(data, sizeof(int), 0);
                return BitConverter.ToInt32(data, 0).ToString();
            }
            catch (SocketException)
            {
                this.Close();
                return "";
            }
            catch (ObjectDisposedException)
            {
                return "";
            }
        }
    }
}
