using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PhoneSaveSystem : MonoBehaviour
{
  // Makes it a singleton / single instance
  static public PhoneSaveSystem instance;
  string filePath;

  private void Awake()
  {
    // Check there are no other instances of this class in the scene
    if (!instance)
    {
      instance = this;
    }
    //else
   // {
    //  Destroy(phoneObject);
    //}

    filePath = Application.persistentDataPath + "/Savedphone.duck";
  }

  public void Savephone(phoneData saveData)
  {
    FileStream dataStream = new FileStream(filePath, FileMode.Create);

    BinaryFormatter converter = new BinaryFormatter();
    converter.Serialize(dataStream, saveData);

    dataStream.Close();
  }

  public phoneData Loadphone()
  {
    if(File.Exists(filePath))
    {
      // File exists 
      FileStream dataStream = new FileStream(filePath, FileMode.Open);

      BinaryFormatter converter = new BinaryFormatter();
      phoneData saveData = converter.Deserialize(dataStream) as phoneData;

      dataStream.Close();
      return saveData;
    }
    else
    {
      // File does not exist
      Debug.LogError("Save file not found in " + filePath);
      return null;
    }
  }
}