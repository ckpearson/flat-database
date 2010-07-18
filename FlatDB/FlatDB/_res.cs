using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlatDB
{
    public static class _res
    {
        public static string DatabaseNotFoundMessage = "Database file '{0}' not found";
        public static string TableOfTypeNotFoundMessage = "Table of type '{0}' not found";

        public static string xmlRootName = "FlatDB";
        public static string xmlTablesSectionName = "Tables";
        public static string xmlTableSectionName = "Table";
        public static string xmlTableNameAttribute = "Name";
        public static string xmlTableTypeAttribute = "Type";
        public static string xmlRecordsSectionName = "Records";
        public static string xmlRecordSectionName = "Record";
        public static string xmlRecordIdentAttribute = "Ident";
    }
}
