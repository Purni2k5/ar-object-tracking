using System;
using System.Collections;
using System.Collections.Generic;
using coc;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TestObjectTracking : MonoBehaviour, ISettings
{
    public ARTrackedObjectManager objectManager;
    private int objectTrackableCount = 0;
    private int objectReferenceLibraryCount = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        Settings.Instance.RegisterSettings(this);
    }

    private void OnDestroy()
    {
        Settings.Instance.UnregisterSettings(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (objectManager)
        {
            objectTrackableCount = objectManager.trackables.count;
            
            if (objectManager.referenceLibrary)
            {
                objectReferenceLibraryCount = objectManager.referenceLibrary.count;
            }
        }
    }

    public void RenderSettings()
    {
        GUILayout.Label(string.Format("objectTrackableCount {0}", objectTrackableCount));
        GUILayout.Label(string.Format("objectReferenceLibraryCount {0}", objectReferenceLibraryCount));
    }

    public string GetSettingsUid()
    {
        return "Test Object Tracking";
    }
}
