
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager
{
    
    private List<Level> levels;

    private Level currentLevel;

    private bool initialized = false;

    [SerializeField]
    private int currentLevelNumber = 0;

    private GameManager main;

    public LevelManager(GameManager main, List<Level> levels)
    {
        this.main = main;
        this.levels = levels;
    }

    public void Initialize()
    {
        initialized = true;
    }

    public bool hasNextLevel()
    {
        return levels.Count > currentLevelNumber + 1;
    }

    public bool LoadNextLevel()
    {
        if (!initialized)
        {
            Initialize();
        }
        else
        {
            currentLevelNumber += 1;
        }
        if (levels.Count > currentLevelNumber)
        {
            LoadLevel(currentLevelNumber);
            return true;
        }
        Debug.Log("The end!");
        return false;
    }

    public void ReloadLevel()
    {
        LoadLevel(currentLevelNumber);
    }

    private void LoadLevel(int levelNumber)
    {
        if (currentLevel != null)
        {
            currentLevel.Unload();
        }
        currentLevel = Object.Instantiate(levels[levelNumber]);
        currentLevel.transform.SetParent(main.transform, false);
        currentLevel.Load();
    }
}
