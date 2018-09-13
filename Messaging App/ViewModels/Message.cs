using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging_App.ViewModels
{
    public class Message : INotifyPropertyChanged
    {
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }
        private string _sender;
        public string Sender
        {
            get
            {
                return _sender;
            }
            set
            {
                if (value != _sender)
                {
                    _sender = value;
                    NotifyPropertyChanged("Sender");
                }
            }
        }
        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (value != _text)
                {
                    _text = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }
        private string _sendtime;
        public string SendingTime
        {
            get
            {
                return _sendtime;
            }
            set
            {
                if (value != _sendtime)
                {
                    _sendtime = value;
                    NotifyPropertyChanged("SendingTime");
                }
            }
        }
        private bool _secret;
        public bool Secret
        {
            get
            {
                return _secret;
            }
            set
            {
                if (value != _secret)
                {
                    _secret = value;
                    NotifyPropertyChanged("Secret");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
