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
using System.IO;
using System.Net.Sockets;

namespace Client_ProfiChat
{
    public partial class ProfiChat : Form
    {

        string stringIP;
        int port;
        TcpClient client;
        //StreamReader sr;
        StreamWriter sw;
        string name;
        NetworkStream ns;
        Thread thread;
        string s;
        volatile bool isConnected = false;
        

        public ProfiChat()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (isConnected == false)
            { 
            stringIP = txtServer.Text;
            port = Int32.Parse(txtPort.Text);
            client = new TcpClient(stringIP, port);
            //sr = new StreamReader(client.GetStream());
            sw = new StreamWriter(client.GetStream());
            name = txtUsername.Text;

            sw.Write(name + ": connected.");
            sw.Flush();

            ns = client.GetStream();
            thread = new Thread(o => ReceiveData((TcpClient)o));
            thread.Start(client);

            isConnected = true;
            }else
            {
                txtChat.AppendText("Already Connected" +Environment.NewLine);
            }
            

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            s = name + " " + txtMessage.Text;
            byte[] buffer = Encoding.ASCII.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);
            ns.Flush();
        }


        public void ReceiveData(TcpClient client)
        {
            ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                txtChat.Invoke(new Action(() =>
                {
                    //txtChat.Text += Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
                    txtChat.AppendText(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
                }));
                //Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));

            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            sw.Write(name + " has disconnected from the Server.");
            sw.Flush();
            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            txtChat.Text += "disconnect from Server!!";
            //Console.ReadKey();
        }

        //public void 
    }
}
