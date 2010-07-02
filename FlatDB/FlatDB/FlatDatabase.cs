using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;

namespace FlatDB
{
    /// <summary>
    /// Represents a Flat-file database
    /// </summary>
    public sealed class FlatDatabase
    {
        /// <summary>
        /// The list of tables in the database
        /// </summary>
        private List<FlatTable> _tables = new List<FlatTable>();
        
        /// <summary>
        /// The file Path of the database
        /// </summary>
        private string _filePath = string.Empty;

        /// <summary>
        /// The file path of the database
        /// </summary>
        public string Path
        {
            get { return _filePath; }
        }
        
        /// <summary>
        /// Creates an instance of flat database
        /// </summary>
        /// <param name="Path">The path to the database file</param>
        public FlatDatabase(string Path)
        {
            this._filePath = Path;
            _innerBootstrap();
        }

        /// <summary>
        /// Creates an instance of flat database
        /// </summary>
        /// <param name="Path">The path to the database file</param>
        /// <param name="Create">Whether to create the database if it doesn't exist</param>
        public FlatDatabase(string Path, bool Create)
        {
            this._filePath = Path;
            if (System.IO.File.Exists(Path) == false && Create)
                _innerCreate();

            _innerBootstrap();
        }

        /// <summary>
        /// Bootstraps the database
        /// </summary>
        private void _innerBootstrap()
        {
            if (System.IO.File.Exists(_filePath) == false)
            {
                throw new System.IO.FileNotFoundException(_filePath);
            }
            else
            {
                _loadDatabase();
            }
        }

        /// <summary>
        /// Loads the database file and perfoms a process on it
        /// </summary>
        /// <param name="Process">The process to perform</param>
        private void _loadDatabaseAndProcess(Action<XDocument> Process)
        {
            XDocument xdoc = null;
            try
            {
                xdoc = XDocument.Load(_filePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot load database", ex);
            }

            if (Process != null)
                Process(xdoc);
        }

        /// <summary>
        /// Loads the database file and reads in the data
        /// </summary>
        private void _loadDatabase()
        {
            _loadDatabaseAndProcess(
                (db) =>
                {
                    // Get all the tables
                    foreach (XElement tableElement in db.Root.Element("Tables").Elements())
                    {
                        // Add table devoid of records
                        _tables.Add(
                            new FlatTable(tableElement.Attribute("Name").Value,
                                tableElement.Attribute("Type").Value, null));

                        // Get the records
                        List<FlatRecord<object>> _loadedRecords = new List<FlatRecord<object>>();
                        foreach (XElement recElement in tableElement.Element("Records").Elements())
                        {
                            // Deserialse the value for the record
                            MemoryStream ms = new MemoryStream();
                            byte[] _b = Encoding.UTF8.GetBytes(recElement.Element("Value").Value);
                            ms.Write(_b, 0, _b.Length);
                            ms.Position = 0;
                            // Get the table in the database (in-memory) that is linked to this table in the file
                            FlatTable table = _tables[_tables.IndexOf(_tables.Single(t => t.Name == tableElement.Attribute("Name").Value))];

                            // Add the record to the in-memory collection (after deserialising the value)
                            XmlSerializer ser = new XmlSerializer(table.UnderlyingType);
                            _loadedRecords.Add(new FlatRecord<object>(Guid.Parse(recElement.Attribute("Ident").Value), ser.Deserialize(ms)));
                        }

                        // Updates the linked table's records
                        _tables[_tables.IndexOf(_tables.Single(t => t.Name == tableElement.Attribute("Name").Value))].Records = _loadedRecords;
                    }
                });
        }

        /// <summary>
        /// Creates an empty database file
        /// </summary>
        private void _innerCreate()
        {
            XElement _database = new XElement(
                "FlatDB",
                new XElement("Tables"));
            _database.Save(_filePath);
        }

        #region "Table Methods"
        /// <summary>
        /// Creates a new table with a type and a non-default name
        /// </summary>
        /// <typeparam name="T">The type representing structure</typeparam>
        /// <param name="Name">The name to give the table</param>
        /// <returns></returns>
        public FlatTable<T> CreateTable<T>(string Name)
        {
            if (_tables.Any(t => t.Name.ToLower().Trim() == Name.ToLower().Trim()))
            {
                throw new Exception("Table '" + Name + "' already exists, use GetTable instead");
            }
            else if (TableOfTypeExists<T>() == true)
            {
                throw new Exception("Table of type '" + typeof(T).FullName + "' already exists");
            }
            else
            {
                // Create the xml, put it in the file and put the table representation in the in-memory collection
                FlatTable<T> table = new FlatTable<T>(Name);
                _tables.Add(FlatTable.InferFromFlatTable<T>(table));
                _loadDatabaseAndProcess(
                    (db) =>
                    {
                        db.Root.Element("Tables").Add(
                            new XElement("Table",
                                new XAttribute("Name", Name),
                                new XAttribute("Type",table.UnderlyingType.AssemblyQualifiedName),
                                new XElement("Records")));
                        db.Save(_filePath);
                    });
                return table;
            }
        }

        /// <summary>
        /// Deletes a table 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DeleteTable<T>()
        {
            if (TableOfTypeExists<T>() == true)
            {
                FlatTable _tbl = _tables.First(t => t.UnderlyingType == typeof(T));
                
                // Remove the xml from the file
                _loadDatabaseAndProcess(
                    (db) =>
                    {
                        db.Root.Element("Tables").Elements().First(t => t.Attribute("Name").Value == _tbl.Name).Remove();
                        db.Save(_filePath);
                    });
                // Remove table from memory
                _tables.Remove(_tbl);
            }
            else
            {
                throw new Exception("No table of Type '" + typeof(T).AssemblyQualifiedName + "' was found for deletion");
            }
        }

        /// <summary>
        /// Deletes table of a specified name
        /// </summary>
        /// <param name="Name">The name of the table</param>
        public void DeleteTable(string Name)
        {
            if (_tables.Any(t => t.Name == Name))
            {
                _tables.Remove(_tables.First(t => t.Name == Name));
                _loadDatabaseAndProcess(
                    (db) =>
                    {
                        db.Root.Element("Tables").Elements().First(t => t.Attribute("Name").Value == Name).Remove();
                        db.Save(_filePath);
                    });
            }
            else
            {
                throw new Exception("Table of name '" + Name + "' not found for deletion");
            }
        }

        //[Obsolete("Works same way as DeleteTable(string Name)")]
        //public void DeleteTable<T>(string Name)
        //{
        //    if (_tables.Any(t => t.Name == Name))
        //    {
        //        _tables.Remove(_tables.First(t => t.Name == Name));
        //        _loadDatabaseAndProcess(
        //            (db) =>
        //            {
        //                db.Root.Element("Tables").Elements().First(t => t.Attribute("Name").Value == Name).Remove();
        //                db.Save(_filePath);
        //            });
        //    }
        //    else
        //    {
        //        throw new Exception("Table of name '" + Name + "' not found for deletion");
        //    }
        //}

        /// <summary>
        /// Checks whether a table of a specified type exists in the database
        /// </summary>
        /// <typeparam name="T">Type to check for</typeparam>
        /// <returns></returns>
        private bool TableOfTypeExists<T>()
        {
            return _tables.Any(t => t.UnderlyingType == typeof(T));
        }

        /// <summary>
        /// Creates a table from a type and auto-pluralises its name
        /// </summary>
        /// <typeparam name="T">Type for structure</typeparam>
        /// <returns></returns>
        public FlatTable<T> CreateTable<T>()
        {
            return CreateTable<T>(System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(
                CultureInfo.CurrentCulture).Pluralize(typeof(T).Name));
        }

        /// <summary>
        /// Gets the table represented by a specified type
        /// </summary>
        /// <typeparam name="T">The type for the table</typeparam>
        /// <returns></returns>
        public FlatTable<T> GetTableByType<T>()
        {
            FlatTable table = _tables.DefaultIfEmpty(null).FirstOrDefault(t => t.UnderlyingType == typeof(T));
            FlatTable<T> tb = FlatTable<T>.InferFromFlatTable(table);
            return tb;
        }

        /// <summary>
        /// Gets a table on name without type being considered
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public FlatTable UnsafeGetTableByName(string Name)
        {
            return _tables.DefaultIfEmpty(null).FirstOrDefault(t => t.Name.ToLower().Trim() == Name.ToLower().Trim());
        }

        /// <summary>
        /// Gets a table on name and type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Name"></param>
        /// <returns></returns>
        public FlatTable<T> SafeGetTableByName<T>(string Name)
        {
            return FlatTable<T>.InferFromFlatTable(_tables.DefaultIfEmpty(null).FirstOrDefault(t => (t.Name.ToLower().Trim() == Name.ToLower().Trim()) &&
                (t.UnderlyingType == typeof(T))));
        }

        /// <summary>
        /// Adds a table to the update list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Table"></param>
        public void UpdateOnSubmit<T>(FlatTable<T> Table)
        {
            _tablesToUpdate.Clear();
            _tablesToUpdate.Add(_tables[_tables.IndexOf(_tables.First(t => t.Name == Table.Name.ToString()))] = FlatTable.InferFromFlatTable<T>(Table));
        }
        private List<FlatTable> _tablesToUpdate = new List<FlatTable>();
        #endregion

        /// <summary>
        /// Updates the database file with all changes registered via UpdateOnSubmit, Unregisters tables after submit is complete
        /// </summary>
        public void Submit()
        {
            _loadDatabaseAndProcess(
                (db) =>
                {
                    _saveTables(db);
                });
            _tablesToUpdate.Clear();
        }

        /// <summary>
        /// Saves the tables
        /// </summary>
        /// <param name="db"></param>
        private void _saveTables(XDocument db)
        {
            // Go through each table in file
            foreach (XElement tblElement in db.Root.Element("Tables").Elements())
            {
                // Checks if this table is in the update list
                if (_tablesToUpdate.Any(t => t.Name == tblElement.Attribute("Name").Value))
                {
                    // Create the record, and fill it with identifier and serialised value
                    XElement recs = tblElement.Element("Records");
                    recs.RemoveNodes();
                    FlatTable table = _tables.First(t => t.Name == tblElement.Attribute("Name").Value);
                    foreach (FlatRecord<object> rec in table.Records)
                    {
                        XmlSerializer ser = new XmlSerializer(table.UnderlyingType);
                        MemoryStream ms = new MemoryStream();
                        ser.Serialize(ms, rec.Value);
                        ms.Position = 0;
                        byte[] _b = new byte[ms.Length];
                        ms.Read(_b, 0, _b.Length);
                        recs.Add(
                            new XElement(
                                "Record",
                                new XAttribute("Ident", rec.Identifier),
                                new XElement("Value",
                                    Encoding.UTF8.GetString(_b))));
                    }
                }
            }

            db.Save(_filePath);
        }
    }
}
