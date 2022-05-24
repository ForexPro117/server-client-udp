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
        P_error,
        P_UserReadyChange,
        P_GameStart,
        P_NextStage,
        P_SelectChange
    };
    enum Role
    {
        NoneRole = -1,
        Mafia,
        Com,
        doc,
        Civil

    };
    public partial class Form1 : Form
    {
        private static Socket _socket;
        private string Nickname;
        Role _role;
        int _playerIndex;
        int countReadyPlayrs = 0;
        List<string> _players;
        GameController gameObject;


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

        internal void GetRoleList()
        {
            string listJson = MessageReceive();
            List<Role> listRole = JsonConvert.DeserializeObject<List<Role>>(listJson);
            listRole = listRole.Where(x => x != Role.NoneRole).ToList();
            _playerIndex = _players.IndexOf(Nickname);
            if (_playerIndex == -1)
            {
                throw new ArgumentException("Это ты виноват!");
            }
            gameObject = new GameController(listRole, _playerIndex);

            Task.Delay(4000).Wait();
            switch (gameObject.UserRole)
            {
                case Role.Mafia:
                    pictureBox2.Image = ClientForm.Properties.Resources.mafia;
                    this.Invoke(new Action(() => TextBox.Text = "Система: Ваша роль - МАФИЯ!\n" +
                        "Убейте всех своих врагов вместе со своими союзниками(если они есть)\n"));
                    _role = Role.Mafia;
                    break;
                case Role.Com:
                    pictureBox2.Image = ClientForm.Properties.Resources.commissar;
                    this.Invoke(new Action(() => TextBox.Text = TextBox.Text = "Система: Ваша роль - Комиссар!\n" +
                        "Поймайте преступников, но будьте осторожны и не задержите невиновных!\n"));
                    _role = Role.Com;
                    break;
                case Role.doc:
                    pictureBox2.Image = ClientForm.Properties.Resources.doctor;
                    this.Invoke(new Action(() => TextBox.Text = TextBox.Text = "Система: Ваша роль - Доктор!\n" +
                        "Вы можете спасти человека от гибели, но зачем вам это!?\n"));
                    _role = Role.doc;
                    break;
                case Role.Civil:
                    pictureBox2.Image = ClientForm.Properties.Resources.civil;
                    this.Invoke(new Action(() => TextBox.Text = TextBox.Text = "Система: Ваша роль - Мирный житель!\n" +
                        "Вы можете только умирать и голосовать на совете, ужасная участь...\n"));
                    _role = Role.Civil;
                    break;
                default:
                    break;
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

            var sendName = Encoding.UTF8.GetBytes(Nickname + "\0");
            _socket.Send(BitConverter.GetBytes(sendName.Length), sizeof(int), 0);
            _socket.Send(sendName, sendName.Length, 0);
            Packet packet = Packet.P_error;
            byte[] buffer = new byte[sizeof(Packet)];
            TextBox.Text = $"{null,-35}Соединение установлено!\n\n";
            TextBox.Text = "Система: Ожидание подключения игроков!";

            Timer timer;
            int time;


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
                            switch (gameObject.stage)
                            {
                                case 0:
                                    TextBox.Text += message;
                                    break;
                                case 1:
                                    if (gameObject.UserRole == Role.Mafia)
                                        TextBox.Text += message;
                                    break;
                                case 4:
                                    TextBox.Text += message;
                                    break;
                            }
                        }
                        break;
                    case Packet.P_UserReadyChange:
                        break;

                    case Packet.P_UserListChange:
                        {
                            var result = await Task.Run(() => ListReceive());
                            if (result != null)
                            {
                                listBox1.Items.Clear();
                                listBox1.Items.AddRange(result.Where(x => x != "null").ToArray());
                                //Сделай больше 0!!!!
                                button2.Enabled = countReadyPlayrs > 3 ? true : false;
                            }
                        }
                        break;


                    case Packet.P_GameStart:
                        {
                            button1.Visible = false;
                            button2.Visible = false;
                            messageBox.Enabled = false;
                            sendButton.Enabled = false;
                            listBox1.Enabled = false;
                            timer = new Timer();
                            time = 5;
                            timer.Tick += new EventHandler((send, EventArgs) =>
                            {
                                TextBox.Text = $"Система: До начала {time} секунд!";
                                if (time == 0)
                                {

                                    timer.Stop();
                                }
                                time--;
                            });
                            timer.Interval = 1000;
                            timer.Start();
                            //**//

                            await Task.Delay(TimeSpan.FromSeconds(time))
                                .ContinueWith(_ => GetRoleList());

                            listBox1.Items.Clear();
                            _players[_playerIndex] = "*" + _players[_playerIndex];
                            if (gameObject.UserRole == Role.Mafia)
                            {
                                for (int i = 0; i < _players.Count; i++)
                                    if (gameObject.RoleList[i] == Role.Mafia)
                                        _players[i] += "(М)";
                            }
                            listBox1.Items.AddRange(_players.ToArray());
                            gameObject.stage = 1;
                            if (gameObject._playerIndex == 0)
                                _socket.Send(BitConverter.GetBytes((int)Packet.P_NextStage), sizeof(Packet), 0);
                        }
                        break;

                    case Packet.P_NextStage:
                        ///--------
                        ///
                        timer = new Timer();
                        time = 20;
                        progressBar1.Maximum = time;
                        progressBar1.Value = time;
                        progressBar1.Visible = true;
                        timer.Tick += new EventHandler(async (send, EventArgs) =>
                        {
                            if (time == 0)
                            {

                                timer.Stop();


                                messageBox.Enabled = false;
                                sendButton.Enabled = false;
                                listBox1.Enabled = false;
                                var maxVoices = gameObject.selectorList.Max();
                                var playerPosition = gameObject.selectorList.IndexOf(maxVoices);
                                gameObject.ClearSelector();
                                if (maxVoices > 0)
                                {
                                    switch (gameObject.stage)
                                    {
                                        case 1:
                                            gameObject.SetStatus(playerPosition, Status.InDead);
                                            break;
                                        case 2:
                                            gameObject.SetStatus(playerPosition, Status.Healing);
                                            break;
                                        case 3:
                                            gameObject.SetStatus(playerPosition, Status.InPrison);
                                            break;
                                        case 4:
                                            gameObject.PlayerKill(playerPosition);
                                            TextBox.Text += $"Игрок \"{_players[playerPosition] }\" " +
                                            $"вылетает по решению голосования!\n" +
                                                $"Его роль - \"{gameObject.RoleList[playerPosition]}\"\n";
                                            _players[playerPosition] += $" ({gameObject.RoleList[playerPosition]})👻";
                                            listBox1.Items.Clear();
                                            listBox1.Items.AddRange(_players.ToArray());
                                            if (!this.CanContinue())
                                                break;
                                            await Task.Delay(TimeSpan.FromSeconds(4));
                                            gameObject.stage = 0;
                                            break;
                                    }
                                }

                                gameObject.stage++;
                                if (gameObject._playerIndex == 0 && gameObject.stage < 5)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(4));
                                    _socket.Send(BitConverter.GetBytes((int)Packet.P_NextStage), sizeof(Packet), 0);
                                }
                                return;
                            }
                            progressBar1.Value--;
                            time--;
                        });
                        timer.Interval = 1000;
                        //----
                        switch (gameObject.stage)
                        {
                            case 1:
                                TextBox.Text = $"{null,-35}Начало новой ночи!\n";
                                TextBox.Text += "Система:Сейчас ход мафии! 20 сек на ход!\n";
                                if (gameObject.UserRole == Role.Mafia &&
                                    gameObject.CanAnyAction(gameObject._playerIndex))
                                {
                                    TextBox.Text += "Мафия может общаться между собой.\n";
                                    messageBox.Enabled = true;
                                    sendButton.Enabled = true;
                                    listBox1.Enabled = true;
                                }
                                timer.Start();
                                break;
                            case 2:
                                if (gameObject.doctor > 0)
                                {

                                    TextBox.Text += "Система:Сейчас ход доктора! 20 сек на ход!\n";
                                    if (gameObject.UserRole == Role.doc &&
                                        gameObject.CanAnyAction(gameObject._playerIndex))
                                    {
                                        TextBox.Text += "Доктор может спасти себя!\n";
                                        listBox1.Enabled = true;
                                    }
                                    timer.Start();
                                }
                                else
                                {
                                    gameObject.stage++;
                                    if (gameObject._playerIndex == 0)
                                        _socket.Send(BitConverter.GetBytes((int)Packet.P_NextStage), sizeof(Packet), 0);
                                }
                                break;
                            case 3:
                                if (gameObject.comisar > 0)
                                {

                                    TextBox.Text += "Система:Сейчас ход комиссара! 20 сек на ход!\n";
                                    if (gameObject.UserRole == Role.Com &&
                                        gameObject.CanAnyAction(gameObject._playerIndex))
                                    {
                                        TextBox.Text += "Не стесняйтесь сажать в тюрьму кого хотите!\n";
                                        listBox1.Enabled = true;
                                    }
                                    timer.Start();
                                }
                                else
                                {
                                    gameObject.stage++;
                                    if (gameObject._playerIndex == 0)
                                        _socket.Send(BitConverter.GetBytes((int)Packet.P_NextStage), sizeof(Packet), 0);
                                }
                                break;
                            case 4:
                                TextBox.Text = $"{null,-40}Наступило утро!\n" +
                                        "Произошли такие события:\n" +
                                        "******************************\n";
                                var list = gameObject.GetStatusList();
                                for (int i = 0; i < list.Count; i++)
                                {
                                    switch (list[i])
                                    {
                                        case Status.Healing:
                                            list[i] = Status.Alive;
                                            break;

                                        case Status.InDead:
                                            gameObject.PlayerKill(i);
                                            TextBox.Text += $"Игрок \"{_players[i] }\" убит!" +
                                                $" Его роль - \"{gameObject.RoleList[i]}\"\n";
                                            _players[i] += $" ({gameObject.RoleList[i]})👻";
                                            break;

                                        case Status.InPrison:
                                            gameObject.PlayerKill(i);
                                            TextBox.Text += $"Игрок \"{_players[i] }\" отправлен в тюрьму!" +
                                                $" Его роль - \"{gameObject.RoleList[i]}\"\n";
                                            _players[i] += $" ({gameObject.RoleList[i]})👻";
                                            break;
                                    }
                                }
                                TextBox.Text += "******************************\n";
                                listBox1.Items.Clear();
                                listBox1.Items.AddRange(_players.ToArray());

                                if (!this.CanContinue())
                                    break;


                                time = 60;
                                progressBar1.Maximum = time;
                                progressBar1.Value = time;
                                if (gameObject.CanAnyAction(gameObject._playerIndex))
                                {
                                    TextBox.Text += "У вас 60 сек, чтобы выбрать кого отправть в тюрьму!\n";
                                    messageBox.Enabled = true;
                                    sendButton.Enabled = true;
                                    listBox1.Enabled = true;
                                }


                                timer.Start();
                                break;

                        }

                        break;
                    case Packet.P_SelectChange:
                        string mess = await Task.Run(() => MessageReceive());
                        gameObject.selectorList = JsonConvert.DeserializeObject<List<int>>(mess);
                        switch (gameObject.stage)
                        {
                            case 1:
                                if (gameObject.UserRole == Role.Mafia)
                                {
                                    listBox1.Items.Clear();
                                    listBox1.Items.AddRange(_players.ToArray());
                                    for (int i = 0; i < gameObject.selectorList.Count; i++)
                                        for (int j = 0; j < gameObject.selectorList[i]; j++)
                                            listBox1.Items[i] += "🔪";
                                }
                                break;
                            case 2:
                                if (gameObject.UserRole == Role.doc)
                                {
                                    listBox1.Items.Clear();
                                    listBox1.Items.AddRange(_players.ToArray());
                                    for (int i = 0; i < gameObject.selectorList.Count; i++)
                                        for (int j = 0; j < gameObject.selectorList[i]; j++)
                                            listBox1.Items[i] += "💊";
                                }
                                break;
                            case 3:
                                if (gameObject.UserRole == Role.Com)
                                {
                                    listBox1.Items.Clear();
                                    listBox1.Items.AddRange(_players.ToArray());
                                    for (int i = 0; i < gameObject.selectorList.Count; i++)
                                        for (int j = 0; j < gameObject.selectorList[i]; j++)
                                            listBox1.Items[i] += "🔒";
                                }
                                break;
                            case 4:
                                {
                                    listBox1.Items.Clear();
                                    listBox1.Items.AddRange(_players.ToArray());
                                    for (int i = 0; i < gameObject.selectorList.Count; i++)
                                        for (int j = 0; j < gameObject.selectorList[i]; j++)
                                            listBox1.Items[i] += "⚖";
                                }
                                break;
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

        void SendChoice(ListBox list)
        {
            gameObject.SetChoice(list.SelectedIndex);
            Byte[] bytesSend;
            string message = JsonConvert.SerializeObject(gameObject.selectorList);
            bytesSend = Encoding.UTF8.GetBytes(message);
            _socket.Send(BitConverter.GetBytes((int)Packet.P_SelectChange), sizeof(Packet), 0);
            _socket.Send(BitConverter.GetBytes(bytesSend.Length), sizeof(int), 0);
            _socket.Send(bytesSend, bytesSend.Length, 0);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gameObject != null)
            {
                var list = (ListBox)sender;
                if (list.Text != "")
                    switch (gameObject.stage)
                    {
                        case 1:
                            if (gameObject.RoleList[list.SelectedIndex] != Role.Mafia &&
                                gameObject.CanAnyAction(list.SelectedIndex))
                                SendChoice(list);
                            break;
                        case 2:
                            if (gameObject.CanAnyAction(list.SelectedIndex))
                                SendChoice(list);
                            break;
                        case 3:
                            if (list.SelectedIndex != gameObject._playerIndex &&
                                gameObject.CanAnyAction(list.SelectedIndex))
                                SendChoice(list);
                            break;
                        case 4:
                            if (gameObject.CanAnyAction(list.SelectedIndex))
                                SendChoice(list);
                            break;
                    }

            }
        }
        private bool CanContinue()
        {
            if (gameObject.mafia == 0)
            {
                TextBox.Text += "ФИНАЛ: Победа мирных жителей," +
                    "мафии не осталось!";
                button1.Visible = true;
                button2.Visible = true;
                listBox1.Enabled = true;
                return false;
            }


            var playersAlive = gameObject
               .GetStatusList()
               .Where(x => x == Status.Alive)
               .Count();
            if (playersAlive - gameObject.mafia < 2)
            {
                TextBox.Text += "ФИНАЛ: Победа мафии!";
                button1.Visible = true;
                button2.Visible = true;
                listBox1.Enabled = true;
                return false;
            }
            return true;
        }
    }
}
