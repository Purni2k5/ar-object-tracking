using System;
using System.Collections;
using System.Collections.Generic;
using coc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelector : MonoBehaviour, ISettings
{
    private int sceneCount = 0;
    private void Start()
    {
        Settings.Instance.RegisterSettings(this);
        sceneCount = SceneManager.sceneCountInBuildSettings;
    }

    private void OnDestroy()
    {
        Settings.Instance.UnregisterSettings(this);
    }

    public void RenderSettings()
    {
        for (int i = 1; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(i);
            if (GUILayout.Button(string.Format("Scene {0}", i)))
            {
                SceneManager.LoadScene(i);
            }
        }
    }

    public string GetSettingsUid()
    {
        return "SceneSelector";
    }
}
