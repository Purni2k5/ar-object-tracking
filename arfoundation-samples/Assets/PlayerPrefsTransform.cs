using System;
using coc;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
public class PlayerPrefsTransform : MonoBehaviour, ISettings
{
    public string id = "";
    private Vector3 position = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 scale = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        Settings.Instance.RegisterSettings(this);
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("id not set");
        }

        if (!PlayerPrefsGetVector(id + "Position", out position))
        {
            Debug.LogFormat("Could not find key in PlayerPrefs for {0} ", id + "Position");
        }
        else
        {
            transform.localPosition = position;
        }

        if (!PlayerPrefsGetVector(id + "Rotation", out rotation))
        {
            Debug.LogFormat("Could not find key in PlayerPrefs for {0} ", id + "Rotation");
        }
        else
        {
            transform.localRotation = Quaternion.Euler(rotation);
        }

        if (!PlayerPrefsGetVector(id + "Scale", out scale))
        {
            Debug.LogFormat("Could not find key in PlayerPrefs for {0} ", id + "Scale");
        }
        else
        {
            transform.localScale = scale;
        }
    }

    private void OnDestroy()
    {
        Settings.Instance.UnregisterSettings(this);
    }

    public void RenderSettings()
    {
        if (GUILayout.Button(string.Format("Set Position {0}", position)))
        {
            transform.localPosition = position;
        }
        
        if (GUILayout.Button(string.Format("Set Rotation {0}", transform.localRotation.eulerAngles)))
        {
            transform.localRotation = Quaternion.Euler(rotation);
        }
        
        if (GUILayout.Button(string.Format("Set Scale {0}", transform.localScale)))
        {
            transform.localScale = scale;
        }
        
        if (GUILayout.Button(string.Format("Save Position {0}", transform.localPosition)))
        {
            PlayerPrefsSetVector(id + "Position", transform.localPosition);
        }
        
        if (GUILayout.Button(string.Format("Save Rotation {0}", transform.localRotation.eulerAngles)))
        {
            PlayerPrefsSetVector(id + "Rotation", transform.localRotation.eulerAngles);
        }
        
        if (GUILayout.Button(string.Format("Save Scale {0}", transform.localScale)))
        {
            PlayerPrefsSetVector(id + "Scale", transform.localScale);
        }
    }
    
    public string GetSettingsUid()
    {
        return id + " " + "PlayerPrefsTransform";
    }

    public static bool PlayerPrefsGetVector(string id, out Vector3 value)
    {
        
        if (PlayerPrefs.HasKey(id + "X"))
        {
            value.x = PlayerPrefs.GetFloat(id + "X");
        }
        else
        {
            value = Vector3.zero;
            return false;
        }
        if (PlayerPrefs.HasKey(id + "Y"))
        {
            value.y = PlayerPrefs.GetFloat(id + "Y");
        }else
        {
            value = Vector3.zero;
            return false;
        }
        if (PlayerPrefs.HasKey(id + "Z"))
        {
            value.z = PlayerPrefs.GetFloat(id + "Z");
        }else
        {
            value = Vector3.zero;
            return false;
        }
        PlayerPrefs.Save();
        return true;
    }

    public static void PlayerPrefsSetVector(string id, Vector3 value)
    {
        PlayerPrefs.SetFloat(id + "X", value.x);
        PlayerPrefs.SetFloat(id + "Y", value.y);
        PlayerPrefs.SetFloat(id + "Z", value.z);
        PlayerPrefs.Save();
    }
}
