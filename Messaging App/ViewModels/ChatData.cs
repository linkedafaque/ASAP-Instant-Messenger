using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Messaging_App.ViewModels
{
    public class ChatData : INotifyPropertyChanged
    {

        private string _id;
        /// <summary>
        /// Sample ViewModel property; this property is used to identify the object.
        /// </summary>
        /// <returns></returns>
        public string ID
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
                    NotifyPropertyChanged("ID");
                }
            }
        }

        private ObservableCollection<Message> messages;
        public ObservableCollection<Message> Chats
        {
            get
            {
                return messages;
            }
            set
            {
                if (messages != value)
                {
                    messages = value;
                    if (Chats != null)
                    {
                        if (Chats.Count != 0)
                        {
                            FirstLine = Chats[Chats.Count - 1].Text;
                        }
                    }
                    
                    NotifyPropertyChanged("Chats");
                }
            }
        }
        
        private string _contactName;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string ContactName
        {
            get
            {
                return _contactName;
            }
            set
            {
                if (value != _contactName)
                {
                    _contactName = value;
                    NotifyPropertyChanged("ContactName");
                }
            }
        }

        private string _firstLine;

        public string FirstLine
        {
            get { return _firstLine; }
            set
            {
                if (value != _firstLine)
                {
                    _firstLine = value;
                    NotifyPropertyChanged("FirstLine");
                }
            }
        }
        private string _phoneNumber;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string PhoneNumber
        {
            get
            {
                return _phoneNumber;
            }
            set
            {
                if (value != _phoneNumber)
                {
                    _phoneNumber = value;
                    NotifyPropertyChanged("PhoneNumber");
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