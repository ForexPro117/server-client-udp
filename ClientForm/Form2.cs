using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    enum Packet
    {
        ChatSend,
        PlayersChange,
        MakeAction,
        Error
    };
    enum Actions
    {
        Rock,
        Scissors,
        Paper
    };
    public partial class Form2 : Form
    {
        private static Socket _socket;
        private string Nickname;
        int wins = 0;
        int lose = 0;
        int draw = 0;

        public Form2(Socket socket, string name)
        {
            _socket = socket;
            Nickname = name;
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

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void Form2_Load(object sender, EventArgs e)
        {
            label4.Text += Nickname;
            _socket.Send(Encoding.ASCII.GetBytes("4321\0"), 5, 0);
            byte[] buffer = new byte[sizeof(Packet)];
            Packet packet = Packet.Error;

            while (true)
            {



                await Task.Run(() =>
                {
                    try
                    {
                        _socket.Receive(buffer, sizeof(Packet), 0);
                        packet = (Packet)BitConverter.ToInt32(buffer, 0);
                    }
                    catch (Exception)
                    {
                        packet = Packet.Error;
                    }
                });

                switch (packet)
                {
                    case Packet.ChatSend:

                        break;
                    case Packet.PlayersChange:
                        try
                        {
                            _socket.Receive(buffer, sizeof(int), 0);
                            label3.Text = $"Игроков онлайн:{BitConverter.ToInt32(buffer, 0)}";
                        }
                        catch (ObjectDisposedException)
                        {
                            richTextBox1.Text = "Ошибка соединения";
                            RockBtn.Visible = false;
                            ScissorsBtn.Visible = false;
                            PaperBtn.Visible = false;

                            return;
                        }
                        break;

                    case Packet.Error:
                        richTextBox1.Text = "Ошибка соединения";
                        RockBtn.Visible = false;
                        ScissorsBtn.Visible = false;
                        PaperBtn.Visible = false;
                        return;
                }



            }
        }

        private void Image_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var name = btn.Name;
            _socket.Send(BitConverter.GetBytes((int)Packet.MakeAction), sizeof(Packet), 0);
            switch (name)
            {
                case "RockBtn":
                    _socket.Send(BitConverter.GetBytes((int)Actions.Rock), sizeof(Actions), 0);
                    richTextBox1.Text = "Вы выбрали камень." +
                        "\nОжидайте ход остальных игороков!";
                    break;
                case "ScissorsBtn":
                    _socket.Send(BitConverter.GetBytes((int)Actions.Scissors), sizeof(Actions), 0);
                    richTextBox1.Text = "Вы выбрали ножницы." +
                        "\nОжидайте ход остальных игороков!";
                    break;
                case "PaperBtn":
                    _socket.Send(BitConverter.GetBytes((int)Actions.Paper), sizeof(Actions), 0);
                    richTextBox1.Text = "Вы выбрали бумагу." +
                        "\nОжидайте ход остальных игороков!";
                    break;
            }

        }
    }
}
