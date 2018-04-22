// Date   : #CREATIONDATE#
// Project: #PROJECTNAME#
// Author : #AUTHOR#

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    [SerializeField]
    List<Level> levels;

    LevelManager levelManager;

    [SerializeField]
    CanvasGroup DeathScreen;
    [SerializeField]
    CanvasGroup LevelScreen;
    [SerializeField]
    CanvasGroup EndScreen;
    [SerializeField]
    CanvasGroup QuitScreen;

    void Start () {
        levelManager = new LevelManager(this, levels);
        levelManager.LoadNextLevel();

        DeathScreen.alpha = 0.0f;
    }

    void Update () {

        if (fadeInQuit)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ConfirmQuit(true);
            }
            else if (Input.anyKeyDown)
            {
                ConfirmQuit(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
            if (ended)
            {
                Application.Quit();
            }
        }
        
        fadeCanvas();
    }

    public void triggerDeath()
    {
        fadeDeath = true;
    }

    public void triggerRestart()
    {
        DeathScreen.alpha = 0.0f;
        fadeDeath = false;
        levelManager.ReloadLevel();
    }

    public void triggerNext()
    {
        if (levelManager.hasNextLevel())
        {
            fadeLevelOut = true;
        }
        else
        {
            fadeEnd = true;
            ended = true;
        }
    }

    float FADE_TIME = 0.5f;
    bool fadeDeath = false;

    bool fadeLevelIn = true;
    bool fadeLevelOut = false;

    bool fadeEnd = false;

    bool fadeInQuit = false;
    bool fadeOutQuit = false;

    bool ended = false;

    void fadeCanvas()
    {
        if (fadeDeath)
        {
            if (DeathScreen.alpha < 1.0f)
            {
                DeathScreen.alpha += Time.deltaTime / FADE_TIME;
            }

            if (DeathScreen.alpha > 1.0f)
            {
                DeathScreen.alpha = 1.0f;
            }
        }

        if (fadeLevelIn)
        {
            LevelScreen.alpha -= Time.deltaTime / FADE_TIME;

            if (LevelScreen.alpha <= 0.0f)
            {
                LevelScreen.alpha = 0.0f;
                fadeLevelIn = false;
            }
        }


        if (fadeLevelOut)
        {
            LevelScreen.alpha += Time.deltaTime / FADE_TIME;

            if (LevelScreen.alpha >= 1.0f)
            {
                LevelScreen.alpha = 1.0f;
                fadeLevelOut = false;
                levelManager.LoadNextLevel();
                fadeLevelIn = true;
            }
        }

        if (fadeEnd)
        {
            if (EndScreen.alpha < 1.0f)
            {
                EndScreen.alpha += Time.deltaTime / FADE_TIME;
            }

            if (EndScreen.alpha > 1.0f)
            {
                EndScreen.alpha = 1.0f;
            }
        }

        if (fadeOutQuit)
        {
            QuitScreen.alpha -= Time.deltaTime / FADE_TIME;

            if (QuitScreen.alpha <= 0.0f)
            {
                QuitScreen.alpha = 0.0f;
                fadeOutQuit = false;
            }
        }


        if (fadeInQuit)
        {
            QuitScreen.alpha += Time.deltaTime / FADE_TIME;

            if (QuitScreen.alpha >= 1.0f)
            {
                QuitScreen.alpha = 1.0f;
            }
        }
    }

    public void Quit()
    {
        fadeInQuit = true;
        fadeOutQuit = false;
    }

    public void ConfirmQuit(bool confirm)
    {
        if (confirm && fadeInQuit)
        {
            Application.Quit();
            return;
        }
        fadeInQuit = false;
        fadeOutQuit = true;
    }
}
