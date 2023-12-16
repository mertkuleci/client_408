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
            public string Username { get; set; }

            public bool isConnected { get; set; }// Could be set after its initialization.

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
            this.FormClosing += Form1_FormClosing;
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
                    richTextBox_Actions.AppendText($"Server starting on {ip}:{port}\n");
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
            if (richTextBox_Actions.InvokeRequired && !richTextBox_Actions.IsDisposed)
            {
                richTextBox_Actions.Invoke(new Action<string>(UpdateRichTextBox), message);
            }
            else if (!richTextBox_Actions.IsDisposed)
            {
                richTextBox_Actions.AppendText(message);
            }

        }

        private void ListenForClients()
        {
            // If the user enters invalid numerical values in the field,
            // this try & catch blocks handles it and redirects the user to
            // fix the input field values.

            try
            {
                tcpListener.Start();

                while (true)
                {

                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ClientInfo clientInfo = new ClientInfo(tcpClient);
                    clients.Add(clientInfo);

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(clientInfo);
                }
            }
            catch (Exception ex)
            {
                UpdateRichTextBox($"Server could not start properly. Something went wrong: {ex.Message}\n");
                button_ServerStart.Invoke(new Action(() => button_ServerStart.Enabled = true));
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

                if (clientInfo != null)
                    clients.Remove(clientInfo);
                UpdateSubscribedClientsList("All", clients, richTextBox_AllChannels);

                if (clientInfo != null && subscribedClientsIF100.Contains(clientInfo))
                {
                    subscribedClientsIF100.Remove(clientInfo);
                    UpdateSubscribedClientsList("IF100", subscribedClientsIF100, richTextBox_IF100);
                    UpdateRichTextBox($"Client {clientInfo.Username} unsubscribed from the IF100 channel due to the connection termination\n");

                }
                if (clientInfo != null && subscribedClientsSPS101.Contains(clientInfo))
                {
                    subscribedClientsSPS101.Remove(clientInfo);
                    UpdateSubscribedClientsList("SPS101", subscribedClientsSPS101, richTextBox_SPS101);
                    UpdateRichTextBox($"Client {clientInfo.Username} unsubscribed from the SPS101 channel due to the connection termination\n");
                }

            }
        }

        private void HandleClientConnect(ClientInfo clientInfo, string username)
        {
            // Check if the username is already in use
            if (IsUsernameUnique(username))
            {
                // Username is unique, store it in the ClientInfo instance

                if (IsUsernameValid(username))
                {
                    clientInfo.Username = username;


                    clientInfo.isConnected = true;

                    // Print the username in the actions richtext box after it is parsed properly.
                    UpdateRichTextBox($"Client connected: {clientInfo.Username}\n");
                    UpdateSubscribedClientsList("All", clients, richTextBox_AllChannels);
                    SendToClient($"CONNECTED", clientInfo, $"{username} connected to the server \n");
                }
                else
                {

                    UpdateRichTextBox($"A client tried to access with a username that is not valid. Request is rejected\n");
                    SendToClient("NOTUNIQUE", clientInfo, "The username you are using is not valid. It cannot contain a charecter other than letter, number or dot. Try it again with a different username.\n");
                    DisconnectClient(clientInfo);
                }



            }
            else
            {
                // Username is already in use, disconnect the client
                UpdateRichTextBox($"A client tried to access with a username that is already exist. Request is rejected\n");
                SendToClient("NOTUNIQUE", clientInfo, "The username you are using is already claimed. Try it again with a different username.\n");
                DisconnectClient(clientInfo);
            }
        }
        private bool IsUsernameUnique(string username)
        {
            // Check if the username is unique among connected clients
            return !clients.Any(client => client.Username != null && client.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsUsernameValid(string username)
        {
            // Check if the username is not empty
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            // Check if the username contains only letters, numbers, or "."
            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '.')
                {
                    return false;
                }
            }

            // If all checks pass, the username is valid
            return true;
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
            // Assuming messages are formatted as "ACTION|CHANNEL|dummy"
            string[] parts = message.Split('|');
            string data = " ", username;

            int firstIndex = message.IndexOf('|');
            int secondIndex = message.IndexOf('|', firstIndex + 1);
            int lastIndex = message.LastIndexOf('|');


            if (parts[2] != "dummy")
            {
                data = message.Substring(secondIndex + 1, lastIndex - secondIndex - 1);
            }

            string action = parts[0].ToUpper();
            string channel = parts[1];

            switch (action)
            {
                case "CONNECT":
                    username = message.Substring(lastIndex + 1);
                    HandleClientConnect(clientInfo, username);
                    break;
                case "DISCONNECT":
                    username = message.Substring(lastIndex + 1);
                    clientInfo.isConnected = false;
                    SendToClient(action, clientInfo, $"Client disconnected: {clientInfo.Username}\n");

                    clients.Remove(clientInfo);
                    if (subscribedClientsIF100.Contains(clientInfo))
                    {
                        subscribedClientsIF100.Remove(clientInfo);
                        UpdateRichTextBox($"Client {clientInfo.Username} unsubscribed from the IF100 channel.\n");
                    }
                    if (subscribedClientsSPS101.Contains(clientInfo))
                    {
                        subscribedClientsSPS101.Remove(clientInfo);
                        UpdateRichTextBox($"Client {clientInfo.Username} unsubscribed from the SPS101 channel.\n");
                    }
                    UpdateRichTextBox($"Client disconnected: {clientInfo.Username}\n");

                    UpdateSubscribedClientsList("All", clients, richTextBox_AllChannels);
                    UpdateSubscribedClientsList("IF100", subscribedClientsIF100, richTextBox_IF100);
                    UpdateSubscribedClientsList("SPS101", subscribedClientsSPS101, richTextBox_SPS101);

                    DisconnectClient(clientInfo);
                    break;
                case "SUBSCRIBE":
                    if (clientInfo.isConnected == true)
                    {
                        SubscribeToChannel(clientInfo, channel);
                        break;
                    }
                    break;

                case "UNSUBSCRIBE":
                    if (clientInfo.isConnected == true)
                    {
                        UnsubscribeFromChannel(clientInfo, channel);
                        break;
                    }
                    break;
                case "SEND":
                    if (channel == "IF100")
                    {
                        if (clientInfo.isConnected == true)
                        {
                            if (subscribedClientsIF100.Contains(clientInfo))
                            {
                                data = message.Substring(secondIndex + 1, lastIndex - secondIndex - 1);
                                SendMessageToChannel(clientInfo, channel, data);
                                break;
                            }
                            else
                            {
                                SendToClient("IF100unsub", clientInfo, "Please subscribe to send message!");

                            }
                        }
                        else
                        {
                            SendToClient("IF100uncon", clientInfo, "Please connect to send message!");
                        }
                    }
                    else
                    {
                        if (clientInfo.isConnected == true)
                        {
                            if (subscribedClientsSPS101.Contains(clientInfo))
                            {
                                data = message.Substring(secondIndex + 1, lastIndex - secondIndex - 1);
                                SendMessageToChannel(clientInfo, channel, data);
                                break;
                            }
                            else
                            {
                                SendToClient("SPS101unsub", clientInfo, "Please subscribe to send message!");

                            }
                        }
                        else
                        {
                            SendToClient("SPS101uncon", clientInfo, "Please connect to send message!");
                        }
                    }
                    break;
            }

        }

        private List<ClientInfo> subscribedClientsIF100 = new List<ClientInfo>();
        private List<ClientInfo> subscribedClientsSPS101 = new List<ClientInfo>();

        private void UpdateSubscribedClientsList(string channel, List<ClientInfo> subscribedClients, RichTextBox richTextBox)
        {
            Invoke(new Action(() =>
            {
                richTextBox.Clear();

                foreach (var client in subscribedClients)
                {
                    richTextBox.AppendText($"{client.Username}\n");
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
                        SendToClient(channel, clientInfo, "SubscribedtoIF100");
                        UpdateSubscribedClientsList("IF100", subscribedClientsIF100, richTextBox_IF100);
                        UpdateRichTextBox($"Client {clientInfo.Username} subscribed to the IF100 channel.\n");
                    }
                    break;
                case "SPS101":
                    if (!subscribedClientsSPS101.Contains(clientInfo))
                    {
                        subscribedClientsSPS101.Add(clientInfo);
                        SendToClient(channel, clientInfo, "SubscribedtoSPS101");
                        UpdateSubscribedClientsList("SPS101", subscribedClientsSPS101, richTextBox_SPS101);
                        UpdateRichTextBox($"Client {clientInfo.Username} subscribed to the SPS101 channel.\n");
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
                        SendToClient(channel, clientInfo, "UnsubscribedfromIF100");
                        UpdateSubscribedClientsList("IF100", subscribedClientsIF100, richTextBox_IF100);
                        UpdateRichTextBox($"Client {clientInfo.Username} unsubscribed from the IF100 channel.\n");
                    }
                    break;
                case "SPS101":
                    if (subscribedClientsSPS101.Contains(clientInfo))
                    {
                        subscribedClientsSPS101.Remove(clientInfo);
                        SendToClient(channel, clientInfo, "UnsubscribedfromSPS101");
                        UpdateSubscribedClientsList("SPS101", subscribedClientsSPS101, richTextBox_SPS101);
                        UpdateRichTextBox($"Client {clientInfo.Username} unsubscribed from the SPS101 channel.\n");
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
            switch (channel)
            {
                case "IF100":
                    foreach (var subscriber in subscribedClientsIF100)
                    {
                        SendToClient(channel, subscriber, $"{sender.Username}: {data} \n");
                    }
                    UpdateRichTextBox($"({channel}) {sender.Username}: {data}\n");
                    break;
                case "SPS101":
                    foreach (var subscriber in subscribedClientsSPS101)
                    {
                        SendToClient(channel, subscriber, $"{sender.Username}: {data} \n");
                    }
                    UpdateRichTextBox($"({channel}) {sender.Username}: {data}\n");
                    break;
                default:
                    richTextBox_Actions.AppendText($"Invalid channel: {channel}\n");
                    break;
            }
        }
        private void SendToClient(string channel, ClientInfo clientInfo, string message)
        {
            try
            {
                message = channel + "|" + message;
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Terminate connections and close sockets for all connected clients
            foreach (var clientInfo in clients)
            {
                SendToClient("DISCONNECT", clientInfo, "dummy");
                DisconnectClient(clientInfo);
            }

            // Close the listener and clear the clients list
            if (tcpListener != null)
            {
                tcpListener.Stop();
                clients.Clear();
            }

            Environment.Exit(0);
        }
    }
}