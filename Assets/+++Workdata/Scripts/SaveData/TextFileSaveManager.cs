using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class TextFileSaveManager : MonoBehaviour
{
    public string path;
    public string fileName = "default";

    public GameData gameData;


    public int saveSlotIndex;

    private string _currentPath;
    
    private void Awake()
    {
        path = Application.persistentDataPath + "/";
        _currentPath = path + fileName;
    }

    private void Start()
    {
        if (File.Exists(_currentPath))
        {
            LoadGameData();
            return;
        }
    }

    [ContextMenu("Delete Text File")]
    public void DeleteTextfile()
    {
        if (!_currentPath.Contains(Application.persistentDataPath)) return;
        
        File.Delete(_currentPath);
    }
    
    
    public void SaveGameData()
    {
        Vector3 currentPlayerPos = GameObject.FindAnyObjectByType<PlayerController>().transform.position;
        gameData.saveSlots[saveSlotIndex].playerPos = currentPlayerPos;
        
        JsonManager.SaveToTextFile(_currentPath, gameData);
    }

    [ContextMenu("Load Text File")]
    public void LoadGameData()
    {
        gameData = JsonManager.LoadFromTextFile<GameData>(_currentPath);
        CharacterController characterController = FindAnyObjectByType<CharacterController>();

        characterController.enabled = false;
        
        Vector3 newPlayerPos = gameData.saveSlots[saveSlotIndex].playerPos;
        GameObject.FindAnyObjectByType<PlayerController>().transform.position = newPlayerPos;
        
        characterController.enabled = true;
    }
    
}