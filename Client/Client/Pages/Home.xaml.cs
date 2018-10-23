using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.Pages
{
    /// <summary>
    /// Logica di interazione per Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        Socket senderSock;
        TaskManager pageforNewConnection;
        Cursor saveCursor;

        public Home()
        {
            InitializeComponent();
            pageforNewConnection = new TaskManager();
            Return_Button.Visibility = Visibility.Hidden;
        }

        public Home(TaskManager ts)
        {
            pageforNewConnection = ts;
            InitializeComponent();
        }

        private void writeError(string str)
        {
            textBoxError.Text = str;
            error.Visibility = Visibility.Visible;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            saveCursor = this.Cursor;
            Mouse.OverrideCursor = Cursors.Wait;

            Connect_Button.IsEnabled = false; 

            try
            {
                senderSock = SocketHelper.Connect(textBox.Text);
                IPEndPoint remoteIpEndPoint = senderSock.RemoteEndPoint as IPEndPoint;
                pageforNewConnection.reportActivity("\n Client connected to : " + remoteIpEndPoint, Brushes.Blue, FontWeights.Regular);
                //faccio prima anche se potrei fare dopo, cosi mi porto avanti prima di fare la recv, ma forse non va bene
                pageforNewConnection.addServer(senderSock);

                JsonObject jsObj = SocketHelper.ReceiveImmediatly<JsonObject>(senderSock);
                pageforNewConnection.firstInfo(jsObj, senderSock);

                Switcher.Switch(pageforNewConnection);      //forse meglio spostare, decido quando cambiare pagina

            }
            catch (Exception ex)
            {
                writeError(ex.Message);
            }
            finally
            {
                Connect_Button.IsEnabled = true;
                Mouse.OverrideCursor = saveCursor;
            }

        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(pageforNewConnection);
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Equals("IP address or Hostname"))
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Equals(""))
            {
                textBox.Text = "IP address or Hostname";
                textBox.Foreground = Brushes.Gray;
            }
        }
    }
}
