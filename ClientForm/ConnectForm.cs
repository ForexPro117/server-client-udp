﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class ConnectForm : Form
    {

        private string _ip;
        private int _port;


        public ConnectForm()
        {
            InitializeComponent();
            Random rd = new Random();
            nicname.Text += rd.Next(1, 999);
        }
        private static Socket ConnectSocket(string ip, int port)
        {
            IPAddress address = IPAddress.Parse(ip);

            IPEndPoint ipe = new IPEndPoint(address, port);
            Socket tempSocket =
                new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                tempSocket.Connect(ipe);
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
            label2.Text = "Статус: Проверка";
            checkButton.Enabled = false;
            this._ip = IPBox.Text;
            Int32.TryParse(PortBox.Text, out _port);
            try
            {
                Socket socket;

                var task = Task.Run(() => ConnectSocket(_ip, _port));
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

    }
}
