using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace FlatDB
{
    /// <summary>
    /// Describes a Table used in the FlatDatabase
    /// NOTE: Normally not usable (In any meaningful way) outside of the Library, use FlatTable[T] Instead.
    /// </summary>
    public class FlatTable : IEnumerable
    {
        /// <summary>
        /// The type the table uses for data structure
        /// </summary>
        public Type UnderlyingType { get; protected set; }

        /// <summary>
        /// The list of records
        /// </summary>
        protected internal List<FlatRecord<object>> _records = new List<FlatRecord<object>>();

        /// <summary>
        /// The records in the table
        /// </summary>
        public List<FlatRecord<object>> Records
        {
            get { return _records; }
            protected internal set { _records = value; }
        }

        /// <summary>
        /// Name of the table
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Creates a new FlatTable
        /// </summary>
        /// <param name="Name">The name of the table</param>
        /// <param name="Underlying">The type used for data structure</param>
        protected FlatTable(string Name, Type Underlying)
        {
            this.Name = Name;
            this.UnderlyingType = Underlying;
        }

        /// <summary>
        /// Creates a new FlatTable
        /// </summary>
        /// <param name="Name">The name of the table</param>
        /// <param name="TypeName">The type used for data structure</param>
        /// <param name="Records">The records to put into the table</param>
        internal FlatTable(string Name, string TypeName, List<FlatRecord<object>> Records)
        {
            this.Name = Name;
            this.UnderlyingType = Type.GetType(TypeName);
            this._records = Records != null ? Records : new List<FlatRecord<object>>();
        }

        /// <summary>
        /// Adds a record to the table
        /// </summary>
        /// <param name="Value">The data to put in the table</param>
        public void AddRecord(Object Value)
        {
            if (Value.GetType() == UnderlyingType)
            {
                _records.Add(new FlatRecord<object>(Value));
            }
            else
            {
                throw new Exception("Value type does not match that of the underlying type");
            }
        }

        /// <summary>
        /// Removes a record from the table
        /// </summary>
        /// <param name="Identifier">The identifier for the record</param>
        public void RemoveRecord(Guid Identifier)
        {
            FlatRecord<object> rec = _records.DefaultIfEmpty(null).FirstOrDefault(r => r.Identifier == Identifier);
            if (rec != null)
            {
                _records.RemoveAt(_records.IndexOf(rec));
            }
            else
            {
                throw new Exception("Record with Identifier '" + Identifier.ToString() + "' not found");
            }
        }

        /// <summary>
        /// Creates a temp working non-generic FlatTable from a generic instance
        /// </summary>
        /// <typeparam name="T">The type used by the generic FlatTable</typeparam>
        /// <param name="Table">The FlatTable[T] to infer from</param>
        /// <returns>The new non-generic FlatTable</returns>
        internal static FlatTable InferFromFlatTable<T>(FlatTable<T> Table)
        {
            FlatTable tbl = new FlatTable(Table.Name, typeof(T));
            // Pull out the records
            tbl._records = (from rec in Table.Records select new FlatRecord<object>(rec.Identifier, (object)rec.Value)).ToList();
            return tbl;
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            // Return just the values for the enumerator
            return _records.Select(r => r.Value).GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Represents a strongly typed FlatTable
    /// </summary>
    /// <typeparam name="T">The type used to represent data structure</typeparam>
    public sealed class FlatTable<T> : FlatTable, IEnumerable<T>
    {
        internal new List<FlatRecord<T>> _records = new List<FlatRecord<T>>();

        /// <summary>
        /// Creates a new FlatTable
        /// </summary>
        /// <param name="Name">The name of the table</param>
        public FlatTable(string Name)
            : base(Name, typeof(T))
        {
            //this.UnderlyingType = typeof(T);
        }

        /// <summary>
        /// Infers a generic FlatTable from a non-generic instance
        /// </summary>
        /// <param name="Table">The non-generic FlatTable</param>
        /// <returns>The generic FlatTable</returns>
        internal static FlatTable<T> InferFromFlatTable(FlatTable Table)
        {
            FlatTable<T> tbl = new FlatTable<T>(Table.Name);
            if (tbl.UnderlyingType == typeof(T))
            {
                tbl._records = (from rec in Table.Records select new FlatRecord<T>(rec.Identifier, (T)rec.Value)).ToList();
                return tbl;
            }
            else
            {
                throw new Exception("Cannot infer tables with different types");
            }
        }

        /// <summary>
        /// The records for the table
        /// </summary>
        public new List<FlatRecord<T>> Records
        {
            get {
                return _records;
            }
            internal set { _records = value; }
            
        }

        /// <summary>
        /// Adds a record to the table
        /// </summary>
        /// <param name="Value">The data to add to the table</param>
        public void AddRecord(T Value)
        {
            this._records.Add(new FlatRecord<T>(Value));
        }

        /// <summary>
        /// Removes a record from the table
        /// </summary>
        /// <param name="Identifier">The identifier for the record</param>
        public new void RemoveRecord(Guid Identifier)
        {
            FlatRecord<T> rec = _records.DefaultIfEmpty(null).FirstOrDefault(r => r.Identifier == Identifier);
            if (rec != null)
            {
                _records.RemoveAt(_records.IndexOf(rec));
            }
            else
            {
                throw new Exception("Record with Identifier '" + Identifier.ToString() + "' not found");
            }
        }


        #region IEnumerable<T> Members

        public new IEnumerator<T> GetEnumerator()
        {
            // return just the values for the enumerator
            return _records.Select(r => r.Value).GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Represents a record in a FlatTable
    /// </summary>
    /// <typeparam name="T">The type used to represent the data</typeparam>
    public sealed class FlatRecord<T>
    {
        /// <summary>
        /// Unique identifier for the record
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// The value
        /// </summary>
        public T Value { get; private set; }

        
        /// <summary>
        /// Creates a new FlatRecord
        /// </summary>
        /// <param name="Identifier">The Identifier</param>
        /// <param name="Value">The Value</param>
        internal FlatRecord(Guid Identifier, T Value)
        {
            this.Identifier = Identifier;
            this.Value = Value;
        }

        /// <summary>
        /// Creates a new FlatRecord
        /// </summary>
        /// <param name="Value">The Value</param>
        internal FlatRecord(T Value)
        {
            this.Identifier = Guid.NewGuid();
            this.Value = Value;
        }
    }
}
