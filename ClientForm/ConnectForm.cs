using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class ConnectForm : Form
    {
        private string _ip;
        private int _port;
        List<EndPoint> serverList = new List<EndPoint>();
        public ConnectForm()
        {
            InitializeComponent();
            Random rd = new Random();
            nicname.Text += rd.Next(1, 999);
        }
        private static Socket ConnectSocket(EndPoint address)
        {

            Socket tempSocket =
                new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                tempSocket.Connect(address);
                return tempSocket;
            }
            catch (SocketException)
            {
                tempSocket.Close();
                return null;
            }
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
           
            try
            {
                Socket socket;
               // var adr = serverList[comboBox1.SelectedIndex];
                //*************//
               var adr = new IPEndPoint(IPAddress.Parse("192.168.0.40"),1111);
                var task = Task.Run(() => ConnectSocket(adr));

                if (task.Wait(TimeSpan.FromSeconds(5)))
                    socket = task.Result;
                else
                    throw new TimeoutException();
                if (socket == null)
                {
                    label2.Text = "Статус: Ошибка соединения";
                    return;
                }
                label2.Text = "Статус: Успех";
                serverList.Clear();
                comboBox1.Items.Clear();
                comboBox1.Text = "Список серверов";
                checkButton.Enabled = false;
                this.Hide();
                Form2 room = new Form2(socket, nicname.Text);
                room.Owner = this;
                room.ShowDialog();
                this.Show();
                socket.Close();
            }
            catch (TimeoutException)
            {
                label2.Text = "Статус: Timeout";
                checkButton.Enabled = true;
            }
            finally
            {
                
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var text = comboBox.SelectedItem.ToString();
            if (text.Contains("8/8"))
            {
                checkButton.Enabled = false;
                return;
            }
            checkButton.Enabled = true;
        }

        private async void searchButton_Click(object sender, EventArgs e)
        {
            searchButton.Enabled = false;
            comboBox1.Items.Clear();
            label2.Text = "Статус: Поиск серверов";
            try
            {
                this._port = Int32.Parse(PortBox.Text);
                await Task.Run(() => udpController());
            }
            catch (FormatException)
            {
                label2.Text = "Статус: Ошибка аргумента";
                return;
            }
            finally
            {
                searchButton.Enabled = true;
            }
            label2.Text = "Статус: Поиск завершен";
        }

        private void udpController()
        {

            IPEndPoint ssender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)ssender;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.EnableBroadcast = true;
            s.ReceiveTimeout = 2000;
            byte[] msg = BitConverter.GetBytes(1);
            serverList.Clear();
            EndPoint address = new IPEndPoint(IPAddress.Broadcast, _port);
            s.SendTo(msg, 0, sizeof(int), SocketFlags.None, address);

            try
            {
                while (true)
                {
                    s.ReceiveFrom(msg, sizeof(int), SocketFlags.None, ref senderRemote);
                    serverList.Add(senderRemote);
                    this.Invoke(new Action(() => comboBox1.Items.Add(senderRemote + $" Игроков:{BitConverter.ToInt32(msg, 0)}/8")));
                }

            }
            catch (SocketException)
            {

            }
            finally
            {

                s.Close();
            }


        }

        private void ConnectForm_Load(object sender, EventArgs e)
        {
           checkButton_Click(sender, e);
        }
    }
}
