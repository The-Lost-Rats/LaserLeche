using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public struct UFOData
    {
        public int startingCell;
        public float startingBackgroundYPos;
        public bool moving;
        public int movementCellA;
        public int movementCellB;
        public bool spawnProbes;
    }

    public List<UFOData> ufoList;
    [Range(0.0f, 30.0f)]
    public float timeToNextUFO;
    public Sprite waveNumImage;
}
