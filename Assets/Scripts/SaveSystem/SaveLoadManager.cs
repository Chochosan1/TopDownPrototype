using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Chochosan
{
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveData savedGameData;
        private void Awake()
        {
            savedGameData = LoadGameState();
            Debug.Log(savedGameData);
        }

        public static void SaveGameState()
        {
            SeriouslyDeleteAllSaveFiles();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.OpenOrCreate);
            SaveData saveData = new SaveData();
            bf.Serialize(file, saveData);
            file.Close();
            Debug.Log("Saved");
        }

        public static SaveData LoadGameState()
        {
            if (File.Exists(Application.persistentDataPath + "/save.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.Open);
                SaveData saveData = (SaveData)bf.Deserialize(file);
                Debug.Log(saveData.buildingList);
                file.Close();
                return saveData;
            }
            return null;
        }

        public static void SeriouslyDeleteAllSaveFiles()
        {
            string path = Application.persistentDataPath;
            DirectoryInfo directory = new DirectoryInfo(path);
            directory.Delete(true);
            Directory.CreateDirectory(path);
        }

        public static bool IsSaveExists()
        {
            if(savedGameData != null)
            {
                Debug.Log("GOT YA");
                return true;
            }
            Debug.Log("PROBLEM");
            return false;             
        }


        [System.Serializable]
        public class SaveData
        {
            public InventorySaveData inventorySaveData = PlayerInventory.Instance.GetInventory();
            public List<BuildingControllerSerializable> buildingList = ObjectSpawner.Instance.GetBuildingsInfo();
        }
    }
}
