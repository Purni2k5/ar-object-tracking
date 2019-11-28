using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSample : MonoBehaviour
{
    public GameObject modelGo;

    public GameObject cameraGo;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
//        GUILayout.Label(string.Format("Camera Position {0}", cameraGo.transform.position));
//        GUILayout.Label(string.Format("Model Position {0}", modelGo.transform.position));
    }

    private void OnDrawGizmos()
    {
       Gizmos.DrawLine(cameraGo.transform.position, modelGo.transform.position);
    }
}
