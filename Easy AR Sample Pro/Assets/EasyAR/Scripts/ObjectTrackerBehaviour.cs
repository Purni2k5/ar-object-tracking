//================================================================================================================================
//
//  Copyright (c) 2015-2019 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================
using System;
using System.Collections.Generic;
using UnityEngine;
using easyar;
public class ObjectTrackerBehaviour : MonoBehaviour {
    public enum CenterMode
    {
        SpecificTarget,
        FirstTarget,
        Camera
    }
    public int SimultaneousNum = 1;
    public CenterMode CenterTarget = CenterMode.Camera;
    public ObjectTargetController CenterImageTarget = null;
    public Camera TargetCamera = null;
    private List<ObjectTargetController> targetControllers;
    private ObjectTracker tracker = null;
    private Matrix4x4 centerTransform = Matrix4x4.identity;

    void Awake()
    {
        if (!ObjectTracker.isAvailable())
        {
            throw new Exception("object tracker not support");
        }
        tracker = ObjectTracker.create();
        tracker.setSimultaneousNum(SimultaneousNum);
        targetControllers = new List<ObjectTargetController>();
    }

    public FeedbackFrameSink Input()
    {
        if (tracker == null)
        {
            throw new Exception("object tracker is null");
        }
        return tracker.feedbackFrameSink();
    }

    public OutputFrameSource Output()
    {
        if (tracker == null)
        {
            throw new Exception("object tracker is null");
        }
        return tracker.outputFrameSource();
    }

    public void UpdateFrame(ARSessionUpdateEventArgs args)
    {
        List<ObjectTargetController> currentTrackingBehaviours = new List<ObjectTargetController>();
        var frame = args.IFrame;

        var results = args.OFrame.results();
        ObjectTrackerResult result = null;
        foreach (var _result in results)
        {
            if (_result.OnSome)
            {
                result = _result.Value as ObjectTrackerResult;
            }
            if (result != null)
            {
                break;
            }
        }
        if (result != null)
        {
            var targetInstances = result.targetInstances();
            int centerTargetId = -1;
            if (TargetCamera == null)
            {
                Utility.SetMatrixOnTransform(Camera.main.transform, centerTransform);
            }
            else
            {
                Utility.SetMatrixOnTransform(TargetCamera.transform, centerTransform);
            }
            if (CenterImageTarget != null && CenterImageTarget.Target() != null && CenterTarget == CenterMode.SpecificTarget)
            {
                centerTargetId = CenterImageTarget.Target().runtimeID();
            }
            foreach (var targetInstance in targetInstances)
            {
                var op_target = targetInstance.target();
                if (!op_target.OnSome)
                {
                    continue;
                }
                var target = op_target.Value;
                var status = targetInstance.status();
                foreach (var targetController in targetControllers)
                {
                    var _target = targetController.Target();
                    if (target.runtimeID() == _target.runtimeID())
                    {
                        if (status == TargetStatus.Tracked)
                        {
                            if (!targetController.Tracked)
                            {
                                targetController.OnFound();
                                targetController.Tracked = true;
                            }
                            var pose = Utility.Matrix44FToMatrix4x4(targetInstance.pose());
                            pose = args.ImageRotationMatrixGlobal * pose;

                            if (CenterTarget == CenterMode.FirstTarget && centerTargetId == -1)
                            {
                                centerTargetId = target.runtimeID();
                                CenterImageTarget = targetController;
                            }

                            if (centerTargetId != target.runtimeID())
                            {
                                pose = centerTransform * pose;
                                targetController.OnTracking(pose);
                            }
                            else
                            {
                                targetController.OnTracking(Matrix4x4.identity);
                                centerTransform = pose.inverse;
                            }
                            currentTrackingBehaviours.Add(targetController);
                        }
                    }
                }
                target.Dispose();
                targetInstance.Dispose();
            }
            result.Dispose();
        }
        foreach (var targetController in targetControllers)
        {
            bool contain = false;
            foreach (var item in currentTrackingBehaviours)
            {
                if (item == targetController)
                {
                    contain = true;
                }
            }
            if (!contain && targetController.Tracked)
            {
                targetController.OnLost();
                targetController.Tracked = false;
            }
        }
    }

    public void LoadObjectTarget(ObjectTargetController controller, System.Action<Target, bool> callback)
    {
        if (tracker == null)
        {
            throw new Exception("object tracker is null");
        }
        tracker.loadTarget(controller.Target(), EasyARBehaviour.Scheduler, callback);
        targetControllers.Add(controller);
    }

    public void UnloadObjectTarget(ObjectTargetController controller, System.Action<Target, bool> callback)
    {
        if (tracker == null)
        {
            throw new Exception("object tracker is null");
        }
        tracker.unloadTarget(controller.Target(), EasyARBehaviour.Scheduler, callback);
        targetControllers.Remove(controller);
    }

    public void StartTracker()
    {
        if (tracker == null)
        {
            throw new Exception("object tracker is null");
        }
        tracker.start();
    }

    public void StopTracker()
    {
        if (tracker == null)
        {
            throw new Exception("object tracker is null");
        }
        tracker.stop();
    }

    public void CloseTracker()
    {
        if (tracker == null)
        {
            throw new Exception("object tracker is null");
        }
        tracker.close();
        tracker.Dispose();
        tracker = null;
    }

    private void Start()
    {
        StartTracker();
    }

    private void OnDisable()
    {
        StopTracker();
    }
    private void OnDestroy()
    {
        CloseTracker();
    }

}
