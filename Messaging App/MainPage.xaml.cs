using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Messaging_App.Resources;
using Messaging_App.ViewModels;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.UserData;
using Microsoft.Phone.Media;
using System.Windows.Media.Imaging;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Xna.Framework.GamerServices;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices;
using Messaging_App.AzureModel;
using System.Threading.Tasks;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;
using Windows.Networking.Connectivity;

namespace Messaging_App
{
    public partial class MainPage : PhoneApplicationPage
    {
        //private List<Person> _people = new List<Person>();
        private ChatData lastSelected=null;
        public MainPage()
        {
            DataContext = App.ViewModel;
            InitializeComponent();
            AddrBook.ItemsSource = App.ViewModel.DataSource;
            //favAddrbook.ItemsSource = App.ViewModel.favDataSource;
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(ChangeDetected);   
        }


        
        

        void ChangeDetected(object sender, NetworkNotificationEventArgs e)
        {
            string change = string.Empty;
            switch (e.NotificationType)
            {
                case NetworkNotificationType.InterfaceConnected:
                    change = "Connected to ";
                    ChatModel.AzureConnected = true;
                    break;
                case NetworkNotificationType.InterfaceDisconnected:
                    change = "Disconnected from ";
                    ChatModel.AzureConnected = false;
                    break;
                case NetworkNotificationType.CharacteristicUpdate:
                    change = "Characteristics changed for ";
                    break;
                default:
                    change = "Unknown change with ";
                    break;


            }

            string changeInformation = String.Format(" {0} {1} {2} ({3})",
                        DateTime.Now.ToString(), change, e.NetworkInterface.InterfaceName,
                        e.NetworkInterface.InterfaceType.ToString());
            Guide.BeginShowMessageBox("Network", changeInformation, new string[] { "Yes", "NO" }, 0, MessageBoxIcon.Warning, null, null);

        }

        //new - contextmenu for inviting a selected contact through SMS
        //to be implemented later
        //private void Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    //Creating a menu
        //    //Adding items to that menu
        //    MenuItem item1 = new MenuItem();
        //    ContextMenu Mymenu = new ContextMenu();
        //    item1.Header = "Invite Through SMS";
            
        //    item1.Click += new RoutedEventHandler(send_message_Click);

        //    Mymenu.Items.Add(item1);

        //    // Locate context menu in the right spot 
        //    Mymenu.VerticalOffset = e.GetPosition(LayoutRoot).Y + 50;

        //    //Displaying that menu 
        //    Mymenu.IsOpen = true;
        //}

        //new - message sending part
        
        

        // Load data for the ViewModel Items
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            //if (e.NavigationMode == NavigationMode.New) // only handle initial launch 
                //await InitLocalStoreAsync();
            if (!App.ViewModel.IsDataLoaded)
            {
                await App.ViewModel.LoadData();
            }
            
            
        }
        private async Task InitLocalStoreAsync()
        {
            if (!App.MobileService.SyncContext.IsInitialized)
            {
                var store = new MobileServiceSQLiteStore("localstore.db");
                store.DefineTable<SendData>();
                await App.MobileService.SyncContext.InitializeAsync(store);
            }
            if (ChatModel.AzureConnected)
                App.ViewModel.Sync();
        }

        

        
        /*private void MainLongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainLongListSelector.SelectedItem == null)
                return;
            
            ChatData selected= MainLongListSelector.SelectedItem as ChatData;
           
           // Navigate to the new page
            NavigationService.Navigate(new Uri("/ChatPage.xaml?id=" + selected.ID, UriKind.Relative));

            // Reset selected item to null (no selection)
            MainLongListSelector.SelectedItem = null;
        }
        private void MainLongListSelector_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MainLongListSelector_SelectionChanged(null, null);
        }*/
        
        //new - navigates to contacts page when application bar is clicked
        private void addButton_Click(object sender, EventArgs e)
        {
            myPivot.SelectedItem = myFav;
        }
        
        private void DeletePart2()
        {

            App.ViewModel.RemoveChat(lastSelected);
           
            
      }
        public async void Update()
        {
            await App.ViewModel.UpdateChats();
        }

        private void OnMessageBoxAction(IAsyncResult ar)
        {
            int? selectedButton = Guide.EndShowMessageBox(ar);
            switch (selectedButton)
            {
                case 0:
                    Deployment.Current.Dispatcher.BeginInvoke(() => DeletePart2());
                    Update();
                    break;

                case 1:
                    break;

                default:
                    break;
            }
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            lastSelected = (sender as MenuItem).DataContext as ChatData;
            Guide.BeginShowMessageBox("Please Confirm", "Are you sure you want to delete this chat?", new string[] { "Yes", "NO" }, 0, MessageBoxIcon.Warning, new AsyncCallback(OnMessageBoxAction), null);
        }

       // new - opens up the chat page when a contact is clicked
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var contact = myListBox.SelectedItem as Favourites;
            ChatData newChat = App.ViewModel.CreateNewChat(contact);
            NavigationService.Navigate(new Uri("/ChatPage.xaml?id=" + newChat.ID, UriKind.Relative));
        }

        //new - Makes application bar available only on chat page to add contacts
        private void Pivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
        }

        private void MainLongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainLongListSelector.SelectedItem == null)
                return;

            ChatData selected = MainLongListSelector.SelectedItem as ChatData;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/ChatPage.xaml?id=" + selected.ID, UriKind.Relative));
            
            // Reset selected item to null (no selection)
            MainLongListSelector.SelectedItem = null;
        }
        private void MainLongListSelector_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MainLongListSelector_SelectionChanged(null, null);
        }

        private void AddrBook_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var contact = AddrBook.SelectedItem as AddressBook;
            ChatData newChat = App.ViewModel.CreateNewChat(contact,0);
            NavigationService.Navigate(new Uri("/ChatPage.xaml?id=" + newChat.ID, UriKind.Relative));
        }

        private void myPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem pivot = null;
            pivot = (PivotItem)(sender as Pivot).SelectedItem;
            switch(pivot.Header.ToString())
            {
                case "contacts" :
                    this.ApplicationBar = this.Resources["secondBar"] as ApplicationBar;
                    break;

                case "favourites":
                    this.ApplicationBar = this.Resources["thirdBar"] as ApplicationBar;
                    break;

                case "chats":
                    this.ApplicationBar = this.Resources["firstBar"] as ApplicationBar;
                    break;

                default:
                    break;
            }
        }

        private void fav_refresh(object sender, EventArgs e)
        {
            App.ViewModel.refreshfav();
        }

        private void contacts_refresh(object sender, EventArgs e)
        {
            App.ViewModel.LoadContacts(string.Empty);
        }

       
    }

    //new - ContactPictureConverter for getting the image of clicked contact
    public class ContactPictureConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Contact c = value as Contact;
            if (c == null) return null;
            
            System.IO.Stream imageStream = c.GetPicture();
            if (null != imageStream)
            {
                return Microsoft.Phone.PictureDecoder.DecodeJpeg(imageStream);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}