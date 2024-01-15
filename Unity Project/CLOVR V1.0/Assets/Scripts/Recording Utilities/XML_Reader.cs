using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

namespace XRT_OVR_Grabber
{
    public class XML_Reader
    {
        public Questionnaire Load_XML_Questionnaire(string filename)
        {

            //TODO: Figure out the XML Loading.
            //Serializer setup. 
            //Debug.Log(filename);
            XmlSerializer SerializerXML = new XmlSerializer(typeof(Questionnaire));
            SerializerXML.UnknownNode += new XmlNodeEventHandler(_SerializerUnknownNode);
            SerializerXML.UnknownAttribute += new XmlAttributeEventHandler(_SerializerUnknownAttribuite);

            //Debug.Log(filename);

            //Load file and read.
            FileStream fs = new FileStream(filename, FileMode.Open);
            Questionnaire q = (Questionnaire) SerializerXML.Deserialize(fs);
            return q;
        }

        public ProjectSettings Load_XML_ProjectSettings(string filename)
        {

            //TODO: Figure out the XML Loading.
            //Serializer setup. 
            XmlSerializer SerializerXML = new XmlSerializer(typeof(ProjectSettings));
            SerializerXML.UnknownNode += new XmlNodeEventHandler(_SerializerUnknownNode);
            SerializerXML.UnknownAttribute += new XmlAttributeEventHandler(_SerializerUnknownAttribuite);

            //Debug.Log(filename);

            //Load file and read.
            FileStream fs = new FileStream(filename, FileMode.Open);
            ProjectSettings q = (ProjectSettings) SerializerXML.Deserialize(fs);
            return q;
        }

        public void Write_XML_Questtionaire(Questionnaire _qFileOut, string fileLocation)
        {
            //Serializer setup. 
            XmlSerializer SerializerXML = new XmlSerializer(typeof(Questionnaire));
            SerializerXML.UnknownNode += new XmlNodeEventHandler(_SerializerUnknownNode);
            SerializerXML.UnknownAttribute += new XmlAttributeEventHandler(_SerializerUnknownAttribuite);

            //Load file and read.
            TextWriter writer = new StreamWriter(fileLocation);
            SerializerXML.Serialize(writer, _qFileOut);
        }

        public void Write_XML_ProjectSettings(ProjectSettings _qFileOut, string fileLocation)
        {
            //Serializer setup. 
            XmlSerializer SerializerXML = new XmlSerializer(typeof(ProjectSettings));
            SerializerXML.UnknownNode += new XmlNodeEventHandler(_SerializerUnknownNode);
            SerializerXML.UnknownAttribute += new XmlAttributeEventHandler(_SerializerUnknownAttribuite);

            //Load file and read.
            TextWriter writer = new StreamWriter(fileLocation);
            SerializerXML.Serialize(writer, _qFileOut);
        }



        void _SerializerUnknownNode(object sender, XmlNodeEventArgs e)
        {
            Debug.LogError("Error with node: " + e.ToString());
        }
        void _SerializerUnknownAttribuite(object sender, XmlAttributeEventArgs e)
        {
            Debug.LogError("Unknown Attribuite error: " + e.ToString());
        }
    }
}