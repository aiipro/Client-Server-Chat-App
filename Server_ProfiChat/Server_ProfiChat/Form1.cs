using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Server_ProfiChat
{
    public partial class Form1 : Form
    {

        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        volatile bool isConnected = false;
        Thread t;
        int count;
        TcpListener ServerSocket;

        public Form1()
        {
            InitializeComponent();

        }

        public void handle_clients(object o)
        {
            int id = (int)o;
            Console.WriteLine(o);
            TcpClient client;

            lock (_lock) client = list_clients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(data);
                //Console.WriteLine(data);
                //var chatline = txtChat.Text;
                txtChat.Invoke(new Action(() =>
                {
                    //txtChat.Text += data + Environment.NewLine;
                    txtChat.AppendText(data + Environment.NewLine);
                }));
                
                //Form1 formObj = new Form1();
                //formObj.txtChat.Text += data;
            }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void broadcast(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }








        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (isConnected == false)
            {
                count = 1;

                string serverIP = txtServerIP.Text;
                int serverPort = Int32.Parse(txtServerPort.Text);

                ServerSocket = new TcpListener(IPAddress.Parse(serverIP), serverPort);
                isConnected = true;

                try
                {
                    ServerSocket.Start();
                    txtChat.Text += "Server online!" + Environment.NewLine;

                }
                catch
                {
                    txtChat.Text += "Fehler beim Verbinden mit dem Server" + Environment.NewLine;
                }
            }else
            {
                txtChat.AppendText("Already Connected!" + Environment.NewLine);
            }

            while (isConnected==true)
            {
                //TcpClient client = ServerSocket.AcceptTcpClient();
                TcpClient client = await ServerSocket.AcceptTcpClientAsync();
                lock (_lock) list_clients.Add(count, client);
                Console.WriteLine("Someone connected!!");

                t = new Thread(handle_clients);
                t.Start(count);
                count++;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            //isConnected = false;
            //Console.WriteLine(list_clients.Values);
            //Console.WriteLine(list_clients.Count);     ///<- ANZAHL

            Console.WriteLine(list_clients[1]);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            isConnected = false;
            t.Join();
        }
    }
}
