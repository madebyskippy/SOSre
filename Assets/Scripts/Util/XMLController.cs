using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

public static class XMLController
{
    public static string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters);
        return constructedString;
    }

    public static byte[] StringToUTF8ByteArray(string pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }

    public static string SerializeObject(object pObject, Type classType)
    {
        string XmlizedString = null;

        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer xS = new XmlSerializer(classType);
        XmlTextWriter xTW = new XmlTextWriter(memoryStream, Encoding.UTF8);
        xS.Serialize(xTW, pObject);
        memoryStream = (MemoryStream)xTW.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());

        return XmlizedString;
    }

    public static object DeserialzeObject(string pXmlizedString, Type classType)
    {
        XmlSerializer xS = new XmlSerializer(classType);
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        XmlTextWriter xTW = new XmlTextWriter(memoryStream, Encoding.UTF8);
        return xS.Deserialize(memoryStream);
    }
}
