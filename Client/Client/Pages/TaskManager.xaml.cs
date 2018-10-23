using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Client.Pages
{
    /// <summary>
    /// Logica di interazione per TaskManager.xaml
    /// </summary>   

    public partial class TaskManager : UserControl
    {

        private Server _tabAdd;
        private Server lastSelected;
        private Socket sock;
        //List<Server> serverList = new List<Server>();
        //con lista, se la modifico non notifica UI, allora serve fare Observable..
        public ObservableCollection<Server> serverList = new ObservableCollection<Server>();
        private int index = 0;      //useful for server image
        private string[] name_image = { "computer_black.png", "computer_blue.png", "computer_red.png", "computer_green.png", "computer_yellow.png" };
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        SocketHelper sockhelper;
        DispatcherTimer timer;
        private bool afterFirstInfo = false;
        private bool removingServer = false;

        public TaskManager()
        {
            //metto per vedere come mai si chiude
            try
            {
                InitializeComponent();
                LoadListItems();
                sockhelper = new SocketHelper();
                //aggiungo questo metodo all'evento, chiamato quando riceve pacchetto json
                sockhelper.Response += updateInfo;
                //aggiungo questo metodo all'evento, chiamato quando server si disconnette
                SocketHelper.End += removeServer;

                Loaded += MyLoadedRoutedEventHandler;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            /*ListView lv = (ListView)RecursiveVisualChildFinder<ListView>(TabControl1);
            if (lv.Name == "lvPrograms")
            {
                GridViewColumn column =lv.Child
            }
            ((System.ComponentModel.INotifyPropertyChanged)column).PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "ActualWidth")
                {
                     //do something here...
                 }
            };*/
        }

        void MyLoadedRoutedEventHandler(Object sender, RoutedEventArgs e)
        {
            Loaded -= MyLoadedRoutedEventHandler;            

            //timer for update % focus
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //int selectedIndex;
            foreach (var item in serverList)
            {
                if (item.Name != "")    //non agisco su server con +
                {
                    item.ProcessesActive.updateTime();
                    /*ListView lv = (ListView)RecursiveVisualChildFinder<ListView>(TabControl1);
                    if (lv.Name == "lvPrograms")
                    {
                        selectedIndex = lv.SelectedIndex;
                        // get selected tab
                        Server selectedTab = TabControl1.SelectedItem as Server;

                        lv.ItemsSource = selectedTab.List;
                        lv.SelectedIndex = selectedIndex;
                    }*/
                    //forse basta TabControl1.ItemSource= serverList;   
                }
            }
        }

        public void firstInfo(JsonObject jsObj, Socket sock)
        {
            Server result = serverList.FirstOrDefault(x => x.sock == sock);
            this.sock = sock;
            if (result != null)//se presente server ci lavoro, ma dovrebbe esserci sempre
            {
                //elaboro lista processi
                if (jsObj.list != null)
                {
                    foreach (var item in jsObj.list)
                    {
                        // Convert the string back to a byte array.
                        byte[] newBytes = Convert.FromBase64String(item.icon);
                        //save icon on file

                        BitmapImage img = ToImage(newBytes);

                        Process p = new Process(item.title, item.handle, img);
                        //ai processi attivi aggiungo questo 
                        result.ProcessesActive.addProgram(p);
                    }

                    //Trace Activity
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        reportActivity("\n List Update in : " + result.Name, Brushes.Black, FontWeights.Regular);
                    }
                    );
                }
                //elaboro nome app con focus
                if (!string.IsNullOrEmpty(jsObj.nameF))
                {
                    //può essere non ci sia ancora nella lista, ma è un caso gestito
                    result.ProcessesActive.changeFocus(jsObj.nameF, jsObj.handle);

                    //Trace Activity
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        reportActivity(" Focus Update in : " + result.Name, Brushes.Black, FontWeights.Regular);
                    }
                    );
                }

                //mi preparo a ricevere un prossimo pacchetto, chiamo receive asincorna
                sockhelper.Receive(sock);
            }
            else
            {
                //errore, server non acnora presente nella lista
                MessageBox.Show("server is not present in list", "Error in list", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void updateInfo(JsonObject jsObj, Socket sock)
        {
            // Finds first element 
            Server result = serverList.FirstOrDefault(x => x.sock == sock);

            if (result != null)//se presente server ci lavoro, ma dovrebbe esserci sempre
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    // clear tab control binding
                    TabControl1.DataContext = null;     //forse conviene togliere
                }
                );


                //elaboro lista processi
                if (jsObj.list != null)
                {
                    foreach (var item in jsObj.list)
                    {
                        //MessageBox.Show("trovato elemento nella lista " + item.title);
                        // Convert the string back to a byte array.
                        byte[] newBytes = Convert.FromBase64String(item.icon);
                        BitmapImage img = ToImage(newBytes);

                        Process p = new Process(item.title, item.handle, img);

                        if (result.ProcessesActive.removeProgram(p))    //non funziona però cmq non aggiunge
                        {
                            //correctly removed
                        }
                        else
                        {
                            //ai processi attivi aggiungo questo
                            result.ProcessesActive.addProgram(p);
                        }
                    }

                    //Trace Activity
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        reportActivity("\n List Update in : " + result.Name, Brushes.Black, FontWeights.Regular);
                    }
                    );

                }
                //elaboro nome app con focus
                if (!string.IsNullOrEmpty(jsObj.nameF))
                {
                    if (jsObj.nameF == "Error")
                    {
                        //Trace Activity
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                        {
                            reportActivity("\n Error, the application that should receive keystrokes isn't anymore in focus in server : " + result.Name + ". Suggest to retry", Brushes.Red, FontWeights.Bold);
                            Switcher.Switch(this);      //guardo se corretto
                        }
                        );

                    }
                    else
                    {
                        //può essere non ci sia ancora nella lista, ma è un caso gestito
                        //MessageBox.Show("applicazione che dovrebbe avere il focus" + jsObj.nameF + "," + jsObj.handle);
                        result.ProcessesActive.changeFocus(jsObj.nameF, jsObj.handle);

                        //Trace Activity
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                        {
                            reportActivity("\n Focus Update in : " + result.Name, Brushes.Black, FontWeights.Normal);
                        }
                        );

                    }
                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    // bind tab control
                    //TabControl1.DataContext = serverList;
                    //TabControl1.SelectedItem = result;

                    ListView lv = (ListView)RecursiveVisualChildFinder<ListView>(TabControl1);
                    if (lv.Name == "lvPrograms")
                    {
                        // get selected tab
                        Server selectedTab = TabControl1.SelectedItem as Server;

                        lv.ItemsSource = selectedTab.List;
                    }

                    TabControl1.DataContext= serverList;
                }
                );

                afterFirstInfo = true;
                /*string nomi = "La lista contiene";
                foreach (var item in serverList)
                {
                    foreach (var nomeproc in item.List)
                        nomi += nomeproc.Name + "\n";
                    nomi += "\n";
                }*/
                //MessageBox.Show(nomi);
            }
        }

        /*public bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                // Open file for reading
                System.IO.FileStream _FileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);  //verifico parametri passati
                // Writes a block of bytes to this stream using data from
                // a byte array.
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

                // close file stream
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                //Trace Activity
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    reportActivity("\n  Exception: " + _Exception.ToString(), Brushes.Black, FontWeights.Normal);
                }
                );
            }

            // error occured, return false
            return false;
        }
        */
        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze(); //serve assolutamente, se no parseXamlException
                return image;
            }
        }
        /*private TabItem AddTabItem()
        {
            int count = _tabItems.Count;

            // create new tab item
            TabItem tab = new TabItem();

            tab.Header = string.Format("Tab {0}", count);
            tab.Name = string.Format("tab{0}", count);
            tab.HeaderTemplate = tabDynamic.FindResource("TabHeader") as DataTemplate;

            // add controls to tab item, this case I added just a textbox
            TextBox txt = new TextBox();
            txt.Name = "txt";

            tab.Content = txt;

            // insert tab item right before the last (+) tab item
            _tabItems.Insert(count - 1, tab);

            return tab;
        }
        */
        //disconnessione dal server
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string tabName = (sender as Button).CommandParameter.ToString();

            var item = TabControl1.Items.Cast<Server>().Where(i => i.Name.Equals(tabName)).SingleOrDefault();

            Server tab = item as Server;

            if (tab != null)
            {
                if (MessageBox.Show(string.Format("Are you sure you want to disconnect from '{0}'?", tab.Name.ToString()),
                   "Disconnect From Server", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CommandObject coObj = new CommandObject();
                    coObj.Application = "Client-Closed";
                    coObj.Lista = null;
                    coObj.Tasti = 0;

                    removingServer = true;
                    SocketHelper.Send(tab.sock, coObj);

                }
               
                /*if (serverList.Count <= 2)
                {
                    if (MessageBox.Show(string.Format("Are you sure you want to disconnect from '{0}'?", tab.Name.ToString()),
                    "Disconnect From Server", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        // Release the socket.
                        SocketHelper.Disconnect(tab.sock);

                        timer.Stop();
                        timer = null;

                        Home pageforNewServer = new Home();
                        Switcher.Switch(pageforNewServer);
                    }

                    //torna alla pagina iniziale
                }
                else if (MessageBox.Show(string.Format("Are you sure you want to disconnect from '{0}'?", tab.Name.ToString()),
                    "Disconnect From Server", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {

                    // Release the socket.
                    SocketHelper.Disconnect(tab.sock);

                    // clear tab control binding
                    //TabControl1.DataContext = null;

                    TabControl1.SelectedIndex = 0;

                    serverList.Remove(tab);

                    // bind tab control
                    //TabControl1.DataContext = serverList;
                    //Trace Activity
                    IPEndPoint remoteIpEndPoint = tab.sock.RemoteEndPoint as IPEndPoint;
                    reportActivity("Client disconnected from : " + tab.Name + " , " + remoteIpEndPoint + "\n", Brushes.Blue, FontWeights.Regular);
                }*/
            }
        }

        private void removeServer(Socket s)
        {
            // Finds first element 
            Server tab = serverList.FirstOrDefault(x => x.sock == s);

            if (tab != null)
            {
                if (serverList.Count <= 2)
                {
                    if (removingServer)
                        removingServer = false;
                    else
                        MessageBox.Show(string.Format("'{0}' disconnected", tab.Name.ToString()), "Disconnect From Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    timer.Stop();
                    timer = null;
                    //torna alla pagina iniziale
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        Home pageforNewServer = new Home();
                        Switcher.Switch(pageforNewServer);
                    }
                    );
                }
                else
                {
                    if (removingServer)
                        removingServer = false;
                    else
                        MessageBox.Show(string.Format("'{0}' disconnected", tab.Name.ToString()), "Disconnect From Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                
                    //Trace Activity
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        // get selected tab
                        Server selectedTab = TabControl1.SelectedItem as Server;    //verifico che può dare errore, forse devo mettere tutto in begininvoke

                        // clear tab control binding
                        //TabControl1.DataContext = null;

                        // select previously selected tab. if that is removed then select first tab
                        if (selectedTab == null || selectedTab.Equals(tab) || selectedTab.Equals(_tabAdd))
                        {
                            selectedTab = serverList[0];
                        }
                        TabControl1.SelectedItem = selectedTab;

                        serverList.Remove(tab);

                        // bind tab control
                        //TabControl1.DataContext = serverList;

                        IPEndPoint remoteIpEndPoint = tab.sock.RemoteEndPoint as IPEndPoint;
                        reportActivity("Server : " + tab.Name + " , " + remoteIpEndPoint + " disconnected\n", Brushes.Blue, FontWeights.Regular);

                        removingServer = false;
                    }
                    );
                }
            }
        }

        public void setRemoving()
        {
            removingServer = true;
        }

        private void addServer(object sender, RoutedEventArgs e)
        {
            TabControl1.SelectedItem = lastSelected;
            Switcher.Switch(new Home(this));
        }

        private void tabDynamic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Server tab = TabControl1.SelectedItem as Server;
            if (tab == null) return;
            //MessageBox.Show(contatore + " , " + tab.Name);
           if (tab.Equals(_tabAdd))
           {
               TabControl1.SelectedItem = lastSelected;
               /*MessageBox.Show("sono il più 1");
               contatore++;
               TabControl1.SelectedItem = lastSelected;
               Switcher.Switch(new Home(this));*/

               // clear tab control binding
               /* TabControl1.DataContext = null;
                Server item = new Server(calculate_image(), "Server " + ++count);
                serverList.Insert(serverList.Count -1, item);   //pass the name of the image to view

                //TabControl1.ItemsSource = serverList;

                // select newly added tab item
                TabControl1.SelectedItem = item;

                // bind tab control
                TabControl1.DataContext = serverList;*/
           }
           else
           {
               //TabControl1.DataContext = serverList;
               if (afterFirstInfo && lastSelected!=tab)
               {
                   //MessageBox.Show("carico");
                   ListView lv = (ListView)RecursiveVisualChildFinder<ListView>(TabControl1);
                   if (lv.Name == "lvPrograms")
                   {
                       lv.ItemsSource = tab.List;
                   }
               }
               lastSelected = tab;
           }                     
        }

        public void reportActivity(string str, Brush color, FontWeight font)
        {
            TextRange rangeOfText1 = new TextRange(activityViewer.Document.ContentEnd, activityViewer.Document.ContentEnd);
            rangeOfText1.Text = str;
            rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, font);
            activityViewer.ScrollToEnd();
        }

        //obtain object from template
        private static DependencyObject RecursiveVisualChildFinder<T>(DependencyObject rootObject)
        {
            var child = VisualTreeHelper.GetChild(rootObject, 0);
            if (child == null) return null;

            return child.GetType() == typeof(T) ? child : RecursiveVisualChildFinder<T>(child);
        }

        //order  the list
        private void lvUsersColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListView lv = (ListView)RecursiveVisualChildFinder<ListView>(TabControl1);
            if (lv.Name == "lvPrograms")
            {
                GridViewColumnHeader column = (sender as GridViewColumnHeader);
                string sortBy = column.Tag.ToString();
                if (listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                    lv.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                listViewSortCol = column;
                listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
                lv.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        private void LoadListItems()
        {
            _tabAdd = new Server("add.png", "");
            serverList.Add(_tabAdd);

            TabControl1.ItemsSource = serverList;
            //this.listBox1.DataContext = this;
            //TabControl1.SelectedIndex = 0;          
        }

        // for this code image needs to be a project resource
        private BitmapImage LoadImage(string filename)
        {
            return new BitmapImage(new Uri(@"images/icons/" + filename));
            //return new BitmapImage(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+"/icons/" + filename));
        }

        private void ToSendClick(object sender, MouseButtonEventArgs e)
        {
            Process item = (sender as ListView).SelectedItem as Process;
            if (item != null)
            {
                ToSendHandler(item);
            }
        }

        private void ToSendHandler(Process p)
        {
            if (!p.InFocus)
            {
                MessageBox.Show("Attention please , you could send keystrokes just to an application with focus", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                SendKey pageforSendKey = new SendKey(p.Name, (serverList.Count - 1), this, (TabControl1.SelectedItem as Server));      
                Switcher.Switch(pageforSendKey);
            }    
        }

        public void addServer(Socket s)
        {
            IPEndPoint remoteIpEndPoint = s.RemoteEndPoint as IPEndPoint;
            Server item = new Server(s, calculate_image(), "Server " + remoteIpEndPoint.Address);
            serverList.Insert(serverList.Count - 1, item);   //pass the name of the image to view
			//aggiungo questo, devo provare
            //contatore = 0;
            TabControl1.SelectedItem = item;
            lastSelected = item;
            /*Home initialPage = new Home();
             Switcher.Switch(initialPage);*/
        }

        private string calculate_image()
        {
            index = (index + 1) % name_image.Length;
            return name_image[index];
        }

        private void ToSendWithEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Process item = (sender as ListBox).SelectedItem as Process;
                if (item != null)
                {
                    ToSendHandler(item);
                }
            }
        }
       
    }

    public class SortAdorner : Adorner
    {
        private static Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
        {
            this.Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform
                    (
                            AdornedElement.RenderSize.Width - 15,
                            (AdornedElement.RenderSize.Height - 5) / 2
                    );
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.Direction == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(Brushes.Black, null, geometry);

            drawingContext.Pop();
        }
    }
}
