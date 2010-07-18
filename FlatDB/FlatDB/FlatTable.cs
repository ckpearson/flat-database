using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;


namespace FlatDB
{
    public class FlatTable : IEnumerable
    {
        protected List<FlatRecord<object>> _records = new List<FlatRecord<object>>();

        public Type UnderlyingType { get; protected set; }

        public FlatTable(Type t)
        {
            UnderlyingType = t;
            this.Name = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(
                CultureInfo.CurrentCulture).Pluralize(t.Name);
        }

        public string Name
        {
            get;
            private set;
        }

        public List<FlatRecord<object>> Records
        {
            get
            {
                return _records;
            }
            set
            {
                if (_records != value)
                    _records = value;
            }
        }

        private int _recordCountAtLastCheck = 0;

        public bool PendingChanges
        {
            get
            {
                bool val = (_records.Any((r) => r.PendingChanges)) && (_recordCountAtLastCheck != _records.Count);
                _recordCountAtLastCheck = _records.Count;
                return val;
            }
        }

        public void AddRecord(object Value)
        {
            if (Value.GetType().Equals(UnderlyingType) == false)
            {
                throw new ArgumentException("Value type doesn't match Underlying Type", "Value");
            }

            _records.Add(FlatRecord<object>.CreateAsNewRecord(Value));
        }

        public void RemoveRecord(Guid Identifier)
        {
            FlatRecord<object> rec = _records.DefaultIfEmpty(null).FirstOrDefault(r => r.Identifier == Identifier);
            if (_records == null)
            {
                _records.RemoveAt(_records.IndexOf(rec));
            }
            else
            {
                throw new ArgumentException(string.Format("Record with Identifier '{0}' not found", Identifier.ToString()));
            }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _records.Select(r => r.Value).GetEnumerator();
        }

        #endregion

        internal XElement ToStorageForm()
        {
            return new XElement(_res.xmlTableSectionName,
                    new XAttribute(_res.xmlTableNameAttribute, this.Name),
                    new XAttribute(_res.xmlTableTypeAttribute, this.UnderlyingType.AssemblyQualifiedName),
                    new XElement(_res.xmlRecordsSectionName,
                    this.Records.Select<FlatRecord<object>, XElement>(rec => rec.ToStorageForm())));
        }
    }

    public sealed class FlatTable<T> : FlatTable, IEnumerable<T>
    {
        public FlatTable()
            : base(typeof(T))
        {
            base.UnderlyingType = typeof(T);
        }

        public new List<FlatRecord<T>> Records
        {
            get
            {
                return base._records.Select<FlatRecord<object>, FlatRecord<T>>(rec => FlatRecord<T>.TypedFlatRecord(rec)).ToList();
            }
            set
            {
                base._records = value.Select<FlatRecord<T>, FlatRecord<object>>(rec => FlatRecord<T>.ObjectFlatRecord(rec)).ToList();
            }
        }

        private FlatTable _wrapperTable = null;
        internal static FlatTable<T> CreateAsWrapper(FlatTable Table)
        {
            FlatTable<T> tbl = new FlatTable<T>();
            tbl._wrapperTable = Table;
            tbl.Records = FlatRecord<T>.TypedFlatRecordList(Table.Records);
            return tbl;
        }

        public static FlatTable<object> TypedTable<T>(FlatTable<T> Table)
        {
            var tbl = new FlatTable<object>();
            tbl.Records = Table.Records.Select(rec => FlatRecord<object>.CreateFromExistingData(rec.Value,
                rec.Identifier)).ToList();
            return tbl;
        }

        // new keyword not needed apparently (this is likely because T is not the same as object, might be
        // worth if this could be overriden somehow? (prevent access to the base method)
        public void AddRecord(T Value)
        {
            base.AddRecord((object)Value);
        }

        #region IEnumerable<T> Members

        public new IEnumerator<T> GetEnumerator()
        {
            return this.Records.Select(r => r.Value).GetEnumerator();
        }

        #endregion
    }

    

    
}
