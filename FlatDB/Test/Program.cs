using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            FlatDB.FlatDatabase db = new FlatDB.FlatDatabase(@"C:\users\clint\desktop\db1.xml", true);
            FlatDB.FlatTable<Contact> contacts;
            try
            {
                contacts = db.CreateTable<Contact>();
            }
            catch (Exception)
            {
                contacts = (FlatDB.FlatTable<Contact>)db.GetTableByType<Contact>();
            }

            //contacts.AddRecord(
            //    new Contact()
            //    {
            //        Title = "Mr",
            //        FirstName = "Bob",
            //        LastName = "Smith",
            //        Email = null,
            //    });

            var misters = from mr in db.GetTableByType<Contact>()
                          where mr.Title == "Mr"
                          select mr;

            //db.DeleteTable<Contact>("Contacts");

            db.UpdateOnSubmit<Contact>(contacts);
            db.Submit();

            foreach(Contact c in misters)
            {
                Console.WriteLine(
                    String.Format("{0} {1} {2} - {3}",
                    c.Title,
                    c.FirstName,
                    c.LastName,
                    c.Email));
            }

            Console.ReadLine();
        }
    }

    public sealed class Contact
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
