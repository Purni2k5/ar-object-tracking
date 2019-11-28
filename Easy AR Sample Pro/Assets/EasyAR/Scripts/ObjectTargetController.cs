//================================================================================================================================
//
//  Copyright (c) 2015-2019 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;
using easyar;
public class ObjectTargetController : MonoBehaviour
{
    public bool Tracked = false;

    public string ObjPath = null;
    public string MtlPath = null;
    public string JpgPath = null;

    public PathType Storage = PathType.StreamingAssets;

    public ObjectTrackerBehaviour Tracker = null;

    private ObjectTarget objTarget = null;
    private BufferDictionary objBufferDic = null;
    void Start()
    {
        objBufferDic = new BufferDictionary();
        StartCoroutine(LoadObjIntoDic(ObjPath, Storage, objBufferDic));
        StartCoroutine(LoadObjIntoDic(MtlPath, Storage, objBufferDic));
        StartCoroutine(LoadObjIntoDic(JpgPath, Storage, objBufferDic));
        StartCoroutine(LoadObjTarget(objBufferDic, 3));
    }

    IEnumerator LoadObjIntoDic(string path, PathType type, BufferDictionary dic)
    {
        var url = getWWWPath(path, type);
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            yield return 0;
        }
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
            www.Dispose();
            yield break;
        }
        var buffer = easyar.Buffer.create(www.bytes.Length);
        Marshal.Copy(www.bytes, 0, buffer.data(), buffer.size());
        dic.set(url, buffer);
    }

    IEnumerator LoadObjTarget(BufferDictionary dic, int needSize)
    {
        while (dic.count() < needSize)
        {
            yield return 0;
        }
        var p = new ObjectTargetParameters();
        p.setBufferDictionary(dic);
        p.setObjPath(getWWWPath(ObjPath, Storage));
        var op_objTarget = ObjectTarget.createFromParameters(p);
        if (op_objTarget.OnSome)
        {
            objTarget = op_objTarget.Value;
        }
        else
        {
            throw new System.Exception("object target create failed");
        }
        Tracker.LoadObjectTarget(this, (target, status) =>
        {
            gameObject.active = false;
            Debug.Log("[EasyAR] Targtet name: " + target.name() + " Target runtimeID: " + target.runtimeID() + " load status: " + status);
        });
    }

    private string getWWWPath(string path, PathType type)
    {
        var wwwPath = path;
        switch (type)
        {
            case PathType.Absolute:
                wwwPath = "file://" + path;
                break;
            case PathType.StreamingAssets:
#if UNITY_EDITOR || UNITY_IOS || UNITY_STANDALONE
                wwwPath = "file://" + Application.streamingAssetsPath + "/" + path;
#elif UNITY_ANDROID
                    wwwPath = System.IO.Path.Combine(Application.streamingAssetsPath, path);
#endif
                break;
        }
        return wwwPath;
    }

    public void OnFound()
    {
        gameObject.SetActive(true);
    }

    public void OnLost()
    {
        gameObject.SetActive(false);
    }

    public void OnTracking(Matrix4x4 pose)
    {
        Utility.SetMatrixOnTransform(transform, pose);
    }

    public Target Target()
    {
        return objTarget;
    }

    public void Dispose()
    {
        if (Tracker != null)
            Tracker.UnloadObjectTarget(this, (target, status) => { Debug.Log("[EasyAR] Targtet name: " + target.name() + " Target runtimeID: " + target.runtimeID() + " load status: " + status); });
        if (objTarget != null)
            objTarget.Dispose();
    }
}
