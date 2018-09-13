using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Messaging_App.Resources;
using Messaging_App.ViewModels;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;
using Windows.Networking.PushNotifications;
using Microsoft.WindowsAzure.Messaging;
using System.Windows.Threading;
using Windows.Data.Xml.Dom;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Notifications;


namespace Messaging_App
{
    public partial class App : Application
    {
        private static ChatModel viewModel = null;
        //public PushNotificationChannel PushChannel;
        
        public static MobileServiceClient MobileService = new MobileServiceClient(
                            "https://anothertest.azure-mobile.net/",
                            "tskbROAbfLBiIPnAEjStCSUDfipJiV54");

        public PushNotificationChannel PushChannel;
        private async void InitNotificationsAsync()
        {
            Exception exception = null;

            try
            {
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                
                var hub = new NotificationHub("messappnotify", "Endpoint=sb://anothertesthub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=7AFak6SEVVqvAMLs5YxRJ2ZUAhLzhici6wYnenrrVzM=");
                var result = await hub.RegisterNativeAsync(channel.Uri);
                
                // Displays the registration ID so you know it was successful
                if (result.RegistrationId != null)
                {
                    //var dialog = new MessageDialog("Registration successful: " + result.RegistrationId);
                    //dialog.Commands.Add(new UICommand("OK"));
                    //await dialog.ShowAsync();

                    

                    
                    PushChannel = channel;
                    //PushChannel.PushNotificationReceived += OnPushNotification;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {

            }
        }

        //private async void OnPushNotification(PushNotificationChannel sender, PushNotificationReceivedEventArgs e)
        //{
        //    String notificationContent = String.Empty;

        //    e.Cancel = true;

        //    switch (e.NotificationType)
        //    {
        //        // Badges are not yet supported and will be added in a future version
        //        case PushNotificationType.Badge:
        //            notificationContent = e.BadgeNotification.Content.GetXml();
        //            break;

        //        // Tiles are not yet supported and will be added in a future version
        //        case PushNotificationType.Tile:
        //            notificationContent = e.TileNotification.Content.GetXml();
        //            break;

        //        // The current version of AzureChatr only works via toast notifications
        //        case PushNotificationType.Toast:
        //            notificationContent = e.ToastNotification.Content.GetXml();
        //            XmlDocument toastXml = e.ToastNotification.Content;

        //            // Extract the relevant chat item data from the toast notification payload
        //            XmlNodeList toastTextAttributes = toastXml.GetElementsByTagName("text");
        //            string username = toastTextAttributes[0].InnerText;
        //            string chatline = toastTextAttributes[1].InnerText;
        //            string chatdatetime = toastTextAttributes[2].InnerText;
        //            break;
        //            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //            //{
        //            //    //var chatItem = new ChatItem { Text = chatline, UserName = username };
        //            //    //// Post the new chat item received in the chat window.
        //            //    //// IMPORTANT: If you updated the code above to post new chat lines from
        //            //    ////            the current user immediately in the chat window, you will
        //            //    ////            end up with duplicates here. You need to filter-out the
        //            //    ////            current user's chat entries to avoid these duplicates.
        //            //    //items.Add(chatItem);
        //            //    //ScrollDown();
        //            //});

        //            // This is a quick and dirty way to make sure that we don't use speech synthesis
        //            // to read the current user's chat lines out loud
                    

                    

        //        // Raw notifications are not used in this version
        //        case PushNotificationType.Raw:
        //            notificationContent = e.RawNotification.Content;
        //            break;
        //    }
        //    //e.Cancel = true;
        //}
        private void AcquirePushChannel()
        {

            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                InitNotificationsAsync();
            }
        }
        
        
        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static ChatModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new ChatModel();

                return viewModel;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        // Code to execute when a contract activation such as a file open or save picker returns 
        // with the picked file or other return values
        private void Application_ContractActivated(object sender, Windows.ApplicationModel.Activation.IActivatedEventArgs e)
        {
        }
        public void StoreCategoriesAndSubscribe(IEnumerable<string> categories)
        {
            var categoriesAsString = string.Join(",", categories);
            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (!settings.Contains("categories"))
            {
                
                settings.Add("categories", categoriesAsString);
            }
            else
            {
                settings["categories"] = categoriesAsString;
            }
            settings.Save();

            SubscribeToCategories(categories);
        }

        public void SubscribeToCategories(IEnumerable<string> categories)
        {
            var channel = HttpNotificationChannel.Find("MyPushChannel");

            if (channel == null)
            {
                channel = new HttpNotificationChannel("MyPushChannel");
                
                channel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred); 
                channel.Open();
                channel.BindToShellToast();
                channel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushReceived);
                channel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(HttpPushReceived);
            }
            
            channel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(async (o, args) =>
            {
                var hub = new NotificationHub("messappnotify", "Endpoint=sb://anothertesthub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=7AFak6SEVVqvAMLs5YxRJ2ZUAhLzhici6wYnenrrVzM=");
                await hub.RegisterNativeAsync(args.ChannelUri.ToString(),categories);
            });
        }

        private async void HttpPushReceived(object sender, HttpNotificationEventArgs e)
        {
            bool found = false;
            string message;
            string[] separators = { ":" };
            string[] decode = new string[4];
            string sentby = null;
            int count = 0;
            int index = 0;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(e.Notification.Body))
            {
                count++;
                message = reader.ReadToEnd();

            }


            decode = message.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            switch (decode[0])
            {
                case "message":
                    foreach (ChatData i in App.ViewModel.Items)
                    {
                        if (i.PhoneNumber.Equals(decode[1]))
                        {
                            sentby = i.ContactName;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        foreach (Favourites c in App.ViewModel.Favcontacts)
                        {
                            if (c.Favouritenumber.Equals(decode[1]))
                            {
                                sentby = c.Name;
                                found = true;
                                break;
                            }

                        }
                    }

                    if (!found)
                    {
                        sentby = decode[1];
                    }
                    var toastDescriptor = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                    var txtNodes = toastDescriptor.GetElementsByTagName("text");
                    var audio = toastDescriptor.CreateElement("audio");
                    audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");
                    audio.SetAttribute("loop", "false");

                    txtNodes[0].AppendChild(toastDescriptor.CreateTextNode(sentby));
                    txtNodes[1].AppendChild(toastDescriptor.CreateTextNode(decode[3]));


                    var toast = new ToastNotification(toastDescriptor);
                    toast.Activated += new Windows.Foundation.TypedEventHandler<ToastNotification, object>(toast_Activated);


                    toast.Group = sentby;
                    toast.Tag = decode[2];


                    toast.SuppressPopup = false;

                    var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                    toastNotifier.Show(toast);
                    found = false;
                    break;
                case "secret":
                    foreach (ChatData i in App.ViewModel.Items)
                    {
                        if (i.PhoneNumber.Equals(decode[1]))
                        {

                            for (index = (i.Chats.Count - 1); index >= 0; index--)
                            {
                                if (i.Chats[index].Id.Equals(decode[2]))
                                {
                                    i.Chats[index].Secret = true;
                                    break;
                                }

                            }
                            break;
                        }
                    }
                    break;
                case "edit":
                    foreach (ChatData i in App.ViewModel.Items)
                    {
                        if (i.PhoneNumber.Equals(decode[1]))
                        {
                            sentby = i.ContactName;
                            for (index = (i.Chats.Count - 1); index >= 0; index--)
                            {
                                if (i.Chats[index].Id.Equals(decode[2]))
                                {
                                    i.Chats[index].Text = i.Chats[index].Text + Environment.NewLine + "[Edited]" + Environment.NewLine + decode[3];
                                    found = true;
                                    break;
                                }
                                
                            }
                            break;
                        }
                    }
                    if (!found)
                    {
                        decode[3] = "[Edited] " + (await App.ViewModel.RetreiveMessage(decode[2]));
                    }
                    await App.ViewModel.UpdateChats();
                    toastDescriptor = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                    txtNodes = toastDescriptor.GetElementsByTagName("text");
                    txtNodes[0].AppendChild(toastDescriptor.CreateTextNode(sentby));
                    txtNodes[1].AppendChild(toastDescriptor.CreateTextNode(decode[3]));
                    toast = new ToastNotification(toastDescriptor);
                    toast.Activated += new Windows.Foundation.TypedEventHandler<ToastNotification, object>(toast_Activated);
                    toast.Group = sentby;
                    toast.Tag = decode[2];


                    toast.SuppressPopup = true;
                    if (!found)
                        toast.SuppressPopup = false;

                    toastNotifier = ToastNotificationManager.CreateToastNotifier();
                    toastNotifier.Show(toast);

                    found = false;
                    break;

            }
        }

        void toast_Activated(ToastNotification sender, object args)
        {
            
        }

        private void PushReceived(object sender, NotificationEventArgs e)
        {
           
        }

        private void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        

        public IEnumerable<string> RetrieveCategories()
        {
            var categories = (string)IsolatedStorageSettings.ApplicationSettings["categories"];
            return categories != null ? categories.Split(',') : new string[0];
        }


        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            ////For WNS
            //AcquirePushChannel();
            //var categories = new HashSet<string>();
            //categories.Add(ChatPage.mycontact);
            //StoreCategoriesAndSubscribe(categories);
            
            
        }
 //Sending Broadcast push MPNS        
  //        var channel = HttpNotificationChannel.Find("MyPushChannel");
  //if (channel == null)
 //{
//    channel = new HttpNotificationChannel("MyPushChannel");
//    channel.Open();
//    channel.BindToShellToast();
//}

        //channel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(async (o, args) =>
        //{
        //    hub = new NotificationHub("messappnotify", "Endpoint=sb://anothertesthub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=7AFak6SEVVqvAMLs5YxRJ2ZUAhLzhici6wYnenrrVzM=");
        //    await hub.RegisterNativeAsync(args.ChannelUri.ToString());
        //});


        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Handle contract activation such as a file open or save picker
            PhoneApplicationService.Current.ContractActivated += Application_ContractActivated;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}