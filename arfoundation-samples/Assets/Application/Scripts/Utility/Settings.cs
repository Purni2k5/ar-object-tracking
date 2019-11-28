/*
 
//Can use both UI and Parameters or just one or the other

//UI Usage:
- Inherit from ISettings
- Implement GetSettingsUid() to return unique string name
- Implement RenderSettings() with imgui code inside
coc.Settings.Instance.RegisterSettings(this);

//Params Usage:
coc.Settings.Instance.RegisterParam(this,"your_key",your_instance);
coc.Settings.Instance.Load(this);
 
 */

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;


namespace coc
{

    public interface ISettings
    {
        void RenderSettings();
        string GetSettingsUid();
    }

    public class Settings : MonoBehaviour
    {

        #region Singleton

        private static Settings instance;

        public static Settings Instance
        {
            get { return instance; }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                Debug.Log("Another coc.Settings() attempted to be created and was destroyed.");
            }
            else
            {
                instance = this;
                mapSettings = new Dictionary<string, ISettings>();
                mapParams = new Dictionary<ISettings, coc.Parameters>();
                selectionStrings = new List<string>();
            }

            DontDestroyOnLoad(this.gameObject);
        }

        #endregion

        public bool showGui = false;
        public Rect windowRect = new Rect(25, 20, 600, 600);

        private Dictionary<string, ISettings> mapSettings;
        private Dictionary<ISettings, coc.Parameters> mapParams;
        private int selectionIndex = 0;
        private List<string> selectionStrings;
        public void RegisterSettings(ISettings settings)
        {
            string name = settings.GetSettingsUid();
            Debug.Assert(!System.String.IsNullOrEmpty(name));
            mapSettings.Add(name, settings);
            selectionStrings.Add(name);
        }

        private void AddParameters( ISettings settings )
        {
            var parameters = new coc.Parameters();            
            string folderPath = Path.Combine(Application.streamingAssetsPath,"settings/");
            System.IO.Directory.CreateDirectory(folderPath);
            parameters.SetFilePath(Path.Combine(folderPath,"settings_" + settings.GetSettingsUid() + ".xml"));
            mapParams.Add(settings, parameters);
        }

        public void UnregisterSettings(ISettings settings)
        {            
            selectionStrings.Remove(settings.GetSettingsUid());
            mapParams.Remove( mapSettings[settings.GetSettingsUid()] );
            mapSettings.Remove( settings.GetSettingsUid() );
        }

        public bool showWindow = true;
            
        private void OnGUI()
        {
            if (!showGui || selectionStrings.Count == 0) return;
            ResizeGUI(30);
            if (GUILayout.Button("Toggle GUI Window"))
            {
                showWindow = !showWindow;
            }

            selectionIndex = GUI.SelectionGrid(new Rect(0, 40, 300, 30 * selectionStrings.Count), selectionIndex, selectionStrings.ToArray(), 1);
            if (showWindow)
            {
                windowRect = GUI.Window(0, windowRect, WindowFunction, selectionStrings[selectionIndex]);
            }
        }
        
        private void WindowFunction(int windowID)
        {
            var settings = mapSettings[selectionStrings[selectionIndex]];
            GUILayout.Space(40);
            
            if (mapParams.ContainsKey(settings))
            {
                if (GUILayout.Button("Save"))
                {
                    mapParams[mapSettings[selectionStrings[selectionIndex]]].Save();
                }
                else if (GUILayout.Button("Load"))
                {
                    mapParams[mapSettings[selectionStrings[selectionIndex]]].Load();
                }
            }


            mapSettings[selectionStrings[selectionIndex]].RenderSettings();

            GUI.DragWindow();
        }
              
        private void ResizeGUI(int fontSize)
        {
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.button.fontSize = fontSize;
            GUI.skin.toggle.fontSize = fontSize;
            GUI.skin.window.fontSize = fontSize;
            GUI.skin.window.contentOffset = new Vector2(0,0);
            //todo: add other elements as required
        }
        
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftApple) || Input.GetKey(KeyCode.RightApple))
            {
                if (Input.GetKeyDown(KeyCode.D))
                {
                    showGui = !showGui;
                }

            }
        }

        #region Params

        //only here to catch people trying to add value types
        public void RegisterParam(ISettings settings, string name, System.ValueType val)
        {
            mapParams[settings].RegisterParam(name, val);
        }

        public void RegisterParam(ISettings settings, string name, System.Object val)
        {
            if (!mapParams.ContainsKey(settings)) AddParameters(settings);
            mapParams[settings].RegisterParam(name, val);
        }

        public void Load(ISettings settings)
        {
            mapParams[settings].Load();
        }
        
        public void SAve(ISettings settings)
        {
            mapParams[settings].Save();
        }

        public void LoadAll()
        {
            foreach (var entry in mapParams)
            {
                entry.Value.Load();
            }
        }
        
        public void SaveAll()
        {
            foreach (var entry in mapParams)
            {
                entry.Value.Save();
            }
        }

        #endregion


    }

}//namespace coc