using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Messaging_App.ViewModels;

namespace Messaging_App
{
    public class Binder : INotifyPropertyChanged
    {
        
        static Binder[] instance = new Binder[100];
        static readonly object padlock = new object();

        public Binder()
        {
            try
            {
                Messages = App.ViewModel.Items[ChatPage.index].Chats;
            }
            catch(Exception)
            {
                Messages = new ObservableCollection<Message>();
            }
        }

        public static Binder Instance
        {
            get
            {
                lock (padlock)
                {
                    
                    if (instance[ChatPage.index] == null)
                    {
                        if (App.ViewModel.Items[ChatPage.index].Chats==null)
                        {
                            App.ViewModel.Items[ChatPage.index].Chats = new ObservableCollection<Message>();
                        }
                        instance[ChatPage.index] = new Binder();
                    }
                    return instance[ChatPage.index];
                    
                    }
            }
        }

        private ObservableCollection<Message> messages;
        public ObservableCollection<Message> Messages
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
                    NotifyPropertyChanged("Messages");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => { PropertyChanged(this, new PropertyChangedEventArgs(info)); });
            }
        }
    }
}
