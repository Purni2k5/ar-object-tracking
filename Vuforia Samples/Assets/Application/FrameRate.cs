using UnityEngine;

public class FrameRate : MonoBehaviour
{
    private float deltaTime = 0;
    private float msec = 0;
    private float fps = 0;

    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        msec = deltaTime * 1000.0f;
        fps = 1.0f / deltaTime;
    }

    private void OnGUI()
    {
        GUILayout.Label(string.Format("fps: {0}", fps));
    }
}
