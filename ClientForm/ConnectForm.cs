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
            try
            {

                Socket socket;
                //TODO Не забыть убрать!!!
                //EndPoint adr = serverList[listBox1.SelectedIndex];
                EndPoint adr = new IPEndPoint(IPAddress.Parse("127.0.0.1"),1111);
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
                listBox1.Items.Clear();
                checkButton.Enabled = false;
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
            catch (ArgumentException)
            {
                label2.Text = "Статус: Ошибка";
            }
            finally
            {
                checkButton.Enabled = true;
            }



        }

        private void udpController(EndPoint address = null)
        {

            IPEndPoint ssender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)ssender;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverList.Clear();
            s.EnableBroadcast = true;
            s.ReceiveTimeout = 5000;
            byte[] msg = BitConverter.GetBytes(1);
            // This call blocks.
            if (address == null)
                address = new IPEndPoint(IPAddress.Broadcast, _port);
            s.SendTo(msg, 0, sizeof(int), SocketFlags.None, address);

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
               
                s.Close();
            }


        }

        private async void checkButton1_Click(object sender, EventArgs e)
        {
            label2.Text = "Статус: Поиск доступных серверов";
            label4.Visible = false;
            IPBox.Visible = false;
            listBox1.Items.Clear();
            button1.Enabled = false;
            try
            {
                this._ip = IPBox.Text;
                IPBox.Text = "";
                this._port = Int32.Parse(PortBox.Text);
                if (_ip == "")
                    await Task.Run(() => udpController());
                else
                    await Task.Run(() => udpController(new IPEndPoint(IPAddress.Parse(_ip), _port)));
            }
            catch (ArgumentException)
            {
                label2.Text = "Статус: Ошибка аргумента";
                return;
            }
            finally
            {
                button1.Enabled = true;
            }
            label2.Text = "Статус: Поиск завершен";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var list = (ListBox)sender;
            if (list.Text != "")
            checkButton.Enabled = list.SelectedItem.ToString().Contains("8/8") ? false : true;

        }
        int counter = 0;
        private void hiden_click(object sender, EventArgs e)
        {
            counter++;
            if (counter == 5)
            {
                label4.Visible = true;
                IPBox.Visible = true;
                counter = 0;
            }
            else
            {
                label4.Visible = false;
                IPBox.Visible = false;
            }



        }

        private void ConnectForm_Load(object sender, EventArgs e)
        {
            checkButton_Click(sender, e);
        }
    }

}
