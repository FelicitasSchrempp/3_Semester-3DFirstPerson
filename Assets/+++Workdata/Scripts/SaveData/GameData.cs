using System;
using UnityEngine;

[Serializable]
public class GameData
{
    public SettingsData settingsData;
    public SaveSlot[] saveSlots = new SaveSlot[4];
}

[Serializable]
public class SettingsData
{
    public float masterSoundValue = 0.75f;
    public bool fullscreenMode;
    public float motionBlurValue;
    public float brightnessValue;
}

[Serializable]
public class SaveSlot
{
    public Vector3 playerPos;
}