using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

public static class _extensions
{
    public static byte[] Ser(this object obj)
    {
        var ser = new XmlSerializer(obj.GetType());
        using (MemoryStream ms = new MemoryStream())
        {
            ser.Serialize(ms, obj);
            byte[] b = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(b, 0, b.Length);
            return b;
        }
    }

    public static object DeSer(this byte[] data, Type Type)
    {
        var ser = new XmlSerializer(Type);
        using (MemoryStream ms = new MemoryStream(data))
        {
            return ser.Deserialize(ms);
        }
    }

    public static object DeSer(this string data, Type type)
    {
        return DeSer(Encoding.UTF8.GetBytes(data), type);
    }
}