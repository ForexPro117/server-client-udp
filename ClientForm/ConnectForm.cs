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
        private Socket ConnectSocket(EndPoint serverAdr)
        {
            Socket tempSocket =
                new Socket(serverAdr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                tempSocket.Connect(serverAdr);
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
            label2.Text = "Статус: Проверка";
            checkButton.Enabled = false;
            this._ip = IPBox.Text;
            Int32.TryParse(PortBox.Text, out _port);
            try
            {
                Socket socket;
                EndPoint adr= serverList[listBox1.SelectedIndex];
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
                Form1 messageForm = new Form1(socket, nicname.Text);
                messageForm.Owner = this;
                messageForm.ShowDialog();
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

        private void udpController()
        {
            IPEndPoint ssender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)ssender;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverList.Clear();
            s.EnableBroadcast = true;
            s.ReceiveTimeout = 5000;
            byte[] msg = BitConverter.GetBytes(1);
            // This call blocks.
            s.SendTo(msg, 0, sizeof(int), SocketFlags.None, new IPEndPoint(IPAddress.Broadcast, 1111));

            try
            {
                while (true)
                {
                    s.ReceiveFrom(msg, sizeof(int), SocketFlags.None, ref senderRemote);
                    serverList.Add(senderRemote);
                    this.Invoke(new Action(() => listBox1.Items.Add(senderRemote + $" Игроков:{BitConverter.ToInt32(msg, 0)}/8")));
                }

            }
            catch (SocketException)
            {

            }
            finally
            {
                this.Invoke(new Action(() => listBox1.Items.Add(senderRemote + $" Игроков:8/8")));
                s.Close();
            }


        }

        private async void checkButton1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            button1.Enabled = false;
            await Task.Run(() => udpController());
            button1.Enabled = true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var list = (ListBox)sender;
            checkButton.Enabled = list.SelectedItem.ToString().Contains("8/8") ? false : true;

        }

    }
}
