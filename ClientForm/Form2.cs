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

        private void label3_Click(object sender, EventArgs e){}

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            label4.Text += Nickname;
            byte[] data = new byte[2048]; // буфер для ответа
            byte[] connMessage;
            int playersLeft = 0; // количество полученных байт
            connMessage = Encoding.UTF8.GetBytes("Connected" + '\0');
            _socket.Send(BitConverter.GetBytes(connMessage.Length), sizeof(int), 0);

            //Принятие сообщения о количестве игроков от сервера
            while (true)
            {               
                playersLeft = _socket.Receive(data, data.Length, 0);
                label3.Text = $"Осталось игроков: {playersLeft}";
            }
        }
    }
}
