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
        }

        public static void SaveGameState()
        {
         //   SeriouslyDeleteAllSaveFiles();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.Create);
            SaveData saveData = new SaveData();
            bf.Serialize(file, saveData);
            file.Close();
            Debug.Log("Saved");
        }

        public static SaveData LoadGameState()
        {
            if (File.Exists(Application.persistentDataPath + "/save.dat"))
            {
                Chochosan.ChochosanHelper.ChochosanDebug("LOADING FROM SAVE...", "green");
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.Open);
                SaveData saveData = (SaveData)bf.Deserialize(file);
                file.Close();
                return saveData;
            }
            Chochosan.ChochosanHelper.ChochosanDebug("SAVE NOT FOUND. LOADING DEFAULT LEVEL...", "red");
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
                return true;
            }
            return false;             
        }


        [System.Serializable]
        public class SaveData
        {
            public InventorySaveData inventorySaveData = PlayerInventory.Instance.GetInventory();
            public List<BuildingControllerSerializable> buildingList = ObjectSpawner.Instance.GetBuildingsInfo();
            public List<HarvestableControllerSerializable> harvestableList = HarvestableLoader.GetHarvestables();
            public List<AI_Attacker_Serializable> attackerList = AI_Attacker_Loader.GetAttackers();
        }
    }
}
