using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging_App.ViewModels
{
    public class AddressBook
    {
        private string p;
        private Microsoft.Phone.UserData.ContactPhoneNumber ph;

        public string Name
        {
            get;
            set;
        }

        public string Phone
        {
            get;
            set;
        }

        public AddressBook(string name, string ph)
        {
            this.Name = name;
            this.Phone = ph;
        }
    }
}
