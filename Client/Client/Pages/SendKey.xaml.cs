using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.Pages
{
    /// <summary>
    /// Logica di interazione per SendKey.xaml
    /// </summary>
    public partial class SendKey : UserControl
    {
        int numServer;
        TaskManager pageforNewConnection;
        Server actualServer;
        CommandObject commObj;
        List<Input> commandList;

        public SendKey(String name, int numHost, TaskManager tm, Server server)
        {
            InitializeComponent();
            nameApp.Text = name;
            numServer = numHost;
            actualServer = server;
            commObj = new CommandObject();
            commObj.Application = name;
            if (numServer > 1)
            {
                CheckPanel.Visibility = Visibility.Visible;
                panelForKey.Margin = new Thickness(0, 50, 0, 0);
            }
            pageforNewConnection = tm;
            commandList = new List<Input>();
            commandsField.Focus();
        }

        private void Return(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(pageforNewConnection);
        }

        private void SendKeys(object sender, RoutedEventArgs e)
        {
            commObj.Lista = commandList;
            commObj.Tasti = commandList.Count;
           
            try
            {
                if (checkForAllServer.IsChecked == true)
                {
                    //send to all server with this opened programs
                    foreach (var value in pageforNewConnection.serverList)
                    {
                        if(value.List.Exists(item => item.Name == commObj.Application))
                            SocketHelper.Send(value.sock, commObj);
                        //Trace Activity
                        pageforNewConnection.reportActivity("\n Sent keystrtokes: "+ commandsField.Text +", to : " + value.Name, Brushes.Black, FontWeights.Normal);
                    }
                }
                else
                {
                    SocketHelper.Send(actualServer.sock, commObj);
                    //Trace Activity
                    pageforNewConnection.reportActivity("\n Sent keystrtokes: " + commandsField.Text + ", to : " + actualServer.Name, Brushes.Black, FontWeights.Normal);
                }

                commandsField.Text = "";
                commandList.Clear();

                //return to prevoius page
                Switcher.Switch(pageforNewConnection);
            }
            catch (NullReferenceException)
            {
                //ignore
            }
            catch(Exception ex)
            {
                messageError.Content = ex.ToString();
                messageError.Visibility = Visibility.Visible;
            }
        }
        
        private void commandsField_KeyDown(object sender, KeyEventArgs e)
        {
            // The text box grabs all input.
            e.Handled = true;

            // Fetch the actual shortcut key.
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);            
            
            commandList.Add(getInfo(e, key));
            if(commandsField.Text=="")
                commandsField.Text += key.ToString();
            else
                commandsField.Text += " + "+key.ToString();
            commandsField.CaretIndex = commandsField.Text.Length;
        }

        private Input getInfo(KeyEventArgs e, Key key)
        {
            Input input = new Input();
            input.vKey = KeyInterop.VirtualKeyFromKey(key);
            FieldInfo field = e.GetType().GetField
            (
               "_scanCode",
               BindingFlags.NonPublic |
               BindingFlags.Instance
            );

            input.scanCode = (Int32)field.GetValue(e);

            return input;
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            commandsField.Text = "";
            commandList.Clear();
            commandsField.Focus();
        }
    }
}
