using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Messaging_App.Resources;
using System.Collections.Generic;
using Windows.Storage;
using DreamTimeStudioZ.Recipes;
using System.Threading.Tasks;
using Microsoft.Phone.UserData;
using Microsoft.WindowsAzure.MobileServices;
using Messaging_App.AzureModel;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Xna.Framework.GamerServices;
using System.Configuration;
using Microsoft.WindowsAzure.MobileServices.Sync;



namespace Messaging_App.ViewModels
{
    public class ChatModel : INotifyPropertyChanged
    {
       
        CloudTable table;
       
        public static bool AzureConnected=false;
       
        
        const string dataFileName = "Chats.xml";
        const string FavContactFileName = "Contacts.xml";
        public ChatModel()
        {
            LoadContacts(string.Empty);
        }
        public ObservableCollection<ChatData> Items { get; private set; }
        public ObservableCollection<ChatData> UI2Items { get; private set; }
        public List<AlphaKeyGroup<AddressBook>> DataSource;
        //public List<AlphaKeyGroup<AddressBook>> favDataSource;
        public List<AddressBook> source = new List<AddressBook>();
        //public List<AddressBook> favSource = new List<AddressBook>();
        private ObservableCollection<Contact> _PhoneContacts;
        private ObservableCollection<Favourites> _favcontacts;
        public ObservableCollection<Favourites> Favcontacts 
        {
            get { return _favcontacts; }
            set 
            { 
                _favcontacts = value;
                NotifyPropertyChanged("Favcontacts");
            }
        }

        public ObservableCollection<Contact> PhoneContacts
        {
            get { return _PhoneContacts; }
            set 
            { 
                _PhoneContacts = value;
                NotifyPropertyChanged("PhoneContacts");
            }
        }


         public void LoadContacts(string searchString)
        {
            var contacts = new Contacts();
            contacts.SearchCompleted += ContactSearchCompleted;
            contacts.SearchAsync(searchString, FilterKind.DisplayName,null);
        
         }

        void ContactSearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            string[] separators = { "(", " " };
            string[] ph_num = new string[2];

            PhoneContacts = new ObservableCollection<Contact>(e.Results);
            foreach (Contact c in PhoneContacts)
            {
                foreach (ContactPhoneNumber ph in c.PhoneNumbers)
                {
                    ph_num = ph.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    source.Add(new AddressBook(c.DisplayName, ph_num[0]));
                }
                }
            DataSource = AlphaKeyGroup<AddressBook>.CreateGroups(source,
                    System.Threading.Thread.CurrentThread.CurrentUICulture,
                    (AddressBook s) => { return s.Name; }, true);
            
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }
        public bool IsFavLoaded
        {
            get;
            private set;
        }
        public void RemoveChat(ChatData ChatToRemove)
        {
            
            this.Items.Remove(ChatToRemove);
            NotifyPropertyChanged("Items");
        }

        public async Task UpdateChats()
        {
            
            StorageFolder appStorageFolder = IORecipes.GetAppStorageFolder();
            await IORecipes.DeleteFileInFolder(appStorageFolder, dataFileName);
            string itemsAsXML = IORecipes.SerializeToString(this.Items);
            StorageFile dataFile = await IORecipes.CreateFileInFolder(appStorageFolder, dataFileName);
            await IORecipes.WriteStringToFile(dataFile, itemsAsXML);

        }
        private async Task ConnectToWindowsAzure()
        {
            bool error = false;
            try
            {
                
                
                //StorageCredentials credentials = new StorageCredentials("anotherteststorage", "eFCCv4wKnODdcO8IyyEOkj9FbqPReGYzreYL0hBz3wtnOtSnXwpBg5fsHuvJ5DazTxBSqKJCUald/AN7ZpgRkA==");

                CloudStorageAccount storageAccount =
        CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=anotherteststorage;AccountKey=eFCCv4wKnODdcO8IyyEOkj9FbqPReGYzreYL0hBz3wtnOtSnXwpBg5fsHuvJ5DazTxBSqKJCUald/AN7ZpgRkA==");
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                
                table = tableClient.GetTableReference("ChatTable"+ChatPage.mycontact);
                await table.CreateIfNotExistsAsync();
                
                //blobClient = account.CreateCloudBlobClient();
                //blobContainer = blobClient.GetContainerReference("container1");
                //await blobContainer.CreateIfNotExistsAsync();
                //await blobContainer.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            catch
            {
                AzureConnected = false;
                error = true;
            }

            if (error)
                Guide.BeginShowMessageBox(
                    "Azure Error",
                    "Unable to connect to Windows Azure",
                    new string[] { "OK" },
                    0,
                    MessageBoxIcon.Error,
                    null, null);
            else
                AzureConnected = true;
        }
        public async Task<bool> ConnectToWindowsAzure(string TableNameToCheck)
        {
            
                bool exists = false;
                try
                {


                    //StorageCredentials credentials = new StorageCredentials("anotherteststorage", "eFCCv4wKnODdcO8IyyEOkj9FbqPReGYzreYL0hBz3wtnOtSnXwpBg5fsHuvJ5DazTxBSqKJCUald/AN7ZpgRkA==");

                    CloudStorageAccount storageAccount =
            CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=anotherteststorage;AccountKey=eFCCv4wKnODdcO8IyyEOkj9FbqPReGYzreYL0hBz3wtnOtSnXwpBg5fsHuvJ5DazTxBSqKJCUald/AN7ZpgRkA==");
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                    table = tableClient.GetTableReference("ChatTable" + TableNameToCheck);
                    exists = await table.ExistsAsync();

                    //blobClient = account.CreateCloudBlobClient();
                    //blobContainer = blobClient.GetContainerReference("container1");
                    //await blobContainer.CreateIfNotExistsAsync();
                    //await blobContainer.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
                }
                catch
                {
                    exists = false;
                }
                return exists;
               }
          
        
        public async Task LoadData()
        {
            //string sentby="";
            //bool found = false;
            //chatsTable = App.MobileService.GetTable<SendData>();
            //MobileServiceCollection<SendData, SendData> store;
            //store = await chatsTable
            //        .Where(x => x.Read == false)
            //        .ToCollectionAsync();
            //foreach(var msg in store)
            //{

            //    foreach(var item in Items )
            //    {
            //        if(item.PhoneNumber.ToString() == msg.Sender)
            //        {
            //            found = true;
            //            ChatPage.index = Convert.ToInt16(item.ID);
            //            sentby = item.ContactName;
            //        }
            //    }
            //    if (found == false)
            //        sentby = msg.Sender;
            //    Message message = new Message()
            //    {
            //        Sender = sentby,
            //        Text = msg.Message,
            //        SendingTime = Convert.ToDateTime(msg.Time)
            //    };
            //    Binder.Instance.Messages.Add(message);
            //    App.ViewModel.Items[ChatPage.index].FirstLine = msg.Message;
            //}

            bool error = false;
            
           //this.Items = new ObservableCollection<ChatData>();
            StorageFolder appStorageFolder = IORecipes.GetAppStorageFolder();
            StorageFile dataFile = await IORecipes.GetFileInFolder(appStorageFolder, dataFileName);
            if (dataFile != null)
            {
                if (!IsDataLoaded)
                {
                    string itemsAsXML = await IORecipes.ReadStringFromFile(dataFile);
                    this.Items = IORecipes.SerializeFromString<ObservableCollection<ChatData>>(itemsAsXML);
                }
            }
            // Sample data; replace with real data
            else
            {
                if (!IsDataLoaded)
                {
                    Items = CreateSampleItems();

                }
            }
            //await ConnectToWindowsAzure();
            if (AzureConnected)
            {
                createfav();

                //Sync();
                TableQuery<SendData> query = new TableQuery<SendData>().Where("PartitionKey eq 'default' and Read eq 'false'");
                TableQuerySegment<SendData> querySegment = null;
                var resultlist = new List<SendData>();

                try
                {
                    while (querySegment == null || querySegment.ContinuationToken != null)
                    {
                        querySegment = await table.ExecuteQuerySegmentedAsync(query, querySegment != null ? querySegment.ContinuationToken : null);
                        resultlist.AddRange(querySegment);
                    }
                }
                catch
                {
                    error = true;
                }
                if (error)
                {
                    
                }
                foreach (SendData s in resultlist)
                {
                    bool found = false;
                    Message temp = new Message();
                    temp.Id = s.RowKey;
                    
                    temp.Text = s.Message;
                    temp.SendingTime = s.Time;
                    foreach (ChatData i in Items)
                    {
                        if (i.PhoneNumber.Equals(s.From))
                        {
                            found = true;
                            ChatPage.index = Convert.ToInt16(i.ID);

                        }
                    }
                        if (!found)
                        {
                            string[] separators = { "(", " " };
                            string[] ph_num = new string[2];
                            int num;
                            
                            foreach (Contact c in PhoneContacts)
                            {
                                foreach (ContactPhoneNumber ph in c.PhoneNumbers)
                                {
                                    ph_num = ph.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                    if (ph_num[0].Length >= 10)
                                    {
                                        
                                        num = ph_num[0].Length - 10;
                                        ph_num[0] = ph_num[0].Substring(num);

                                        if (ph_num[0] == s.From)
                                        {
                                            AddressBook add = new AddressBook(c.DisplayName,ph_num[0]);
                                            CreateNewChat(add,0);
                                            temp.Sender = c.DisplayName;
                                            ChatPage.index = Items.Count - 1;
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                                if (found == true)
                                    break;
                            }
                            if (!found)
                            {
                                ChatData result =
                                            new ChatData()
                                            {
                                                ID = (Items.Count).ToString(),
                                                ContactName = s.From,
                                                PhoneNumber = s.From
                                            };
                                this.Items.Add(result);
                                ChatPage.index = Items.Count - 1;
                            }
                        }

                        Binder.Instance.Messages.Add(temp);
                        App.ViewModel.Items[ChatPage.index].FirstLine = s.Message;
                        s.Read = true;
                        await UpdateChat(s);
                    }
                if(resultlist.Count>0)
                    await App.ViewModel.UpdateChats();
                }
                
                NotifyPropertyChanged("Items");
                this.IsDataLoaded = true;
            
        }
        public async Task<string> SecretMsg_retrieve(string rowkey)
        {
            if (AzureConnected)
            {
                bool error = false;
                TableOperation retrieveOperation = TableOperation.Retrieve<SendData>("secret", rowkey);
                TableResult retrievedResult = null;
                try
                {
                    retrievedResult = await table.ExecuteAsync(retrieveOperation);
                }
                catch
                {
                    error = true;
                }
                if (error)
                {
                   return null;
                }
                else
                {
                    if (retrievedResult.Result != null)
                        return (((SendData)retrievedResult.Result).Message);
                    else
                    {
                        return null;
                    }

                }
            }
            else return null;
        }

        public async Task<string> RetreiveMessage(string rowkey)
        {
            if (AzureConnected)
            {
                bool error = false;
                TableOperation retrieveOperation = TableOperation.Retrieve<SendData>("default", rowkey);
                TableResult retrievedResult = null;
                try
                {
                    retrievedResult = await table.ExecuteAsync(retrieveOperation);
                }
                catch
                {
                    error = true;
                }
                if (error)
                {
                    return null;
                }
                else
                {
                    if (retrievedResult.Result != null)
                        return (((SendData)retrievedResult.Result).Message);
                    else
                    {
                        return null;
                    }

                }
            }
            else return null;
        }

        public async void deleteSecretAzure(string rowkey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<SendData>("secret", rowkey);

            try
            // Execute the operation.
            {
                TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);

                // Assign the result to a CustomerEntity.
                SendData deleteEntity = (SendData)retrievedResult.Result;

                // Create the Delete TableOperation.
                if (deleteEntity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                    // Execute the operation.
                    await table.ExecuteAsync(deleteOperation);


                }
            }
            catch(Exception)
            {
                Guide.BeginShowMessageBox(
                           "Azure Error",
                           "Cannot delete from azure please check internet connection",
                           new string[] { "OK" }, 0,
                           MessageBoxIcon.Alert,null, null);
            }
        }
        public async void createfav()
        {
            
            int num;
            string[] separators = { "("," "};
            string[] ph_num= new string[2];
            
            bool contactavailable;
            StorageFolder appStorageFolder = IORecipes.GetAppStorageFolder();
            StorageFile dataFile = await IORecipes.GetFileInFolder(appStorageFolder, FavContactFileName);
            if (dataFile != null)
            {
                if (!IsFavLoaded)
                {
                    string itemsAsXML = await IORecipes.ReadStringFromFile(dataFile);
                    this.Favcontacts = IORecipes.SerializeFromString<ObservableCollection<Favourites>>(itemsAsXML);
                    
                }
            }
            // Sample data; replace with real data
            else
            {

                this.Favcontacts = new ObservableCollection<Favourites>();
                foreach (Contact c in PhoneContacts)
                {
                    foreach (ContactPhoneNumber ph in c.PhoneNumbers)
                    {
                        ph_num = ph.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);

                        if (ph_num[0].Length >= 10)
                        {
                            num = ph_num[0].Length - 10;
                            ph_num[0] = ph_num[0].Substring(num);


                            contactavailable = await ConnectToWindowsAzure(ph_num[0]);
                            if (contactavailable)
                            {
                                Favourites fav = new Favourites()
                                {
                                    Name = c.DisplayName,
                                    Favouritenumber = ph_num[0]
                                };
                                this.Favcontacts.Add(fav);
                                //favSource.Add(new AddressBook(c.DisplayName, ph_num[0]));

                            }
                        }
                    }
                }
                
                string itemsAsXML = IORecipes.SerializeToString(this.Favcontacts);
                dataFile = await IORecipes.CreateFileInFolder(appStorageFolder,FavContactFileName);
                await IORecipes.WriteStringToFile(dataFile, itemsAsXML);
            }
            //favDataSource = AlphaKeyGroup<AddressBook>.CreateGroups(favSource,
            //        System.Threading.Thread.CurrentThread.CurrentUICulture,
            //        (AddressBook s) => { return s.Name; }, true);
            IsFavLoaded = true;
        }
       
         public async void refreshfav()
        {
            int num;
            string[] separators = { "(", " " };
            string[] ph_num = new string[2];
            bool contactavailable;
            this.Favcontacts = new ObservableCollection<Favourites>();
            //favDataSource = new List<AlphaKeyGroup<AddressBook>>();
            foreach (Contact c in PhoneContacts)
            {
                foreach (ContactPhoneNumber ph in c.PhoneNumbers)
                {
                    ph_num = ph.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    if (ph_num[0].Length >= 10)
                    {
                        num = ph_num[0].Length - 10;
                        ph_num[0] = ph_num[0].Substring(num);


                        contactavailable = await ConnectToWindowsAzure(ph_num[0]);
                        if (contactavailable)
                        {
                            Favourites fav = new Favourites()
                            {
                                Name = c.DisplayName,
                                Favouritenumber = ph_num[0]
                            };
                            this.Favcontacts.Add(fav);
                            //favSource.Add(new AddressBook(c.DisplayName, ph_num[0]));
                        }
                    }
                }
            }
            //favDataSource = AlphaKeyGroup<AddressBook>.CreateGroups(favSource,
            //       System.Threading.Thread.CurrentThread.CurrentUICulture,
            //       (AddressBook s) => { return s.Name; }, true);
            StorageFolder appStorageFolder = IORecipes.GetAppStorageFolder();
            await IORecipes.DeleteFileInFolder(appStorageFolder, FavContactFileName);
            string itemsAsXML = IORecipes.SerializeToString(this.Favcontacts);
            StorageFile dataFile = await IORecipes.CreateFileInFolder(appStorageFolder, FavContactFileName);
            await IORecipes.WriteStringToFile(dataFile, itemsAsXML);




        }

        public async void Sync()
        {
            string errorString = null;
            
            


            try
            {
                await App.MobileService.SyncContext.PushAsync();
            }
            catch (MobileServicePushFailedException ex)
            {
                errorString = "Push failed because of sync errors: " +
                  ex.PushResult.Errors.Count + " errors, message: " + ex.Message;
            }
            catch (Exception ex)
            {
                errorString = "Push failed: " + ex.Message +
                  "\n\nIf you are still in an offline scenario, " +
                  "you can try your Push again when connected with your Mobile Serice.";
            }

            if (errorString != null)
            {
                Guide.BeginShowMessageBox(
                       "Azure Error",
                       errorString,
                       new string[] { "OK" },
                       0,
                       MessageBoxIcon.Error,
                       null, null);
                
            }
        }
        public async Task UpdateChat(SendData chatToUpdate)
        {
            TableOperation updateOp = TableOperation.InsertOrReplace(chatToUpdate);
            await table.ExecuteAsync(updateOp);
   
        }

        private ObservableCollection<ChatData> CreateSampleItems()
        {

            ObservableCollection<ChatData> data = new ObservableCollection<ChatData>();
            this.Items = data;
            /*ObservableCollection<Message> msg1 = new ObservableCollection<Message>();
            msg1.Add(new Message()
            {
                Text = "Hello",
                SendingDate = "26/12/2015"
            });
            ObservableCollection<Message> msg2 = new ObservableCollection<Message>();
            msg2.Add(new Message()
            {
                Text = "Please Work",
                SendingDate = "26/12/2015"
            });
            ObservableCollection<Message> msg3 = new ObservableCollection<Message>();
            msg3.Add(new Message()
            {
                Text = "It Worked",
                SendingDate = "26/12/2015"
            });
            data.Add(new ChatData()
            {
                ContactName = "shashank",
                
                ID = nextitemnumber.ToString(),
                PhoneNumber = 9958506118,
                Chats=msg1
            });
            nextitemnumber++;
            data.Add(new ChatData()
            {
                ContactName = "venkat",
               
                ID = nextitemnumber.ToString(),
                PhoneNumber = 9958506118,
                Chats = msg2
            });
            nextitemnumber++;
            data.Add(new ChatData()
            {
                ContactName = "saketh",
                
                ID = nextitemnumber.ToString(),
                PhoneNumber = 9958506118,
                
            });*/
            
            return data;
        }

        public ChatData CreateNewChat(Favourites mycon)
        {
            foreach (ChatData c in Items)
            {
                if (c.ContactName == mycon.Name)
                    return c;
            }
            
            ChatData result =
            new ChatData()
            {
                ID = (Items.Count).ToString(),
                ContactName = mycon.Name,
                PhoneNumber = mycon.Favouritenumber
            };

            this.Items.Add(result);
            return result;
        }

        //public ChatData CreateNewChat(AddressBook mycon)
        //{   string[] separators = { "("," "};
        //    string[] ph_num= new string[2];
        //    int num;
        //    foreach (ChatData c in Items)
        //    {
        //        if (c.ContactName == mycon.DisplayName)
        //            return c;
        //    }
        //    foreach(ContactPhoneNumber ph in mycon.PhoneNumbers)
        //    {   //To Note Only the first number of multi-phone number contact will be used
        //        ph_num = ph.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
        //        if (ph_num[0].Length >= 10)
        //        {
        //            num = ph_num[0].Length - 10;
        //            ph_num[0] = ph_num[0].Substring(num);
        //        }
        //        break;
        //    }
        //      ChatData result =
        //    new ChatData()
        //    {
        //        ID = (Items.Count).ToString(),
        //        ContactName = mycon.DisplayName,
        //        PhoneNumber = ph_num[0]
        //    };
            
        //    this.Items.Add(result);
        //    return result;
        //}

        public ChatData CreateNewChat(AddressBook mycon, int i)
        {
            if (i == 0)
            {
                foreach (ChatData c in Items)
                {
                    if (c.ContactName == mycon.Name)
                        return c;
                }

            }
            else if (i == 1)
            {
                foreach (ChatData d in UI2Items)
                {
                    if (d.ContactName == mycon.Name)
                        return d;
                }
                ChatData result =
                    new ChatData()
                    {
                        ID = (Items.Count).ToString(),
                        ContactName = mycon.Name,
                        PhoneNumber = mycon.Phone
                    };
                this.UI2Items.Add(result);
                this.Items.Add(result);
                return result;
            }
            ChatData res =
            new ChatData()
            {
                ID = (Items.Count).ToString(),
                ContactName = mycon.Name,
                PhoneNumber = mycon.Phone
            };
            this.Items.Add(res);
            return res;
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