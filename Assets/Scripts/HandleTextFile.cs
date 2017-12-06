using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class HandleTextFile : MonoBehaviour {


    public static void WriteString(string name, string str){
        string path = "Assets/Resources/Data/" + name + ".txt";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(str);
        writer.Close();
    }

    public static string ReadString(string name)
    {
        string path = "Assets/Resources/Data/"+name+".txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        //Debug.Log(reader.ReadToEnd());
        string read = reader.ReadToEnd();
        reader.Close();

        return read;
    }
}
