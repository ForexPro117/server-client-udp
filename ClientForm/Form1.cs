using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        P_error,
        P_UserReadyChange,
        P_GameStart
    };
    enum Role
    {
        NoneRole = -1,
        Mafia,
        Com,
        doc,
        Civil

    };
    enum Status
    {
        InDead,
        Healing,
        Dead
    }
    public partial class Form1 : Form
    {
        private static Socket _socket;
        private string Nickname;
        Role _role;
        int _playerIndex;
        int countReadyPlayrs = 0;
        List<string> _players;

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
        internal List<string> ListReceive()
        {
            countReadyPlayrs = 0;
            byte[] data = new byte[2048];
            byte[] sizeBuf = new byte[sizeof(int)];
            bool[] ready = new bool[8];
            List<string> list = new List<string>();
            int mesLenth;
            string mes;
            int bytes;

            try
            {
                for (int i = 0; i < 2; i++)
                {
                    _socket.Receive(sizeBuf, sizeof(int), 0);
                    mesLenth = BitConverter.ToInt32(sizeBuf, 0);
                    _socket.Receive(data, mesLenth, 0);
                    mes = Encoding.UTF8.GetString(data, 0, mesLenth);
                    if (i == 0)
                        list = JsonConvert.DeserializeObject<List<string>>(mes);
                    else
                        ready = JsonConvert.DeserializeObject<bool[]>(mes);
                }
                _players = list.Where(x => x != "null").ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == "null")
                        continue;
                    if (ready[i])
                    {
                        list[i] += "✓";
                        countReadyPlayrs++;
                    }
                }
                return list;

            }
            catch (SocketException)
            {
                return null;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        internal List<Role> GetRoleList()
        {
            string listJson = MessageReceive();
            List<Role> listRole = JsonConvert.DeserializeObject<List<Role>>(listJson);
            listRole = listRole.Where(x => x != Role.NoneRole).ToList();
            _playerIndex = _players.IndexOf(Nickname);
            Task.Delay(12000).Wait();
            switch (listRole[_playerIndex])
            {
                case Role.Mafia:
                    pictureBox2.Image = ClientForm.Properties.Resources.mafia;
                    this.Invoke(new Action(() => TextBox.Text = "СИСТЕМА: Ваша роль - МАФИЯ!\n" +
                        "Убейте всех своих врагов вместе со своими союзниками(если они есть)\n"));
                    _role = Role.Mafia;
                    break;
                case Role.Com:
                    pictureBox2.Image = ClientForm.Properties.Resources.commissar;
                    this.Invoke(new Action(() => TextBox.Text = TextBox.Text = "СИСТЕМА: Ваша роль - Комиссар!\n" +
                        "Поймайте преступников, но будьте осторожны и не задержите невиновных!\n"));
                    _role = Role.Com;
                    break;
                case Role.doc:
                    pictureBox2.Image = ClientForm.Properties.Resources.doctor;
                    this.Invoke(new Action(() => TextBox.Text = TextBox.Text = "СИСТЕМА: Ваша роль - Доктор!\n" +
                        "Вы можете спасти человека от гибели, но зачем вам это!?\n"));
                    _role = Role.doc;
                    break;
                case Role.Civil:
                    pictureBox2.Image = ClientForm.Properties.Resources.civil;
                    this.Invoke(new Action(() => TextBox.Text = TextBox.Text = "СИСТЕМА: Ваша роль - Мирный житель!\n" +
                        "Вы можете только умирать и голосовать на совете, ужасная участь...\n"));
                    _role = Role.Civil;
                    break;
                default:
                    break;
            }

            return listRole;
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

            _socket.Send(BitConverter.GetBytes(Nickname.Length), sizeof(int), 0);
            _socket.Send(Encoding.UTF8.GetBytes(Nickname), Nickname.Length, 0);
            Packet packet = Packet.P_error;
            byte[] buffer = new byte[sizeof(Packet)];
            TextBox.Text = $"{null,-35}Соединение установленно!\n\n";
            TextBox.Text = "СИСТЕМА: Ожидание подключения игроков!";

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
                    case Packet.P_UserReadyChange:
                        break;

                    case Packet.P_UserListChange:
                        var result = await Task.Run(() => ListReceive());
                        listBox1.Items.Clear();
                        listBox1.Items.AddRange(result.Where(x => x != "null").ToArray());
                        //TTTTTTYTA KOLWO
                        button2.Enabled = countReadyPlayrs > 0 ? true : false;
                        break;
                    case Packet.P_GameStart:
                        button1.Visible = false;
                        button2.Visible = false;

                        Timer timer = new Timer();
                        int time = 10;
                        timer.Tick += new EventHandler((send, EventArgs) =>
                        {
                            TextBox.Text = $"СИСТЕМА: До начала {time} секунд!";
                            if (time == 0)
                            {

                                timer.Stop();
                            }
                            time--;
                        });
                        timer.Interval = 1000;
                        timer.Start();

                        var roleList = await Task.Run(() => GetRoleList());
                        
                        listBox1.Items.Clear();
                        listBox1.Items.AddRange(_players.ToArray());
                        listBox1.Items[_playerIndex] = "Вы:" + listBox1.Items[_playerIndex];
                        if (_role == Role.Mafia)
                        {
                            for (int i = 0; i < _players.Count; i++)
                                if (roleList[i] == Role.Mafia)
                                    listBox1.Items[i] += "- Мафия";
                        }



                        break;
                    case Packet.P_error:
                        {
                            TextBox.Text += $"\n{null,-40}Ошибка соединения!";
                            sendButton.Enabled = false;
                            messageBox.Enabled = false;
                        }
                        return;
                    default:
                        break;
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
            Task.Run(() =>
            _socket.Send(BitConverter.GetBytes((int)Packet.P_UserReadyChange), sizeof(Packet), 0));
            button1.Text = button1.Text == "Готов" ? "Не готов" : "Готов";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            _socket.Send(BitConverter.GetBytes((int)Packet.P_GameStart), sizeof(Packet), 0));

        }


    }
}
