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
using Coding4Fun.Toolkit.Controls;
using Messaging_App.AzureModel;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace Messaging_App
{

    public partial class ChatPage : PhoneApplicationPage
    {
        public static int index = 0;
        static int tempid = 0;
        public static string mycontact = "9958506118";

        private IMobileServiceSyncTable<SendData> chatsTable = App.MobileService.GetSyncTable<SendData>();
        // Constructor
        Message sourceObject;
        public ChatPage()
        {



            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }



        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                string selectedIndex = "";
                if (NavigationContext.QueryString.TryGetValue("id", out selectedIndex))
                {
                    index = int.Parse(selectedIndex);
                    DataContext = App.ViewModel.Items[index];
                    InitializeComponent();
                }
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtMessage.Text))
            {
                Message message = new Message()
                {
                    Id = tempid.ToString(),
                    Sender = "Me",
                    Text = txtMessage.Text,
                    SendingTime = DateTime.Now.ToString()
                };
                tempid++;
                SendData toSend = new SendData();
                {
                    toSend.Id = message.Id;
                    toSend.From = mycontact;
                    toSend.Read = false;
                    toSend.Time = DateTime.Now.ToString();
                    toSend.Message = txtMessage.Text;
                    toSend.To = App.ViewModel.Items[ChatPage.index].PhoneNumber;
                    toSend.Secret = false;
                }
                //if (ChatModel.AzureConnected)
                //{
                //    bool check = await App.ViewModel.ConnectToWindowsAzure(toSend.To);
                //    if (!check)
                //        Guide.BeginShowMessageBox(
                //                "Azure Error",
                //                "The user does not have the app please consider sending him/her an invite",
                //                new string[] { "OK" },
                //                0,
                //                MessageBoxIcon.Error,
                //                null, null);

                //else
                //{
                Binder.Instance.Messages.Add(message);
                App.ViewModel.Items[index].FirstLine = txtMessage.Text;
                txtMessage.Text = string.Empty;

                await chatsTable.InsertAsync(toSend);
                await App.ViewModel.UpdateChats();
                if (ChatModel.AzureConnected)
                    App.ViewModel.Sync();

                //}
            }
            //}
            Dispatcher.BeginInvoke(() =>
            {
                if (ChatList.Items.Count > 0)
                {
                    var Selecteditem = ChatList.Items[ChatList.Items.Count - 1];

                    //ChatList.ScrollIntoView(Selecteditem);
                    //ChatList.UpdateLayout();
                }
            });
        }

        private async void ChatList_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (ChatModel.AzureConnected)
            {
                sourceObject = ((FrameworkElement)sender).DataContext as Message;
                string sec_msg = await App.ViewModel.SecretMsg_retrieve(sourceObject.Id);

                if (String.IsNullOrEmpty(sec_msg) == false)
                {
                    Guide.BeginShowMessageBox(
                           "Secret Message",
                           sec_msg,
                           new string[] { "Read", "OK" }, 0,
                           MessageBoxIcon.Alert, new AsyncCallback(OnMessageBoxAction), null);
                }

            }
        }
        private void OnMessageBoxAction(IAsyncResult ar)
        {
            int? selectedButton = Guide.EndShowMessageBox(ar);
            switch (selectedButton)
            {
                case 0:
                    Deployment.Current.Dispatcher.BeginInvoke(() => deletesecret(sourceObject.Id));

                    break;

                case 1:
                    break;

                default:
                    break;
            }
        }
        private void deletesecret(string s)
        {
            App.ViewModel.deleteSecretAzure(s);
        }
        private async void sendButton_Click_1(object sender, RoutedEventArgs e)
        {
            bool found = false;
            if (String.IsNullOrWhiteSpace(txtCourseDetails.Text) == false
                && String.IsNullOrEmpty(txtCourseDetails.Text) == false
                && App.ViewModel.Items[index].Chats.Count > 0)
            {
                SendData toSend = new SendData();
                int lastmsgindex = App.ViewModel.Items[index].Chats.Count;
                int i;
                for (i = lastmsgindex - 1; i >= 0; i--)
                {
                    if (App.ViewModel.Items[index].Chats[i].Secret != true &&
                        App.ViewModel.Items[index].Chats[i].Sender == "Me")
                    {
                       
                        toSend.Id = App.ViewModel.Items[index].Chats[i].Id;
                        toSend.RowKey = App.ViewModel.Items[index].Chats[i].Id;
                        toSend.From = mycontact;
                        toSend.Read = false;
                        toSend.Secret = true;
                        toSend.Time = DateTime.Now.ToString();
                        toSend.Message = txtCourseDetails.Text;
                        toSend.To = App.ViewModel.Items[ChatPage.index].PhoneNumber;
                        found = true;
                        App.ViewModel.Items[index].Chats[i].Secret = true;
                        break;
                    }
                }
                if (found == true)
                {
                    await chatsTable.UpdateAsync(toSend);
                    await App.ViewModel.UpdateChats();
                    if (ChatModel.AzureConnected)
                        App.ViewModel.Sync();
                }
                else
                {
                    Guide.BeginShowMessageBox(
                       "Unable to Send Secret",
                       "Please write another message to embed the secret behind",
                       new string[] { "OK" },
                       0,
                       MessageBoxIcon.Error,
                       null, null);
                }
                popupAddCourse.IsOpen = false;
            }
        }

        private void cancelButton_Click_1(object sender, RoutedEventArgs e)
        {
            txtCourseDetails.Text = "";
            popupAddCourse.IsOpen = false;
        }

        private void ApplicationBarSecretbutton_Click_1(object sender, EventArgs e)
        {
            UpdateTextBoxBinding();
            popupAddCourse.IsOpen = true;
            txtCourseDetails.Focus();
        }
        private void UpdateTextBoxBinding()
        {

            object focusedElement = FocusManager.GetFocusedElement();

            if (focusedElement != null && focusedElement is TextBox)
            {
                TextBox textBox = (TextBox)focusedElement;
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            sourceObject = ((FrameworkElement)sender).DataContext as Message;
            UpdateTextBoxBinding();
            EditPopup.IsOpen = true;
            edittedMessage.Focus();
            
        }

        private async void editsendButton_Click_1(object sender, RoutedEventArgs e)
        {
            int x = 0;
            
            if (String.IsNullOrWhiteSpace(edittedMessage.Text) == false
                && String.IsNullOrEmpty(edittedMessage.Text) == false
               )
            {
                SendData toSend = new SendData();

                toSend.Id = sourceObject.Id;
                toSend.RowKey = sourceObject.Id;
                toSend.From = mycontact;
                toSend.Time = DateTime.Now.ToString();
                toSend.Message = edittedMessage.Text;
                toSend.To = App.ViewModel.Items[index].PhoneNumber;

                await chatsTable.UpdateAsync(toSend);
                        
                        for (x = (App.ViewModel.Items[index].Chats.Count - 1); x >= 0; x--)
                        {
                            if (App.ViewModel.Items[index].Chats[x].Id.Equals(sourceObject.Id))
                            {
                                App.ViewModel.Items[index].Chats[x].Text = App.ViewModel.Items[index].Chats[x].Text + Environment.NewLine + "[Edited]" + Environment.NewLine + edittedMessage.Text ;
                                 break;
                            }

                        }
                        
                    
                
                await App.ViewModel.UpdateChats();
                if (ChatModel.AzureConnected)
                   App.ViewModel.Sync();
            }

            EditPopup.IsOpen = false;
        }

        private void editcancelButton_Click_1(object sender, RoutedEventArgs e)
        {
            edittedMessage.Text = "";
            EditPopup.IsOpen = false;
        }
    }

}
        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    
