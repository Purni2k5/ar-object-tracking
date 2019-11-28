using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsSizer : MonoBehaviour, coc.ISettings
{   
    public coc.Param<int> fontSize = new coc.Param<int>();
    private static SettingsSizer instance;

    public static SettingsSizer Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            Debug.Log("Another SettingsSizer attempted to be created and was destroyed.");
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }
    
    void Start(){
        
        coc.Settings.Instance.RegisterSettings(this);
        coc.Settings.Instance.RegisterParam(this,"font_size",fontSize);
        coc.Settings.Instance.Load(this);
    }
    
    public void RenderSettings(){
        
        GUILayout.Label(string.Format("fontSize: {0}",fontSize.Val));        
        fontSize.Val = (int)GUILayout.HorizontalSlider(fontSize.Val, 0, 100);
    }

    public string GetSettingsUid(){
        return "Settings Sizer";
    }

    private void OnDestroy()
    {
        if(coc.Settings.Instance)
            coc.Settings.Instance.UnregisterSettings(this);
    }
}
