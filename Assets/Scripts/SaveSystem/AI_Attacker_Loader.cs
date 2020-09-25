using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AI_Attacker_Loader : MonoBehaviour
{
    //used for spawning
    [SerializeField] private GameObject[] aiAttackerPrefabs;

    //all default scene harvestables; they must be destroyed if a save file exists in order to avoid duplicates
    [SerializeField] private GameObject defaultSceneResourcesPrefab;

    //all harvestables; non-serializable list
    public static List<AI_Attacker> allSpawnedAttackers;

    //copy of all important harvestable stats; serializable list
    public static List<AI_Attacker_Serializable> allSpawnedAttackersSerializable;

    private void Awake()
    {
        allSpawnedAttackers = new List<AI_Attacker>();
        allSpawnedAttackersSerializable = new List<AI_Attacker_Serializable>();
    }

    private void Start()
    {
        if (Chochosan.SaveLoadManager.IsSaveExists())
        {
            Destroy(defaultSceneResourcesPrefab);
            foreach (AI_Attacker_Serializable acs in Chochosan.SaveLoadManager.savedGameData.attackerList)
            {
                //load the position
                Vector3 tempPos;
                tempPos.x = acs.x;
                tempPos.y = acs.y;
                tempPos.z = acs.z;

                //spawn the correct object based on the saved index
                GameObject tempAttacker = Instantiate(aiAttackerPrefabs[acs.attackerIndex], tempPos, Quaternion.identity);

                //set specific controller stats
                AI_Attacker tempController = tempAttacker.GetComponent<AI_Attacker>();
                tempController.SetAttackerHP(acs.currentHP);
                tempController.SetAttackerIndex(acs.attackerIndex);

                //add the object to the list to allow further saving overriding
                AddAttackerToList(tempController);
            }
        }
    }

    //an object has been spawned (no matter where and how)? -> add it to the list to allow it to be saved later on
    public static void AddAttackerToList(AI_Attacker aiAttacker)
    {
        allSpawnedAttackers.Add(aiAttacker);
    }

    //an object has been destroyed (no matter where and how)? -> remove it from the list to avoid sleepless nights
    public static void RemoveAttackerFromList(AI_Attacker aiAttacker)
    {
        allSpawnedAttackers.Remove(aiAttacker);
    }

    public static void AddSerializableAttackerToList(AI_Attacker_Serializable aiAttackerSerializable)
    {
        allSpawnedAttackersSerializable.Add(aiAttackerSerializable);
    }

    //All harvestables in the world are in a list that is not serializable. Looping through that list a serializable copy of the 
    //harvestable controller is created and put in a serializable list. That list is then sent to the SaveLoadManager and extracted
    //later in order to load the harvestables save.
    public static List<AI_Attacker_Serializable> GetAttackers()
    {
        foreach (AI_Attacker aiAttacker in AI_Attacker_Loader.allSpawnedAttackers)
        {
            AI_Attacker_Serializable acs = aiAttacker.GetAttackerData();
            AI_Attacker_Loader.AddSerializableAttackerToList(acs);
        }
        return AI_Attacker_Loader.allSpawnedAttackersSerializable;
    }
}

