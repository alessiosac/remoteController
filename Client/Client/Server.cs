using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Media.Imaging;

namespace Client
{
    public class Server
    {
        public Socket sock { get; set; }
        public BitmapImage icon { get; set; }
        public string Name { get; set; }
        public Programs ProcessesActive { get; set; }

        public List<Process> List
        {
            get
            {
                if (ProcessesActive == null || ProcessesActive.getListInUse() == null) return null;
                else return ProcessesActive.getListInUse();
            }
        }

        public Server(Socket s, string str, string name)
        {
            icon = new BitmapImage(new Uri(@"/Client;component/images/" + str, UriKind.Relative));   //con @ ignora i caratteri di escape
            sock = s;
            Name = name;
            ProcessesActive = Programs.creaListPrograms();
        }

        public Server(string str, string name)      //useful for add Server
        {
            icon = new BitmapImage(new Uri(@"/Client;component/images/" + str, UriKind.Relative));   //con @ ignora i caratteri di escape
            Name = name;
        }        
        
    }
}
