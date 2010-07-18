using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FlatDB
{

    public sealed class FlatRecord<T>
    {
        private T _value;

        public T Value
        {
            get { return _value; }
            set
            {
                _value = Value;
            }
        }

        public bool PendingChanges
        {
            get;
            internal set;
        }

        private Guid? _ident;
        public Guid Identifier
        {
            get
            {
                if (_ident.HasValue == false)
                    _ident = Guid.NewGuid();
                return _ident.Value;
            }
            private set
            {
                _ident = value;
            }
        }

        private FlatRecord(T Value, bool isNew)
        {
            _value = Value;
            PendingChanges = isNew;
        }

        internal static FlatRecord<T> CreateFromExistingData(T Value, Guid Identifier)
        {
            FlatRecord<T> rec = new FlatRecord<T>(Value, false);
            rec.Identifier = Identifier;
            return rec;
        }

        internal static FlatRecord<T> CreateAsNewRecord(T Value)
        {
            return new FlatRecord<T>(Value, true);
        }

        public static FlatRecord<object> ObjectFlatRecord(FlatRecord<T> Record)
        {
            FlatRecord<object> _rec = new FlatRecord<object>((object)Record.Value, false);
            _rec.Identifier = Record.Identifier;
            _rec.PendingChanges = Record.PendingChanges;
            return _rec;
        }

        public static FlatRecord<T> TypedFlatRecord(FlatRecord<object> Record)
        {
            FlatRecord<T> _rec = new FlatRecord<T>((T)Record.Value, false);
            _rec.Identifier = Record.Identifier;
            _rec.PendingChanges = Record.PendingChanges;
            return _rec;
        }

        public static List<FlatRecord<T>> TypedFlatRecordList(List<FlatRecord<object>> RecordList)
        {
            return RecordList.Select<FlatRecord<object>, FlatRecord<T>>(rec => FlatRecord<T>.TypedFlatRecord(rec)).ToList();
        }

        public static List<FlatRecord<object>> ObjectFlatRecordList(List<FlatRecord<T>> RecordList)
        {
            return RecordList.Select<FlatRecord<T>, FlatRecord<object>>(rec => FlatRecord<T>.ObjectFlatRecord(rec)).ToList();
        }

        internal XElement ToStorageForm()
        {
            return new XElement(
                _res.xmlRecordSectionName,
                new XAttribute(_res.xmlRecordIdentAttribute,
                    this.Identifier.ToString()),
                    Encoding.UTF8.GetString(this.Value.Ser()));
        }


    }
}