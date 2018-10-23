using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Client
{
    public class Process : INotifyPropertyChanged
    {
        private float _percTimeFocus;
        private bool _inFocus;
        public long TimeInFocus { get; set; }
        public string Name { get; set; }
        public int Handle { get; set; }
        public bool InFocus
        {
            get { return _inFocus; }
            set
            {
                if (value != _inFocus)
                {
                    _inFocus = value;
                    // Call OnPropertyChanged whenever the property is updated
                    OnPropertyChanged("InFocus");
                }
            }
        }
        public BitmapImage Icon { get; set; }
        public bool InUse { get; set; }     //I need InUse becouse I want the percentage from the moment when I connected
        public float PercentageTimeInFocus
        {
            get { return _percTimeFocus; }
            set
            {
                if (value != _percTimeFocus)
                {
                    _percTimeFocus = value;
                    // Call OnPropertyChanged whenever the property is updated
                    OnPropertyChanged("PercentageTimeInFocus");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public Process(String s, int handle, BitmapImage img = null, bool focus = false)
        {
            TimeInFocus = 0;
            _percTimeFocus = 0.0F;
            Name = s;
            Handle = handle;
            _inFocus = focus;
            Icon = img;
            InUse = true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Process item = obj as Process;

            return ((item.Name == this.Name) && (item.Handle == this.Handle));
        }

        public override int GetHashCode()
        {
            //same meaning of Equal
            int hash = 13;
            hash = Name.GetHashCode();
            hash = (hash * 7) + Handle.GetHashCode();
            return hash;
        }

        public static bool operator ==(Process item1, Process item2)
        {
            if (object.ReferenceEquals(item1, item2)) { return true; }
            if ((object)item1 == null || (object)item2 == null) { return false; }
            return ((item1.Name == item2.Name) && (item1.Handle == item2.Handle));
        }

        public static bool operator !=(Process item1, Process item2)
        {
            return !(item1 == item2);
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
