using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class DataItem
    {
        public string nume { get; set; }
        public string ip { get; set; }
        public string port { get; set; }
    }

    public partial class MainWindow : Window
    {
        int counter = 0;
        int online = 0;
        int port = 54545;
        const int main_port = 54545;
        string broadcastaddress = "192.168.0.154"; // LOCAL IP 
        string mainNode_IP = ""; //PUBLIC IP
        UdpClient recievingClient;
        UdpClient sendingClient;
        Thread recievingThread;
        string externalip;
        bool isHub = false;
        public MainWindow()
        {
            InitializeComponent();
            externalip = new WebClient().DownloadString("http://icanhazip.com");
            if (externalip.Trim() != mainNode_IP)
            {
                broadcastaddress = mainNode_IP;
            }
            else
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    externalip = endPoint.Address.ToString();
                }
            }
            externalip = externalip.Trim();
            ip.Text = externalip;



        }

        private void Reciever()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            while (true)
            {
                byte[] data = recievingClient.Receive(ref endPoint);
                string message = Encoding.ASCII.GetString(data);
                Dispatcher.Invoke(new Action(() =>
                {
                    if (message.Contains("LEAVEhUb#"))
                    {
                        Leave_Hub(message);
                    }
                    else if (message.Contains("ShUtDoWn#"))
                    {
                        Environment.Exit(0);
                    }
                    else if (message.Contains("HUBB#"))
                    {
                        Add_Hub(message);
                    }
                    else if (message.Contains("user#"))
                    {
                        Add_User(message);
                    }
                    else if (message.Contains("LEAVEusr#"))
                    {
                        LEAVEusr(message);
                    }
                    else if (message.Contains("SayHUB#"))
                    {
                        SayHub(message);
                    }
                    else if (message.Contains("say#"))
                    {
                        var x = message.Substring(4);
                        TextBlock_Log.Text += x + "\n";
                    }
                    else if (message.Contains("CNT^users#"))
                    {
                        CNTusers(message);
                    }
                    else if (message.Contains("REZ^users#"))
                    {
                        REZusers(message);

                    }
                    else if (message.Contains("UPDT^usr#"))
                    {
                        UPDT_usr(message);
                    }
                }));
            }
        }

        private void Leave_Hub(string message)
        {
            var msg = message.Split('#');
            if (isHub)
            {
                Send_Message_Users(message);
                Send_Message_Users("say# HUB " + msg[1] + "disconnected with all its nodes");
                TextBlock_Log.Text += "HUB " + msg[1] + "disconnected with all its nodes";
            }
            ListBox_Hubs.Items.Remove(msg[1]);
        }

        private void Add_Hub(string message)
        {
            var x = message.Substring(5).Trim();
            if (!ListBox_Hubs.Items.Contains(x))
            {
                ListBox_Hubs.Items.Add(x);
                repeat(message);
            }
        }

        private void Add_User(string message)
        {
            var x = message.Substring(5);
            var y = x.Split('#');
            if (Convert.ToInt32(y[1]) != main_port && port == main_port && externalip == y[0])
                Become_Hub();
            if (!ListBox_Users.Items.Contains(x))
            {
                TextBlock_Log.Text += "Node " + x + " connected!\n";
                foreach (string user in ListBox_Users.Items)
                {
                    var msg = "user#" + user;
                    if (y[0] != externalip && !isHub)
                        Send_Message(y[0], Convert.ToInt32(y[1]), msg);
                }
                ListBox_Users.Items.Add(x);
            }

            if (isHub)
            {
                foreach (string hub in ListBox_Hubs.Items)
                    Send_Message(y[0], Convert.ToInt32(y[1]), "HUBB#" + hub);
                var msg = "SayHUB#Node " + x + " connected! VIA HUB " + externalip;
                Send_Message_Other_Hubs(msg);
                msg = "say#Node " + x + " connected!";
                Send_Message_Users(msg);
            }
        }

        private void LEAVEusr(string message)
        {
            var x = message.Substring(9).Trim();
            ListBox_Users.Items.Remove(x);
            if(!TextBlock_Log.Text.Contains(x + " just disconnected"))
            TextBlock_Log.Text += x + " just disconnected\n";
            if (isHub)
            {
                Send_Message_Users(message);
            }
        }

        private void SayHub(string message)
        {
            if (isHub)
            {
                if (!message.Contains(" VIA HUB ") && ListBox_Hubs.Items.Count > 1)
                {
                    var msg = message + " VIA HUB " + externalip;
                    Send_Message_Other_Hubs(msg);
                }
                var x = message.Substring(7);
                var msg1 = "say#" + x + " VIA HUB " + externalip;
                Send_Message_Users(msg1);
                TextBlock_Log.Text += message.Substring(7) + "\n";
            }
        }

        private void CNTusers(string message)
        {
            if (isHub)
            {
                var x = message.Split('#')[1].Trim();
                var msg = "REZ^users#" + ListBox_Users.Items.Count;
                Send_Message(x, main_port, msg);
            }
        }

        private void REZusers(string message)
        {
            var x = message.Split('#');
            online += Convert.ToInt32(x[1]);// + ListBox_Hubs.Items.Count;
            counter++;
            if (counter == ListBox_Hubs.Items.Count)
            {
                online += counter;
                online += ListBox_Users.Items.Count;
                counter = 0;
                updatehubs();
            }
        }

        private void UPDT_usr(string message)
        {
            if (isHub)
                updateusers(message);
            var x = message.Split('#')[1];
            Label_nousers.Content = "ONLINE USERS " + x;
        }

        private void updateusers(string message)
        {
            Send_Message_Users(message);
        }

        private void updatehubs()
        {
            var msg = "UPDT^usr#" + online;

            foreach (string hub in ListBox_Hubs.Items)
            {
                Send_Message(hub, main_port, msg);
            }
        }

        private void repeat(string message)
        {
            Send_Message_Other_Hubs(message);
            Send_Message_Users(message);
        }

        private void Become_Hub()
        {
            if (!isHub)
            {
                isHub = true;
                CheckBox_Hub.IsChecked = true;
                var msg = "LEAVEusr#" + externalip + "#" + port;
                var msg2 = "HUBB#" + externalip;
                Send_Message_Users(msg);
                Send_Message_Users(msg2);
                foreach (string hub in ListBox_Hubs.Items)
                {
                    Send_Message(hub, main_port, msg);
                    Send_Message(hub, main_port, msg2);
                }
                ListBox_Hubs.Items.Add(externalip);

            }

        }

        private void Send_Message_Users(string message)
        {
            foreach (string user in ListBox_Users.Items)
            {
                var x = user.Split('#');
                Send_Message(x[0], Convert.ToInt32(x[1]), message);
            }
        }

        private void Send_Message_Other_Hubs(string message)
        {
            foreach (string hub in ListBox_Hubs.Items)
                if (hub != externalip)
                    Send_Message(hub, main_port, message);
        }

        private void Send_Message(string adress, int port, string message)
        {
            sendingClient = new UdpClient(adress.Trim(), port);
            sendingClient.EnableBroadcast = true;
            byte[] data = Encoding.ASCII.GetBytes(message);
            sendingClient.Send(data, data.Length);
            sendingClient.Close();
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_Message.Text))
            {

                if (isHub)
                {
                    string message = "HUB " + externalip + ":" + TextBox_Message.Text;
                    var msg = "SayHUB#" + message + " VIA HUB " + externalip;
                    Send_Message_Other_Hubs(msg);
                    msg = "say#" + message;
                    Send_Message_Users(msg);
                    TextBlock_Log.Text += message + "\n";
                }
                else
                {
                    var msg = "";
                    if (ListBox_Hubs.Items.Count > 0)
                        msg = "SayHUB#";
                    else
                        msg = "say#";
                    msg += externalip + "#" + port + ":" + TextBox_Message.Text;
                    if (ListBox_Hubs.Items.Count == 0)
                    {
                        Send_Message_Users(msg);
                    }
                    else if (ListBox_Hubs.Items.Count == 1)
                        Send_Message(ListBox_Hubs.Items[0].ToString(), main_port, msg);
                    else
                        Send_Message(externalip, main_port, msg);
                }
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Connect.Content.ToString() == "Connect")
            {
                connect();
                Connect.Content = "Disconnect";
            }
            else
            {
                disconnect();
                Connect.Content = "Connect";
            }

        }

        private void disconnect()
        {


            if (!isHub)
            {
                var msg = "LEAVEusr#" + externalip + "#" + port;
                if (ListBox_Hubs.Items.Count == 0)
                    Send_Message_Users(msg);
                else
                    foreach (string hub in ListBox_Hubs.Items)
                        Send_Message(hub, main_port, msg);
            }
            else
            {
                var msg = "LEAVEhUb#" + externalip;
                Send_Message_Other_Hubs(msg);
                var msg1 = "ShUtDoWn#";
                Send_Message_Users(msg1);
            }

            try
            {
                recievingThread.Abort();
                recievingClient.Close();
                ListBox_Hubs.Items.Clear();
                ListBox_Users.Items.Clear();
            }
            catch (Exception) { }
            Environment.Exit(0);
        }

        private void connect()
        {

            bool open_port = (from p in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners() where p.Port == main_port select p).Count() == 1;
            if (!open_port)//&&CheckBox_Hub.IsChecked==true)
            {
                //PORT LIBER
                recievingClient = new UdpClient(main_port);
                recievingThread = new Thread(new ThreadStart(Reciever));
                recievingThread.IsBackground = true;
                recievingThread.Start();
                string message = "user#" + externalip + "#" + main_port;
                if (broadcastaddress != mainNode_IP)
                { ListBox_Users.Items.Add(externalip + "#" + main_port); }
                Send_Message(broadcastaddress, main_port, message);
                // isHub = true;
                port = main_port;
            }
            else
            {
                if (open_port)
                    port = new Random().Next(2000, 60000);

                recievingClient = new UdpClient(port);
                recievingThread = new Thread(new ThreadStart(Reciever));
                recievingThread.IsBackground = true;
                recievingThread.Start();
                string message = "user#" + externalip + "#" + port;
                Send_Message(externalip, 54545, message);
                isHub = false;
            }
            TextBox_User.Text = externalip + " " + port;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            disconnect();
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            // online = -2 * (ListBox_Hubs.Items.Count - 1);
            online = 0;
            if (isHub)
                foreach (string hub in ListBox_Hubs.Items)
                {
                    var msg = "CNT^users#" + externalip;
                    if (hub != externalip || ListBox_Hubs.Items.Count == 1)
                    {
                        Send_Message(hub, main_port, msg);
                    }
                }
            else
                MessageBox.Show("ONLY HUBS CAN REFRESH");
        }
    }
}
