using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    enum Packet
    {
        P_UserListChange,
        P_UserMakeLeader,
        P_UserListGet,
        P_ChatSend,
        P_error
    };
    public partial class Form1 : Form
    {
        private static Socket _socket;
        private string Nickname;
        private Dictionary<int, string> players;
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
                _socket.Send(BitConverter.GetBytes((int)Packet.P_ChatSend), sizeof(Packet), 0);
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

            _socket.Send(BitConverter.GetBytes(Nickname.Length ), sizeof(int), 0);
            _socket.Send(Encoding.UTF8.GetBytes(Nickname ), Nickname.Length, 0);
            Packet packet = Packet.P_error;
            byte[] buffer = new byte[sizeof(Packet)];
            TextBox.Text = $"{null,-35}Соединение установленно!\n\n";
            int socketID = -1;

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
                        packet = Packet.P_error;
                    }
                });




                switch (packet)
                {
                    case Packet.P_ChatSend:
                        {
                            string message = await Task.Run(() => MessageReceive());
                            if (message == "")
                            {
                                TextBox.Text += $"\n{null,-40}Ошибка соединения!";
                                sendButton.Enabled = false;
                                messageBox.Enabled = false;
                            }
                            if (TextBox.Text.Length > 2000000)
                            {
                                TextBox.Text = $"{null,-20}{DateTime.Now:t} Произошла очистка старых сообщений!\n\n";
                            }
                            TextBox.Text += message;
                        }
                        break;
                    case Packet.P_UserMakeLeader:
                        //int a = players.FindIndex(name => name == Nickname);
                        break;
                    case Packet.P_UserListChange:
                        string mes = await Task.Run(() => MessageReceive());
                        List<string> list= JsonConvert.DeserializeObject<List<string>>(mes);
                        listBox1.Items.Clear();
                        listBox1.Items.AddRange(list.Where(x=>x!="null").ToArray());
                        break;

                    case Packet.P_error:
                        {
                            TextBox.Text += $"\n{null,-40}Ошибка соединения!";
                            sendButton.Enabled = false;
                            messageBox.Enabled = false;
                        }
                        return;
                }



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

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = button1.Text == "Готов" ? "Не готов" : "Готов";
        }
    }
}
