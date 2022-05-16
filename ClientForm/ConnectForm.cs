using System;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ClientForm
{
    public partial class ConnectForm : Form
    {
        private string _ip;
        private int _port;
        List<EndPoint> playerList = new List<EndPoint>();
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
            catch (SocketException e)
            {
                tempSocket.Close();
                return null;
            }
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            this._port = 1111;

            Int32.TryParse(PortBox.Text, out _port);
            try
            {
                Socket socket;
                var adr = playerList[comboBox1.SelectedIndex];
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
                this.Hide();
                Form2 waitingForm = new Form2(socket, nicname.Text);
                waitingForm.Owner = this;
                waitingForm.ShowDialog();
                this.Show();
                socket.Close();
            }
            catch (TimeoutException)
            {
                label2.Text = "Статус: Timeout";
            }
            finally
            {
                checkButton.Enabled = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var text = comboBox.SelectedItem.ToString();
            if (text.Contains("мест:0"))
            {
                checkButton.Enabled = false;
                return;
            }
            checkButton.Enabled = true;
        }

        private async void searchButton_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            label2.Text = "Статус: Поиск серверов";
            try
            {
                searchButton.Enabled = false;

                IPEndPoint ssender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)ssender;
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.EnableBroadcast = true;
                byte[] msg = System.Text.Encoding.ASCII.GetBytes("This is a test\0");
                byte[] playersSize = new byte[sizeof(int)];
                // This call blocks.
                s.SendTo(msg, 0, msg.Length, SocketFlags.None, new IPEndPoint(IPAddress.Parse("192.168.0.255"), 1111));
                var sw = new Stopwatch();
                var task = Task.Run(() =>
                {

                    while (true)
                    {
                        //msg-принять число доступных мест
                        s.ReceiveFrom(playersSize, sizeof(int), SocketFlags.None, ref senderRemote);
                        playerList.Add(senderRemote);
                        this.Invoke(new Action(() => comboBox1.Items.Add(senderRemote + " мест:" + BitConverter.ToInt32(playersSize, 0).ToString())));
                        this.Invoke(new Action(() => comboBox1.SelectedIndex = 0));
                        this.Invoke(new Action(() => checkButton.Visible=true));
                    }
                    

                });
                //ОТМЕНИ ТАСКУ!!!
            }
            catch (TimeoutException)
            {
                label2.Text = "Статус: Timeout";
            }
            finally
            {
                searchButton.Enabled = true;
            }
        }


    }
}
