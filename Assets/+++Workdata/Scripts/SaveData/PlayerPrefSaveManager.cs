using System;
using UnityEngine;

public class PlayerPrefSaveManager : MonoBehaviour
{
    public string playerPrefKey = "default";

    public GameData gameData;


    public int saveSlotIndex;

    private void Start()
    {
        if (PlayerPrefs.HasKey(playerPrefKey))
        {
            gameData = JsonManager.LoadFromPlayerPref<GameData>(playerPrefKey);
            LoadGameData();
            return;
        }
    }

    [ContextMenu("Delete All PlayerPrefs")]
    public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    
    
    public void SaveGameData()
    {
        Vector3 currentPlayerPos = GameObject.FindAnyObjectByType<PlayerController>().transform.position;
        gameData.saveSlots[saveSlotIndex].playerPos = currentPlayerPos;
        
        JsonManager.SaveToPlayerPref(playerPrefKey, gameData);
    }

    public void LoadGameData()
    {
        Vector3 newPlayerPos = gameData.saveSlots[saveSlotIndex].playerPos;
        GameObject.FindAnyObjectByType<PlayerController>().transform.position = newPlayerPos;
    }
    
}