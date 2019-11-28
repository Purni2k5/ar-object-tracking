//================================================================================================================================
//
//  Copyright (c) 2015-2019 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
namespace easyar
{
    public class RecorderBehaviour : MonoBehaviour
    {
        public enum RecordType
        {
            Camera,
            Screen
        }
        public RecordProfile Profile = RecordProfile.Quality_Default;
        public RecordVideoSize FrameSize = RecordVideoSize.Vid1080p;
        public RecordVideoOrientation VideoOrientation = RecordVideoOrientation.Portrait;
        public RecordZoomMode RecordZoomMode = RecordZoomMode.NoZoomAndClip;
        public RecordType Type = RecordType.Screen;
        public CameraImageRenderer CameraRenderer;
        private bool Recording = false;
        private easyar.Recorder ezar_recorder;
        private RenderTexture rt;
        private CommandBuffer commandBuffer;
        private Camera cam;
        private bool isAvailable = false;

        private string videoPath;
        private void Start()
        {
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
            {
                GUIPopup.AddShowMessage("Only support OpenGLES2 or OpenGLES3 Grapics API, Please change it to OpenGLES2 or OpenGLES3", 10);
                return;
            }

            if (SystemInfo.graphicsMultiThreaded)
            {
                GUIPopup.AddShowMessage("Not support graphics MultiThreaded, Please disable it in playerSetting", 10);
                return;
            }

            isAvailable = Recorder.isAvailable();
            if (!isAvailable)
            {
                GUIPopup.AddShowMessage("Recorder is not support on this platform", 5);
                return;
            }

            easyar.Recorder.requestPermissions(EasyARBehaviour.Scheduler, (System.Action<PermissionStatus, string>)((status, msg) =>
            {
                if (status == PermissionStatus.Granted)
                {
                    StartCoroutine(UpdateFrame());
                }
                Debug.Log("[EasyAR] RequestPermissions status " + status + " msg " + msg);
            }));
        }

        public void SampleStart()
        {
            videoPath = Application.persistentDataPath + "/" + System.DateTime.Now.Ticks + ".mp4";
            GUIPopup.AddShowMessage("start recording video path : " + videoPath, 5);
            StartRecord(videoPath);
        }

        public void SampleStop()
        {
            StopRecord();
        }

        public void StartRecord(string path)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.LogError("[EasyAR] Only support android or iOS platform");
            return;
#endif

            if (ezar_recorder != null)
            {
                Recording = false;
                if (ezar_recorder.stop())
                {
                    GUIPopup.AddShowMessage("save successed video path : " + videoPath, 5);
                }
                else
                {
                    GUIPopup.AddShowMessage("save failed video path : " + videoPath, 5);
                }
            }

            OnDestroy();

            Debug.Log("[EasyAR] Video save path : " + path);
            if (ezar_recorder != null)
            {
                ezar_recorder.Dispose();
            }
            RecorderConfiguration configuration = new RecorderConfiguration();
            configuration.setOutputFile(path);
            configuration.setProfile(Profile);
            configuration.setVideoSize(FrameSize);
            configuration.setVideoOrientation(VideoOrientation);
            configuration.setZoomMode(RecordZoomMode);
            ezar_recorder = easyar.Recorder.create(configuration, EasyARBehaviour.Scheduler, (System.Action<RecordStatus, string>)((status, msg) =>
            {
                switch (status)
                {
                    case RecordStatus.OnStarted:
                        break;
                    case RecordStatus.OnStopped:
                        break;
                    case RecordStatus.FailedToStart:
                        break;
                    case RecordStatus.FileSucceeded:
                        break;
                    case RecordStatus.FileFailed:
                        break;
                    case RecordStatus.LogInfo:
                        break;
                    case RecordStatus.LogError:
                        break;
                    default:
                        break;
                }
                Debug.Log("[EasyAR] Record status : " + status + " easyar msg : " + msg);
            }));
            configuration.Dispose();
            ezar_recorder.start();
            Recording = true;
            if (Type == RecordType.Screen)
            {
                rt = new RenderTexture(Screen.width, Screen.height, 0);
                commandBuffer = new CommandBuffer();
                commandBuffer.Blit(null, rt);
                cam = gameObject.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Depth;
                cam.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            }
        }

        public void StopRecord()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            GUIPopup.AddShowMessage("[EasyAR] Only support android or iOS platform", 5);
            return;
#endif
            if (ezar_recorder != null)
            {
                Recording = false;
                if (ezar_recorder.stop())
                {
                    GUIPopup.AddShowMessage("save successed video path : " + videoPath, 5);
                }
                else
                {
                    GUIPopup.AddShowMessage("save failed video path : " + videoPath, 5);
                }
            }

            OnDestroy();
        }

        private IEnumerator UpdateFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (Recording)
                {
                    if (Type == RecordType.Camera && CameraRenderer != null)
                    {
                        var texture = CameraRenderer.TargetTexture;
                        var textureId = TextureId.fromInt(texture.GetNativeTexturePtr().ToInt32());
                        ezar_recorder.updateFrame(textureId, texture.width, texture.height);
                        textureId.Dispose();
                    }
                    else
                    {
                        var w = Screen.width;
                        var h = Screen.height;
                        if (rt == null || w != rt.width || h != rt.height)
                        {
                            StopRecord();
                            continue;
                        }

                        var textureId = TextureId.fromInt(rt.GetNativeTexturePtr().ToInt32());
                        if (ezar_recorder != null)
                        {
                            ezar_recorder.updateFrame(textureId, rt.width, rt.height);
                        }
                        textureId.Dispose();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (cam != null)
            {
                cam.RemoveAllCommandBuffers();
                Destroy(cam);
                cam = null;
            }
            if (commandBuffer != null)
            {
                commandBuffer.Dispose();
                commandBuffer = null;
            }
            if (ezar_recorder != null)
            {
                ezar_recorder.Dispose();
                ezar_recorder = null;
            }
            if (rt != null)
            {
                Destroy(rt);
                rt = null;
            }
        }
    }
}