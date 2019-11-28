/*
 
//Usage for value types:
var i = new Param<int>(); // or  var i = new Param<int>(1);
i.Val = 1;
parameters.RegisterParam("i_key",i);

//Usage for reference types:
var c = new MyClass();
parameters.RegisterParam("c_key",c);

parameters.save("/path/file.xml");

parameters.load("/path/file.xml");
 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace coc
{
    
    #region Param class

    public class ParamBase
    {
        public string Type = "";
    }

    public class Param<T> : ParamBase
    {
        public Param()
        {
            Type = Val.GetType().Name;
        }

        public Param(T val)
        {
            Val = val;
            Type = Val.GetType().Name;
        }

        public T Val;

    }
    
    #endregion

    public class Parameters
    {
        private Dictionary<string, System.Object> map;
        private string defaultFilePath = "config.xml";

        public Parameters()
        {
            map = new Dictionary<string, System.Object>();
        }

        public void SetFilePath(string filePath)
        {
            defaultFilePath = filePath;
        }

        
        public void RegisterParam(string name, System.ValueType val)
        {
            Debug.LogError("Must pass value type '" + val.GetType().Name + "' in as Parameter<T>()");
        }

        
        public void RegisterParam(string name, System.Object val)
        {
            if (map.ContainsKey(name))
            {
                Debug.LogError("Key already exists, ignoring!");
                return;
            }

            string typename = val.GetType().Name;
            if (typename == "String") //because string not caught by System.ValueType
            {
                Debug.LogError("Must pass value type '" + val.GetType().Name + "' in as Parameter<T>()");
                return;
            }

            switch (typename)
            {
                case "Param`1":
                    AddValue(name, (ParamBase) val);
                    break;
//                case "EgClass":
//                    map.Add(name, val);
//                    break;
                default:
                    Debug.LogError("Unsupported object Type: " + typename);
                    break;
            }
        }

        
        public void Save(string filePath = "")
        {
            if (filePath.Length == 0) filePath = defaultFilePath;
            
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] {XmlQualifiedName.Empty});
            var writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writerSettings.OmitXmlDeclaration = true;
            writerSettings.IndentChars = "    ";

            using (XmlWriter writer = XmlWriter.Create(filePath, writerSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("pairs");

                foreach (KeyValuePair<string, System.Object> entry in map)
                {
                    writer.WriteStartElement("pair");

                    writer.WriteElementString("key", entry.Key);

                    writer.WriteStartElement("serial_value");
                    var serializer = new XmlSerializer(entry.Value.GetType());
                    serializer.Serialize(writer, entry.Value, emptyNamepsaces);
                    writer.WriteEndElement(); //serial_value

                    writer.WriteEndElement(); //pair
                }

                writer.WriteEndElement(); //pairs
                writer.WriteEndDocument();

            }

        }


        public void Load(string filePath = "")
        {
            if (filePath.Length == 0) filePath = defaultFilePath;

            if (!File.Exists(filePath))
            {
                Debug.LogWarning("File not found, creating " + filePath);
                Save(filePath);
            }

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(filePath);

            var pairs = doc.GetElementsByTagName("pair");
            foreach (XmlNode pair in pairs)
            {

                string key = pair.SelectSingleNode("key").InnerText;
                string xml = pair.SelectSingleNode("serial_value").InnerXml;

                if (!map.ContainsKey(key))
                {
                    Debug.LogError("Skipping missing key: " + key);
                    continue;
                }


                //can not get reflection working so implement each class explicitly
                string typename = map[key].GetType().Name;

                if (typename == "Param`1") //Param<T>()
                {
                    string typefield = pair.SelectSingleNode(".//Type").InnerText;
                    switch (typefield)
                    {
                        case "Boolean":
                            CopyValue<bool>(key, xml);
                            break;
                        case "String":
                            CopyValue<string>(key, xml);
                            break;
                        case "Single":
                            CopyValue<float>(key, xml);
                            break;
                        case "Int32":
                            CopyValue<int>(key, xml);
                            break;
                        default:
                            Debug.LogError("Param type not implemented in load(): " + typename);
                            break;
                    }
                }
                else
                {
                    XmlSerializer serializer = new XmlSerializer(map[key].GetType());
                    StringReader reader = new StringReader(xml);

                    switch (typename)
                    {
//                        case "EgClass":
//                            var dst = (EgClass) map[key];
//                            var src = (EgClass) serializer.Deserialize(reader);
//                            dst.egField = src.egField;
//                            break;
                        default:
                            Debug.LogError(typename + "class not implemented in load!");
                            break;
                    }
                }


            }

        }
        
        
        private void AddValue(string name, ParamBase val)
        {
            switch (val.Type)
            {
                case "":
                    Debug.LogError("Parameter type not set!");
                    break;
                case "Boolean":
                case "Single":
                case "Int32":
                case "String":
                    map.Add(name, val);
                    break;
                default:
                    Debug.LogError("Unimplemented parameter type!");
                    break;
            }
        }

        
        private void CopyValue<T>(string key, string xml)
        {
            XmlSerializer serializer = new XmlSerializer(map[key].GetType());
            StringReader reader = new StringReader(xml);
            var src = (Param<T>) serializer.Deserialize(reader);
            var dst = (Param<T>) map[key];
            dst.Val = src.Val;
        }


        //todo: make optional singleton
        
    }

}//namespace coc