using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public delegate void OnInventoryValueChangedDelegate(string valueName, float value);
    public event OnInventoryValueChangedDelegate OnInventoryValueChanged;

    public float CurrentWood
    {
        get
        {
            return currentWood;
        }
        set
        {
            currentWood = value;
            OnInventoryValueChanged?.Invoke("wood", currentWood);
        }
    }
    private float currentWood;
}
