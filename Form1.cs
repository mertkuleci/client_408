using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace Server_Application_CS408
{
    public partial class Server_Application : Form
    {
        private class ClientInfo
        {
            public TcpClient TcpClient { get; }
            public string IP { get; }
            public int Port { get; }
            public string Username { get; set; }  // Could be set after its initialization.

            public ClientInfo(TcpClient tcpClient)
            {
                TcpClient = tcpClient;
                var endPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                IP = endPoint.Address.ToString();
                Port = endPoint.Port;
            }
        }

        private TcpListener tcpListener;
        private Thread listenThread;
        private List<ClientInfo> clients = new List<ClientInfo>();

        public Server_Application()
        {
            InitializeComponent();
        }

        private void StartServer(string ip, int port)
        {
            try
            {
                IPAddress ipAddress;

                if (IPAddress.TryParse(ip, out ipAddress))
                {
                    tcpListener = new TcpListener(ipAddress, port);
                    listenThread = new Thread(new ThreadStart(ListenForClients));
                    listenThread.Start();
                    richTextBox_Actions.AppendText($"Server started on {ip}:{port}\n");
                    button_ServerStart.Enabled = false;     // Because all the inputs are valid and worked, we can disable the button from now on.
                }
                else
                {
                    richTextBox_Actions.AppendText($"Invalid IP address: {ip}\n");
                }
            }
            catch (Exception ex)
            {
                richTextBox_Actions.AppendText($"Error starting server: {ex.Message}\n");
            }
        }
        private void UpdateRichTextBox(string message)
        {
            if (richTextBox_Actions.InvokeRequired)
            {
                richTextBox_Actions.Invoke(new Action<string>(UpdateRichTextBox), message);
            }
            else
            {
                richTextBox_Actions.AppendText(message);
            }
        }

        private void ListenForClients()
        {
            // TRY & CATCH BLOCK DEFINETELY!
            tcpListener.Start();
            
            while (true)
            {
                // TRY & CATCH BLOCK MIGHT BE NECESSARY FOR THE COMMENT THREADS!

                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                ClientInfo clientInfo = new ClientInfo(tcpClient);
                clients.Add(clientInfo);

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(clientInfo);
            }
        }

        private void HandleClientComm(object clientObj)
        {
            ClientInfo clientInfo = (ClientInfo)clientObj;
            TcpClient tcpClient = clientInfo.TcpClient;
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;

            try
            {
                if (clientInfo == null || clientInfo.TcpClient == null || !clientInfo.TcpClient.Connected)
                    return;

                while (clientInfo.TcpClient.Connected)
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                    if (bytesRead == 0)
                        break;

                    string data = Encoding.ASCII.GetString(message, 0, bytesRead);

                    ProcessMessage(clientInfo, data);

                }
            }
            catch (Exception ex)
            {
                UpdateRichTextBox($"Error with client {clientInfo.Username}: {ex.Message}\n");
            }
            finally
            {
                // Close the resources if the client is still connected
                if (clientInfo != null && clientInfo.TcpClient != null && clientInfo.TcpClient.Connected)
                {
                    clientInfo.TcpClient.GetStream().Close();
                    clientInfo.TcpClient.Close();
                    UpdateRichTextBox($"Client disconnected: {clientInfo.Username}\n");
                }
                clients.Remove(clientInfo);

            }
        }

        private void HandleClientConnect(ClientInfo clientInfo, string username)
        {
            // Check if the username is already in use
            if (IsUsernameUnique(username))
            {
                // Username is unique, store it in the ClientInfo instance
                clientInfo.Username = username;

                // Add the client to the list of connected clients
                connectedClients.Add(clientInfo);

                // Print the username in the actions richtext box after it is parsed properly.
                UpdateRichTextBox($"Client connected: {clientInfo.Username}\n");
            }
            else
            {
                // Username is already in use, disconnect the client
                UpdateRichTextBox($"A client tried to access with a username that is already exist. Request is rejected\n");
                DisconnectClient(clientInfo);
            }
        }
        private bool IsUsernameUnique(string username)
        {
            // Check if the username is unique among connected clients
            return !connectedClients.Any(client => client.Username != null && client.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        private void DisconnectClient(ClientInfo clientInfo)
        {
            try
            {
                NetworkStream clientStream = clientInfo.TcpClient.GetStream();
                clientStream.Close();
                clientInfo.TcpClient.Close();
            }
            catch (Exception ex)
            {
                UpdateRichTextBox($"Error disconnecting client {clientInfo.Username}: {ex.Message}\n");
            }
        }

        private void ProcessMessage(ClientInfo clientInfo, string message)
        {
            // Assuming messages are formatted as "ACTION|CHANNEL|DATA"
            string[] parts = message.Split('|');

            if (parts.Length >= 2)
            {
                string action = parts[0].ToUpper();
                string channel = parts[1].ToUpper();

                switch (action)
                {
                    case "CONNECT":
                        string username = parts[1];
                        HandleClientConnect(clientInfo, username);
                        break;
                    case "SUBSCRIBE":
                        SubscribeToChannel(clientInfo, channel);
                        break;
                    case "UNSUBSCRIBE":
                        UnsubscribeFromChannel(clientInfo, channel);
                        break;
                    case "SEND":
                        if (parts.Length >= 3)
                        {
                            string data = parts[2];
                            SendMessageToChannel(clientInfo, channel, data);
                        }
                        break;
                        // Add more cases for other actions if needed
                }
            }
            else
            {
                richTextBox_Actions.AppendText($"Invalid message format from {clientInfo.Username}: {message}\n");
            }
        }

        private List<ClientInfo> subscribedClientsIF100 = new List<ClientInfo>();
        private List<ClientInfo> subscribedClientsSPS101 = new List<ClientInfo>();
        private List<ClientInfo> connectedClients = new List<ClientInfo>();

        private void UpdateSubscribedClientsList(string channel, List<ClientInfo> subscribedClients, RichTextBox richTextBox)
        {
            Invoke(new Action(() =>
            {
                richTextBox.Clear();

                foreach (var client in subscribedClients)
                {
                    richTextBox.AppendText($"Client {client.Username}\n");
                }
            }));
        }


        private void SubscribeToChannel(ClientInfo clientInfo, string channel)
        {
            switch (channel.ToUpper())
            {
                case "IF100":
                    if (!subscribedClientsIF100.Contains(clientInfo))
                    {
                        subscribedClientsIF100.Add(clientInfo);
/*
                        if (!allSubscribedClients.Contains(clientInfo))
                        {
                            allSubscribedClients.Add(clientInfo);
                            UpdateSubscribedClientsList("All", allSubscribedClients, richTextBox_AllChannels);
                        }*/

                        UpdateSubscribedClientsList("IF100", subscribedClientsIF100, richTextBox_IF100);
                    }
                    break;
                case "SPS101":
                    if (!subscribedClientsSPS101.Contains(clientInfo))
                    {
                        subscribedClientsSPS101.Add(clientInfo);

                        /*
                        if (!allSubscribedClients.Contains(clientInfo))
                        {
                            allSubscribedClients.Add(clientInfo);
                            UpdateSubscribedClientsList("All", allSubscribedClients, richTextBox_AllChannels);
                        }*/

                        UpdateSubscribedClientsList("SPS101", subscribedClientsSPS101, richTextBox_SPS101);
                    }
                    break;
                default:
                    Invoke(new Action(() =>
                    {
                        richTextBox_Actions.AppendText($"Invalid channel.");
                    }));
                    break;
            }
        }

        private void UnsubscribeFromChannel(ClientInfo clientInfo, string channel)
        {
            switch (channel.ToUpper())
            {
                case "IF100":
                    if (subscribedClientsIF100.Contains(clientInfo))
                    {
                        subscribedClientsIF100.Remove(clientInfo);

                        /*
                        if (!subscribedClientsSPS101.Contains(clientInfo))
                        {
                            allSubscribedClients.Remove(clientInfo);
                            UpdateSubscribedClientsList("All", allSubscribedClients, richTextBox_AllChannels);
                        }*/

                        UpdateSubscribedClientsList("IF100", subscribedClientsIF100, richTextBox_IF100);
                    }
                    break;
                case "SPS101":
                    if (subscribedClientsSPS101.Contains(clientInfo))
                    {
                        subscribedClientsSPS101.Remove(clientInfo);

                        /*
                        if (!subscribedClientsIF100.Contains(clientInfo))
                        {
                            allSubscribedClients.Remove(clientInfo);
                            UpdateSubscribedClientsList("All", allSubscribedClients, richTextBox_AllChannels);
                        }*/

                        UpdateSubscribedClientsList("SPS101", subscribedClientsSPS101, richTextBox_SPS101);
                    }
                    break;
                default:
                    Invoke(new Action(() =>
                    {
                        richTextBox_Actions.AppendText($"Invalid channel: {channel}\n");
                    }));
                    break;
            }
        }

        private void SendMessageToChannel(ClientInfo sender, string channel, string data)
        {
            switch (channel.ToUpper())
            {
                case "IF100":
                    foreach (var subscriber in subscribedClientsIF100)
                    {
                        if (subscriber != sender) // Don't send the message back to the sender
                        {
                            SendToClient(subscriber, $"Message from {sender.Username} on IF100: {data}");
                        }
                    }
                    break;
                case "SPS101":
                    foreach (var subscriber in subscribedClientsSPS101)
                    {
                        if (subscriber != sender) // Don't send the message back to the sender
                        {
                            SendToClient(subscriber, $"Message from {sender.Username} on SPS101: {data}");
                        }
                    }
                    break;
                default:
                    richTextBox_Actions.AppendText($"Invalid channel: {channel}\n");
                    break;
            }
        }
        private void SendToClient(ClientInfo clientInfo, string message)
        {
            try
            {
                NetworkStream clientStream = clientInfo.TcpClient.GetStream();
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                clientStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                richTextBox_Actions.AppendText($"Error sending message to {clientInfo.Username}: {ex.Message}\n");
            }
        }
        private void button_ServerStart_Click(object sender, EventArgs e)
        {
            string ip = textBox_IP.Text;
            int port;

            if (int.TryParse(textBox_Port.Text, out port))
            {
                StartServer(ip, port);
            }
            else
            {
                richTextBox_Actions.AppendText("Port number couldn't be parsed, try it again with a valid port value!\n");
            }
        }

    }
}