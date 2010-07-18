using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Linq;

namespace FlatDB
{
    public sealed class FlatDatabase
    {
        private List<FlatTable> _tables = new List<FlatTable>();

        public FlatDatabase(string Path, bool create = false)
        {
            this.Path = Path;
            if (File.Exists(Path) == false)
            {
                if (create)
                {
                    _createEmptyDatabase();
                }
                else
                {
                    throw new FileNotFoundException(string.Format(_res.DatabaseNotFoundMessage, Path));
                }
            }

            Load();
        }

        private void _createEmptyDatabase()
        {
            var doc = new XDocument();
            doc.Add(
                new XElement(_res.xmlRootName,
                    new XElement(_res.xmlTablesSectionName)));
            doc.Save(Path);
        }

        public string Path
        {
            get;
            set;
        }


        public void Load()
        {
            if (File.Exists(Path) == false)
                throw new FileNotFoundException(string.Format(_res.DatabaseNotFoundMessage, Path));

            var xdoc = XDocument.Load(Path);

            foreach (XElement tblElement in xdoc.Root.Element(_res.xmlTablesSectionName).Elements(_res.xmlTableSectionName))
            {
                FlatTable tbl = new FlatTable(Type.GetType(tblElement.Attribute(_res.xmlTableTypeAttribute).Value));
                List<FlatRecord<object>> _recs = new List<FlatRecord<object>>();
                foreach (XElement recElement in tblElement.Element(_res.xmlRecordsSectionName).Elements(_res.xmlRecordSectionName))
                {
                    //TODO Incorporate some sort of try catch for the possible failure of serialisation / guid parsing
                    FlatRecord<object> o = FlatRecord<object>.CreateFromExistingData(
                        recElement.Value.DeSer(tbl.UnderlyingType), Guid.Parse(recElement.Attribute(_res.xmlRecordIdentAttribute).Value));
                    tbl.Records.Add(o);
                }
                _tables.Add(tbl);
            }
        }

        public void AddTable<T>(FlatTable<T> table)
        {
            _tables.Add(table);
        }

        public FlatTable<T> GetTable<T>()
        {
            if (_tables.Any(t => t.UnderlyingType.Equals(typeof(T))) == false)
                throw new ArgumentException(string.Format(_res.TableOfTypeNotFoundMessage, typeof(T).Name));

            FlatTable _origTable = _tables.First(t => t.UnderlyingType.Equals(typeof(T)));
            return FlatTable<T>.CreateAsWrapper(_origTable);
        }

        public void Submit()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }

            bool _compress = false;

            var xdoc = new XDocument();
            xdoc.Add(
                new XElement(_res.xmlRootName,
                    new XElement(_res.xmlTablesSectionName)));

            foreach (FlatTable tbl in _tables)
            {
                xdoc.Root.Element(_res.xmlTablesSectionName).Add(tbl.ToStorageForm());
            }

            xdoc.Save(Path);
            if (_compress)
            {
                File.Move(Path, Path + ".ren");
                using (FileStream fs = new FileStream(Path + ".ren", FileMode.Open))
                {
                    using (FileStream f2 = new FileStream(Path, FileMode.OpenOrCreate))
                    {
                        using (GZipStream gs = new GZipStream(f2, CompressionMode.Compress))
                        {
                            fs.CopyTo(gs);
                        }
                    }
                }
                File.Delete(Path + ".ren");
            }
        }
    }
}
