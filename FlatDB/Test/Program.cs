using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            FlatDB.FlatDatabase db = new FlatDB.FlatDatabase(@"C:\users\clint\desktop\testdatabase.xml", true);

            //var cont = new FlatDB.FlatTable<Contact>();

            //var contacts = db.GetTable<Contact>();

            FlatDB.FlatTable<Contact> c = new FlatDB.FlatTable<Contact>();
            c.AddRecord(new Contact()
            {
                Title = "Mr",
                FirstName = "Bob",
                LastName = "Smith",
                Email = "smith.bob@gmail.com",
            });

            FlatDB.FlatTable<object> o = FlatDB.FlatTable<object>.TypedTable<Contact>(c);

            //cont.AddRecord(
            //    new Contact()
            //    {
            //        Title = "Mr",
            //        FirstName = "Bob",
            //        LastName = "Smith",
            //        Email = "smith.bob@gmail.com",
            //    });

            //cont.AddRecord(
            //    new Contact()
            //    {
            //        Title = "Mrs",
            //        FirstName = "Alice",
            //        LastName = "Smith",
            //        Email = "smith.alice@googlemail.com",
            //    });

            //db.AddTable<Contact>(cont);

            //db.Submit();
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
