using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

public class DataController : MonoBehaviour
{
	public SaveData Configurations;
    public string FileName;

    private string fileLocation;
    private string data;

	void Start()
	{
		fileLocation = Application.dataPath;
        LoadXML();
    }

    public void CreateXML()
    {
        data = XMLController.SerializeObject(Configurations, typeof(SaveData));

        StreamWriter writer;
        FileInfo t = new FileInfo(fileLocation + "\\" + FileName + ".xml");

        Debug.Log(fileLocation);

        if (!t.Exists)
        {
            writer = t.CreateText();
        }
        else
        {
            t.Delete();
            writer = t.CreateText();
        }
        writer.Write(data);
        writer.Close();
        Debug.Log("Created");
    }

    public void LoadXML()
    {
        try
        {
            StreamReader r = File.OpenText(fileLocation + "\\" + FileName + ".xml");
            string _info = r.ReadToEnd();
            r.Close();
            data = _info;

            if (data.ToString() != "")
            {
                Configurations = (SaveData)XMLController.DeserialzeObject(data, typeof(SaveData));
            }

            Debug.Log("Loaded");
        }
        catch
        {
            CreateXML();
        }
    }
}

[System.Serializable]
public class SaveData
{
	public List<Setup> Setups = new List<Setup>();
}

[System.Serializable]
public class Setup
{
	public string Name; //All Settings Go Here
}