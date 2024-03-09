//MIT License

//Copyright (c) 2023 biosmanager

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using UnityEngine.Rendering;
using System.Linq;

/// <summary>
///  Reference Section used for getting a texture from GPU to CPU and out to a file in near-realtime and low latency cost. 
/// 
///  There are several ways of getting data out from a Texture2D which include the use of the sample function in SaveTexture(), however
///  this is highly inefficient and not reccomended for real-time use. 
/// 
///  Instead use the GPU-efficient function of AsyncGPURequest, which attempts at bringing in data efficiently and 
///  uses cycles more efficiently. 
///  
///  https://forum.unity.com/threads/compute-shader-read-texture-value.972342/
///  https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/sm5-object-texture2d
///  https://www.ronja-tutorials.com/post/050-compute-shader/
///  https://forum.unity.com/threads/tutorial-to-start-with-compute-shaders.500648/
///  http://kylehalladay.com/blog/tutorial/2014/06/27/Compute-Shaders-Are-Nifty.html
///  https://www.ronja-tutorials.com/post/050-compute-shader/
///  https://catlikecoding.com/unity/tutorials/basics/compute-shaders/
///  https://forum.unity.com/threads/can-we-write-to-a-texture2d-from-a-compute-buffer.512845/
///  https://dev.to/alpenglow/unity-encoding-data-into-pixels-hlsl-shaders-50a7
///  https://docs.unity3d.com/ScriptReference/Rendering.AsyncGPUReadback.Request.html
///  
///  https://forum.unity.com/threads/graphics-copytexture-mismatching-texture-types.1312653/
///  
///  https://github.com/keijiro/AsyncCaptureTest/blob/master/Assets/AsyncCapture.cs
///  https://forum.unity.com/threads/loadrawtexturedata-not-enough-data-provided.439760/
///  
///  // This uses an advanced type of pixel reading.
///  https://dev.to/alpenglow/unity-fast-pixel-reading-cbc
///  https://dev.to/alpenglow/unity-fast-pixel-reading-part-2-asyncgpureadback-4kgn
///  https://support.unity.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
///  
///  //This explains why its a bad idea to use expensive functions
///  https://medium.com/google-developers/real-time-image-capture-in-unity-458de1364a4c
/// 
///  //This is the final solution found
///  https://forum.unity.com/threads/upload-and-write-to-rendertexture-as-texture2darray-in-compute-shader.495137/
/// </summary>




namespace OVRT
{
    /// <summary>
    /// Manages connection to OpenVR and dispatches new poses and events.
    /// </summary>
    public class OVRT_Manager : MonoBehaviour
    {
        public enum UpdateMode
        {
            FixedUpdate,
            Update,
            LateUpdate,
            OnPreCull
        }

        public ETrackingUniverseOrigin trackingUniverse = ETrackingUniverseOrigin.TrackingUniverseStanding;
        public UpdateMode updateMode = UpdateMode.Update;
        public bool useSteamVrTrackerRoles = true;
        public float predictedSecondsToPhotonsFromNow = 0.0f;

        public bool[] ConnectedDeviceIndices { get; private set; } = new bool[OpenVR.k_unMaxTrackedDeviceCount];
        public Dictionary<string, string> Bindings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> SteamVrTrackerBindings { get; private set; } = new Dictionary<string, string>();

        private bool _isInitialized = false;
        private CVRSystem _vrSystem;
        private CVRCompositor _compositor; 
        private TrackedDevicePose_t[] _poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

        public XRT_OVR_Grabber.LoggingManagerAPI loggingManager; 

        private UnityAction<int, bool> _onDeviceConnected;
        private string _steamVrConfigPath = null;

        private List<ulong> _trackedControllers = new List<ulong>();
        private List<ulong> _actionHandles = new List<ulong>();

        [SerializeField]
        XRT_OVR_Grabber.SteamVR2Input interactionManager;

        [SerializeField]
        XRT_OVR_Grabber.SteamVRInteractionsReader interactionManagerRework; 

        //Getting input interface from the OpenVR interface. 
        public static VRActiveActionSet_t[] rawActiveActionSetArray;

        /// <summary> gets tracker bindings related to SteamVR and returns these bindings as a dictionary <summary> 
        public Dictionary<string, string> GetSteamVrTrackerBindings()
        {
            Dictionary<string, string> trackerBindings = new Dictionary<string, string>();

            string steamVrSettingsPath;
            string deviceSettingsPath = "";

            if (deviceSettingsPath == "")
            {
                //deviceSettingsPath = Path.Combine(Application.dataPath, "Resources/Config/general_vr_settings.json");

                if(loggingManager.openVRRuntimeConfig != "")
                    deviceSettingsPath = Path.Combine(loggingManager.openVRRuntimeConfig, "Resources/Config/general_vr_settings.json");
                else
                    deviceSettingsPath = Path.Combine(Application.persistentDataPath, "Resources/Config/general_vr_settings.json");

            }


            if (_steamVrConfigPath is null)
            {
                steamVrSettingsPath = Path.Combine(OpenVR.RuntimePath(), "../../../config/steamvr.vrsettings");
            }
            else
            {
                steamVrSettingsPath = Path.Combine(_steamVrConfigPath, "steamvr.vrsettings");
            }

            if (!File.Exists(steamVrSettingsPath))
            {
                Debug.LogWarning("[OVRT] Could not find SteamVR configuration file!");
                return trackerBindings;
            }

            var json = File.ReadAllText(steamVrSettingsPath);
            var steamVrSettings = JObject.Parse(json);

            OpenVR.Input.SetActionManifestPath(deviceSettingsPath);

            if (steamVrSettings.ContainsKey("trackers"))
            {
                var trackers = steamVrSettings["trackers"].ToObject<Dictionary<string, string>>();
                foreach (var pair in trackers)
                {
                    Debug.Log(pair);

                    trackerBindings.Add(pair.Key.Replace("/devices/htc/vive_tracker", ""), pair.Value);    
                }
            }

            return trackerBindings;
        }
        /// <summary> takes a JSON string as an argument and attempts to deserialize it into a dictionary of bindings. If successful, it updates the 'Bindings' propertiy with the deserialized data and returns true.
        /// if deserialization fails, it returns false. <summary> 

        public bool ReadBindingsFromJson(string json)
        {
            try
            {
                var newBindings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                Bindings = newBindings;
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        /// <summary> Converts bindings dictonary into a JSON string. If the serialization is successful, it outputs the resulting JSON string and returns true. 
        /// if serialization fails, it outputs an empty string and returns false. <summary> 
        public bool ConvertBindingsToJson(out string json)
        {
            try
            {
                json = JsonConvert.SerializeObject(Bindings);
                return true;
            }
            catch (JsonException)
            {
                json = "";
                return false;
            }
        }
        /// <summary> Retrives a string property from a tracked device in OpenVR. Uses the _vrsystem.GetStringTrackedDeviceProperty method to get the property
        /// If property retrieval is successful and the result has content, it returns the property as a string. 
        /// <summary>
        public string GetStringProperty(ETrackedDeviceProperty prop, uint deviceId)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var capacity = _vrSystem.GetStringTrackedDeviceProperty(deviceId, prop, null, 0, ref error);
            if (capacity > 1)
            {
                var result = new System.Text.StringBuilder((int)capacity);
                _vrSystem.GetStringTrackedDeviceProperty(deviceId, prop, result, capacity, ref error);
                return result.ToString();
            }
            return (error != ETrackedPropertyError.TrackedProp_Success) ? error.ToString() : "<unknown>";
        }
        UnityAction _recordActionEvent;
        /// <summary> A Unity lifecycle method that is called when the script instance is being loaded. 
        /// It subscribes to certain events and initializes refrences to the loggingManagerAPI and the interactionManager by finding them in the scene <summary>
        private void Awake()
        {
            _onDeviceConnected += OnDeviceConnected;
            _recordActionEvent += ToggleRecordingMode; 
            loggingManager = GameObject.Find("LoggingManger").GetComponent<XRT_OVR_Grabber.LoggingManagerAPI>();
            //interactionManager = GameObject.Find("LoggingManger").GetComponent<XRT_OVR_Grabber.SteamVR2Input>();
            loggingManager.InitLogger();

            //trackingUniverse = OpenVR.Compositor.GetTrackingSpace();
            Init();
        }

        /// <summary> Called when script is initalized. Subscribes to the 'TrackedDeviceConnected' and 'ToggleRecordingMode' events <summary> 
        private void OnEnable()
        {
            OVRT_Events.TrackedDeviceConnected.AddListener(_onDeviceConnected);
            XRT_OVR_Grabber.QuestionnaireEvents.ToggleRecordingMode.AddListener(_recordActionEvent); 
        }

        /// <summary> Cleans up and detaches event listeners <summary>
        private void OnDisable()
        {
            OVRT_Events.TrackedDeviceConnected.RemoveListener(_onDeviceConnected);
            Array.Clear(ConnectedDeviceIndices, 0, ConnectedDeviceIndices.Length);
            XRT_OVR_Grabber.QuestionnaireEvents.ToggleRecordingMode.RemoveListener(_recordActionEvent);
        }

        /// <summary> Ensures that OpenVR runtome is properly installed and set up, and retrieves neccesary configurations <summary>
        private void Init()
        {
            if (!OpenVR.IsRuntimeInstalled())
            {
                Debug.LogError("[OVRT] SteamVR runtime not installed!");
                return;
            }

            // Ensure SteamVR is running
            if (!OpenVR.IsHmdPresent())
            {
                var dummyError = EVRInitError.None;
                OpenVR.Init(ref dummyError, EVRApplicationType.VRApplication_Scene);
                System.Threading.SpinWait.SpinUntil(() => OpenVR.IsHmdPresent(), TimeSpan.FromSeconds(10));
                OpenVR.Shutdown();
            }

            var initError = EVRInitError.None;
            _vrSystem = OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Other);
            _compositor = OpenVR.Compositor;
            if (initError != EVRInitError.None)
            {
                var initErrorString = OpenVR.GetStringForHmdError(initError);
                Debug.LogError($"[OVRT] Could not initialize OpenVR tracking: {initErrorString}");
                return;
            }

            _isInitialized = true;

            var openVrPathsConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openvr", "openvrpaths.vrpath");
           // Debug.Log(openVrPathsConfigPath);
            if (File.Exists(openVrPathsConfigPath))
            {
                var json = File.ReadAllText(openVrPathsConfigPath);
                var openVrPathsConfig = JObject.Parse(json);

                if (openVrPathsConfig.ContainsKey("config"))
                {
                    var paths = openVrPathsConfig["config"].ToObject<List<string>>();
                    if (paths.Count > 0)
                    {
                        _steamVrConfigPath = paths[0];
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[OVRT] Could not find {openVrPathsConfigPath}!");
            }

            Debug.Log($"[OVRT] Initialized OpenVR tracking.");

            UpdateSteamVrTrackerBindings();
            loggingManager.InitLogger();
            leftEyeTexture = CollectTextureFromEye(EVREye.Eye_Left);
            rightEyeTexture = CollectTextureFromEye(EVREye.Eye_Right);
        }
        /// <summary>  <summary>
        private void OnDestroy()
        {
            OpenVR.Shutdown();
        }


        /// This version of the update loop will run in corotine instead.
        /// 
        //while (currentTick < rateHz)

        //for(int i = 0; i < rateHz; i++)
        //{
        //    UpdatePoses();
        //    yield return null;
        //}
        //frameCollected++;
        //lockProcess = false

        float rateHz = 72; 
        int frameCollected = 0;
        bool stopUpdateThread = false;
        IEnumerator UpdatePosesAndLog()
        {
            float localDeltaTime = 0.0f;
            localDeltaTime = Time.time;
            while (true)
            {
                yield return new WaitForSecondsRealtime(localDeltaTime - Time.time);
                UpdatePoses();
                localDeltaTime += updateRate;

                if (stopUpdateThread)
                    break;
            }
            lockProcess = false; 
        }

        private void DebugCall()
        {
          
            List<XRT_OVR_Grabber.Pose> actualPoses = new List<XRT_OVR_Grabber.Pose>();
            _vrSystem.GetDeviceToAbsoluteTrackingPose(trackingUniverse, predictedSecondsToPhotonsFromNow, _poses);
            var transposedPose = new OVRT_Utils.RigidTransform(_poses[0].mDeviceToAbsoluteTracking);
            XRT_OVR_Grabber.Pose logPose = new XRT_OVR_Grabber.Pose(_poses[0], transposedPose.pos, transposedPose.rot, frameCollected, Time.realtimeSinceStartup);
            actualPoses.Add(logPose);
            //Debug.Log("An actual potato wrote this");
            if (loggingManager.isRecording)
            {
                loggingManager.AddInteractionPoseRecord(new XRT_OVR_Grabber.PoseInteractionLog(actualPoses, new XRT_OVR_Grabber.ControllerInteraction()));
            }
             
        }

        bool lockProcess = false;
        bool lockPictureProcess = false;
        float updateRate = 1.0f; 
        float pictureRecordTime = 0.0f;
        /// <summary>  
        ///  Fixed update does not sample multiple times per second, it spams multiple requests per frame, but it becomes unsafe to sample multiple times per second. 
        /// 
        /// <summary>
        private void FixedUpdate()
        {
            updateRate = 1.0f / rateHz;
            if (updateMode == UpdateMode.FixedUpdate)
            {
                //Get the thread running once and stop any random restarts. 
                if (lockProcess)
                {
                    StartCoroutine(UpdatePosesAndLog());
                    lockProcess = true;
                }


                //if (timePassed > 1.0f)
                //{
                //    if (loggingManager.isRecording && !loggingManager.pauseRecording && !lockProcess)
                //    {
                //        lockProcess = true;
                //        StartCoroutine(UpdatePosesAndLog());
                //    }
                //    timePassed = 0.0f;
                //}
                //else
                //{
                //    timePassed += Time.deltaTime;
                //    //return;
                //}
                //UpdatePoses();

                if (loggingManager.isRecording && !loggingManager.pauseRecording)
                {
                    if (pictureRecordTime > rateLimit)
                    {
                        StartCoroutine(RealTimeSaveTexture2(leftEyeTexture, "leftEye/"));
                        StartCoroutine(RealTimeSaveTexture2(rightEyeTexture, "rightEye/"));
                        pictureRecordTime = 0.0f;
                    }
                    else
                    {
                        pictureRecordTime += Time.deltaTime;
                        return;
                    }
                }
            }
        }

        /// <summary> If restart button is pressed, or in update mode, it initializes the system.  <summary>
        private void Update()
        {
            //Dynamically setting the refresh rate. 
            if (loggingManager.useDefaultSteamVRRefreshRate)
            { 
                ETrackedPropertyError errorOut = new ETrackedPropertyError();
                ETrackedDeviceProperty props = ETrackedDeviceProperty.Prop_DisplayFrequency_Float;
                var refreshRate = _vrSystem.GetFloatTrackedDeviceProperty((uint)0, props, ref errorOut); //Set this to the HMD
                if (refreshRate > 0)
                {
                    rateHz = (float)refreshRate;
                }
            }
            else
            {
                rateHz = loggingManager.recordRateInterval; 
            }

            //Update the refresh rate
            updateRate = 1.0f / rateHz;
            pictureRecordTime = 1.0f / loggingManager.pictureRecordRate;

            if (updateMode == UpdateMode.Update)
            {
                //Get the thread running once and stop any random restarts. 
                if (!lockProcess)
                {
                    StartCoroutine(UpdatePosesAndLog());
                    lockProcess = true;
                }
                else
                {
                    //UpdatePoses();
                }

                if (!lockPictureProcess)
                {
                    StartCoroutine(PictureThread());
                    lockPictureProcess = true;
                }

                //if (timePassed > 1.0f)
                //{
                //    if (loggingManager.isRecording && !loggingManager.pauseRecording)
                //    {
                //        StartCoroutine(UpdatePosesAndLog());
                //    }
                //    timePassed = 0.0f;
                //}
                //else
                //{
                //    //This simply puts down a regular interval of poses but will not be recorded nor is reccomended to sample from. 
                //    UpdatePoses();
                //    timePassed += Time.deltaTime;
                //}





                //if (pictureRecordTime > rateLimit)
                //{
                //    StartCoroutine(RealTimeSaveTexture2(leftEyeTexture, "leftEye/"));
                //    StartCoroutine(RealTimeSaveTexture2(rightEyeTexture, "rightEye/"));
                //    pictureRecordTime = 0.0f;
                //}
                //else
                //{
                //    pictureRecordTime += Time.deltaTime;
                //    return;
                //}

            }
        }

        //Picture refresh thread. 
        IEnumerator PictureThread()
        {
            float localDeltaTime = 0.0f;
            localDeltaTime = Time.time;
            while (true)
            {
                yield return new WaitForSecondsRealtime(localDeltaTime - Time.time);
                if (loggingManager.isRecording && !loggingManager.pauseRecording && loggingManager.enablePictureCapture)
                {
                    Debug.Log("Picture before");
                    StartCoroutine(RealTimeSaveTexture2(leftEyeTexture, "leftEye/"));
                    StartCoroutine(RealTimeSaveTexture2(rightEyeTexture, "rightEye/"));
                    Debug.Log("Picture after");
                }
                //UpdatePoses();
                localDeltaTime += pictureRecordTime;

                if (stopUpdateThread)
                    break;
            }
            lockPictureProcess = false;
        }



        /// <summary>  <summary>
        private void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate)
            {
                UpdatePoses();
            }
        }
        /// <summary>  <summary>
        private void OnPreCull()
        {
            if (updateMode == UpdateMode.OnPreCull)
            {
                UpdatePoses();
            }
        }
        /// <summary> handles events when device is connected <summary>
        private void OnDeviceConnected(int index, bool connected)
        {
            ConnectedDeviceIndices[index] = connected;
        }

        /// <summary> Toggles recording mode on or off.  <summary>
        public void ToggleRecordingMode()
        {
            GetSessionProperties();
            SetRecordSessionProperties();
            recordingState = !recordingState;
        }

        /// <summary> Retrieves certain properties related to current VR session <summary>
        public void GetSessionProperties()
        {
            HmdQuad_t roomAreaRectangle = new HmdQuad_t();
            float roomSizeX = 0.0f, roomSizeY = 0.0f;
            OpenVR.Chaperone.GetPlayAreaRect(ref roomAreaRectangle);
            OpenVR.Chaperone.GetPlayAreaSize(ref roomSizeX, ref roomSizeY);
            uint applicationNumber = OpenVR.Applications.GetApplicationCount();
            string runtimeVersion =  _vrSystem.GetRuntimeVersion(); 
            loggingManager._projectProperties = new XRT_OVR_Grabber.ProjectUniqueProperties(roomAreaRectangle, roomSizeX, roomSizeY, (int) applicationNumber, runtimeVersion);
            for (uint i = 0; i < applicationNumber; i++)
            {
                return; 
                /*
                //OpenVR.Applications.GetApplicationKeyByIndex(i, );
                OpenVR.Applications.GetApplicationPropertyString(); 
                */
            }
        }

        /// <summary> Sets properties related to recording sessions <summary>
        public void SetRecordSessionProperties()
        {
            rateLimit = (float)loggingManager.recordRateInterval;
        }
        
        //private int updateHzRateLimiter = 0;
        [SerializeField]
        private float rateLimit = 10.0f;
        /// <summary> Updates poses of VR devies, processes VR events, and handles recording related tasks of recording is enabled  <summary>
        private void UpdatePoses()
        {
            //Update limiter set at the rate of rateLimit. 
            if (!_isInitialized) return;
            EDeviceActivityLevel status =  _vrSystem.GetTrackedDeviceActivityLevel(OpenVR.k_unTrackedDeviceIndex_Hmd);

            var statusString = status.ToString("G");

            if (statusString == "k_EDeviceActivityLevel_Idle")
            {
                loggingManager._TogglePauseRecording(true,false);
            }
            else if (statusString == "k_EDeviceActivityLevel_UserInteraction" && loggingManager.pauseRecording)
            {
                loggingManager._TogglePauseRecording(false,false);
            }

            // Process OpenVR event queue
            var vrEvent = new VREvent_t();
            while (_vrSystem.PollNextEvent(ref vrEvent, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t))))
            {
                switch ((EVREventType)vrEvent.eventType)
                {
                    case EVREventType.VREvent_TrackedDeviceActivated:
                        OVRT_Events.TrackedDeviceConnected.Invoke((int)vrEvent.trackedDeviceIndex, true);
                        //loggingManager.UpdateTotalDevicesConnected(totalDevicesConnected);
                        break;
                    case EVREventType.VREvent_TrackedDeviceDeactivated:
                        OVRT_Events.TrackedDeviceConnected.Invoke((int)vrEvent.trackedDeviceIndex, false);
                        break;
                    case EVREventType.VREvent_TrackersSectionSettingChanged:
                        // Allow some time until SteamVR configuration file has been updated on disk
                        Invoke("UpdateSteamVrTrackerBindings", 1.0f);
                        break;
                    case EVREventType.VREvent_ShowRenderModels:
                        OVRT_Events.HideRenderModelsChanged.Invoke(false);
                        break;
                    case EVREventType.VREvent_HideRenderModels:
                        OVRT_Events.HideRenderModelsChanged.Invoke(true);
                        break;
                    case EVREventType.VREvent_ModelSkinSettingsHaveChanged:
                        OVRT_Events.ModelSkinSettingsHaveChanged.Invoke();
                        break;
                    case EVREventType.VREvent_Quit:
                    case EVREventType.VREvent_ProcessQuit:
                    case EVREventType.VREvent_DriverRequestedQuit:
                        return;
                    default:
                        break;
                }
            }
            _vrSystem.GetDeviceToAbsoluteTrackingPose(trackingUniverse, predictedSecondsToPhotonsFromNow, _poses);
            
            XRT_OVR_Grabber.Pose[] _loggedPoses = new XRT_OVR_Grabber.Pose[_poses.Length];
            OVRT_Events.NewPoses.Invoke(_poses);

            //Recording Loop, only enabled if its enabled. 
            List<XRT_OVR_Grabber.Pose> actualPoses = new List<XRT_OVR_Grabber.Pose>();

            for (uint i = 0; i < _poses.Length; i++)
            {
                //Pose referencePose;
                var pose = _poses[i];
                //if (pose.bDeviceIsConnected && pose.bPoseIsValid)

                //Checks if we don't have an invalid device and records. 
                if (_vrSystem.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Invalid)
                {
                    float collectTime = 0;
                    float timeAtFrameCollect = Time.realtimeSinceStartup;
                    var transposedPose = new OVRT_Utils.RigidTransform(_poses[i].mDeviceToAbsoluteTracking);

                    //To check if we are recording.
                    if (loggingManager.recordingSuccess)
                    {
                        if(lockTime){}
                        else
                        {
                            startTime = timeAtFrameCollect;
                            lockTime = true;
                        }
                    }
                    else
                    {
                        lockTime = false;
                        startTime = timeAtFrameCollect;
                    }
                    collectTime = (timeAtFrameCollect - startTime);
                    XRT_OVR_Grabber.Pose logPose = new XRT_OVR_Grabber.Pose(_poses[i], transposedPose.pos, transposedPose.rot, frameCollected, collectTime);
                    _loggedPoses[i] = logPose;

                    var role =_vrSystem.GetControllerRoleForTrackedDeviceIndex((uint)i);
                    var deviceClass = _vrSystem.GetTrackedDeviceClass((uint)i);

                    _loggedPoses[i].SetDeviceRole(role.ToString("G"));
                    if (role.ToString("G") == "Invalid")
                    {
                        _loggedPoses[i].SetDeviceRole(i.ToString());
                    }
                    else
                    {
                        _loggedPoses[i].SetDeviceRole(role.ToString("G"));
                    }
                    _loggedPoses[i].SetDeviceClass(deviceClass.ToString("G"));

                    actualPoses.Add(_loggedPoses[i]);

                    
                    //var serialNumber = GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String, i);                  
                    ////Something?
                    //string binding;
                    //if (Bindings.TryGetValue(serialNumber, out binding))
                    //{
                    //    OVRT_Events.NewBoundPose.Invoke(binding, pose, (int)i);
                    //}

                    //if (useSteamVrTrackerRoles)
                    //{
                    //    string trackerBinding;
                    //    if (SteamVrTrackerBindings.TryGetValue(serialNumber, out trackerBinding))
                    //    {
                    //        OVRT_Events.NewBoundPose.Invoke(trackerBinding, pose, (int)i);
                    //    }
                    //}                    
                }
            }
            if (loggingManager.firstPassInitialization)
            {
                //Should be more reliable than trying to find active devices. 
                loggingManager.UpdateTotalDevicesConnected(GetConnectedDeviceCount());
                //interactionManagerRework.DumpStartingDataAndStartRecording();
                loggingManager.firstPassInitialization = false;
            }


            //record = ReorderList(record);
            var record = new XRT_OVR_Grabber.PoseInteractionLog(actualPoses, interactionManagerRework.GetLastInteractions());
            if (loggingManager.recordingSuccess)
            {
                //We create the index order through get connected device count if we don't have a count already. 
                if(indexOrder.Count != actualPoses.Count)      
                    GetConnectedDeviceCount();
                //Reorders based on {HMD, LEFT HAND,RIGHT HAND}
                record.ReorderPosesList(indexOrder);

                loggingManager.AddInteractionPoseRecord(record);
                //loggingManager.AddInteractionPoseRecord(new XRT_OVR_Grabber.PoseInteractionLog(actualPoses, interactionManager.GetLastInteractions()));
            }
            else
            {
                //Ugly hack but works for now.
                frameCollected = 0;
            }
        }





        //The order of connected devices to SteamVR.
        string devicePattern = ""; 

        /// <summary>
        /// Just returns the amount of connected devices. Use this to start up how many devices are connected.
        /// 
        /// Copilot suggestion, seems very boiler-plately 
        /// </summary>
        /// <returns></returns>
        public int GetConnectedDeviceCount()
        {
            //Device pattern output.
            devicePattern = "";

            if (_vrSystem == null)
            {
                return 0;
            }
            var count = 0;

            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                var deviceClass = _vrSystem.GetTrackedDeviceClass(i);
                if (deviceClass != ETrackedDeviceClass.Invalid)
                {
                    var deviceRole = _vrSystem.GetControllerRoleForTrackedDeviceIndex((uint)i);
                    if (i == 0)
                    {
                        devicePattern += deviceRole + "_" + deviceClass;
                    }
                    else
                    {
                        devicePattern += "," + deviceRole + "_" + deviceClass;
                    }
                    count++;
                }
            }
            CreateDeviceIndexOrder(devicePattern);
            return count;
        }

        List<int> indexOrder = new List<int>();

        void CreateDeviceIndexOrder(string deviceOrder)
        {
            var order = deviceOrder.Split(",");

            List<string> preferredOrder = new List<string>() { "HMD", "LeftHand", "RightHand" };

            
            //Find duplicates and rename. 
            for (int i = 0; i < order.Length - 1; i++)
            {
                var device = order[i];
                int counter = 0; 
                foreach(string s in order)
                {
                    if(device == s)
                    {
                        var newName = device + "_" + counter.ToString();
                        while (true)
                        {
                            if(order.Contains(newName))
                            {
                                counter++;
                                newName = device + "_" + counter.ToString();
                            }
                            else
                            {
                                break;
                            }
                        }
                        order[i] = newName;
                    }
                }
            }


            //foreach (string s in order)
            //{
            //    foreach(string dup in order)
            //    {
            //        if()
            //    }
            //}




            List<string> outputDeviceOrder = new List<string>(order);


            

            Dictionary<string, int> originalIndices = outputDeviceOrder.Select((item, index) => new { item, index }).ToDictionary(pair => pair.item, pair => pair.index);
            StringComparator customCompare = new StringComparator(preferredOrder, originalIndices);
            outputDeviceOrder.Sort(customCompare);

            indexOrder.Clear();
            foreach(string device in outputDeviceOrder)
            {
                indexOrder.Add(originalIndices[device]);
            }
        }



        float startTime = 0.0f;
        bool  lockTime = false; 
        float timePassed = 0.0f;

        // If for external use; read only pls. 
        public Texture2D leftEyeTexture;
        public Texture2D rightEyeTexture;


        /// <summary> This declares a private variable 'recordingState' that determines if the system is recording.  <summary> 
        [SerializeField]
        bool recordingState = false;
        /// <summary> This function takes a 'Texture2D' input image a <summary> 
        private Texture2D InvertAndMirrorImage(Texture2D image)
        {
            Texture2D imageOut = new Texture2D(image.width, image.height);
            Texture2D texOut = new Texture2D(image.width, image.height);
            int x = image.width;
            int y = image.height;

            //Flip horizontally
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    imageOut.SetPixel(x - i - 1, j, image.GetPixel(i, j));
                }
            }
            imageOut.Apply();

            //Flip vertically
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    texOut.SetPixel(i, y - j - 1, imageOut.GetPixel(x - i - 1, j));
                }
            }
            texOut.Apply();
            return texOut;
        }

        /// <summary> Reads a texture from GPU memory, saves, it, and determines which eye's texture is being processed (left or right) <summary>
        private void RealTimeSaveTexture3(Texture2D texIn, string eyeSide)
        {
            var source = RenderTexture.GetTemporary(texIn.width, texIn.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            source.Create();
            Graphics.Blit(texIn, source);

            if (eyeSide == "leftEye/")
            {
                var req = AsyncGPUReadback.Request(source, 0, 0, texIn.width, 0, texIn.height, 0, source.volumeDepth, OnCompleteReadbackLeft);
                while (!req.done)
                {
                    //Spin.
                }
            }
            else
            {
                var req = AsyncGPUReadback.Request(source, 0, 0, texIn.width, 0, texIn.height, 0, source.volumeDepth, OnCompleteReadbackRight);
                while (!req.done)
                {
                    //Spin.
                }
            }
            source.Release();
        }

        /// <summary> Reads a texture from GPU memory, saves, it, and determines which eye's texture is being processed (left or right) <summary>
        IEnumerator<WaitForEndOfFrame> RealTimeSaveTexture2(Texture2D texIn, string eyeSide)
        {
            var source = RenderTexture.GetTemporary(texIn.width, texIn.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            source.Create();
            Graphics.Blit(texIn, source);

            if(eyeSide == "leftEye/")
            {
                var req = AsyncGPUReadback.Request(source,0, 0, texIn.width, 0, texIn.height, 0, source.volumeDepth, OnCompleteReadbackLeft);
                while (!req.done)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                var req = AsyncGPUReadback.Request(source, 0, 0, texIn.width, 0, texIn.height, 0, source.volumeDepth, OnCompleteReadbackRight);
                while (!req.done)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            source.Release();
        }
        /// <summary> Saves the texture to a location  <summary>
        private void OnCompleteReadbackLeft(AsyncGPUReadbackRequest request)
        {
            if (!request.hasError)
            {
                string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff");
                string outputLocation = loggingManager.GetPictureLocationStorage() + "leftEye/" + timeStamp + loggingManager.fileExtension;
                //string outputLocation = loggingManager.GetPictureLocationStorage() + "leftEye/" + timeStamp;
                Texture2D Destination = new Texture2D(request.width, request.height, TextureFormat.RGBA32, false);  // Create CPU texture array

                // Copy the data
                for (var i = 0; i < request.layerCount; i++)
                    Destination.SetPixels32(request.GetData<Color32>(i).ToArray(), i);

                byte[] bytesOut;
                Destination.Apply();
                //Destination = InvertAndMirrorImage(Destination);
                //bytesOut = Destination.GetRawTextureData();
                if (loggingManager.fileExtension == ".png")
                    bytesOut = Destination.EncodeToPNG();
                else
                    bytesOut = Destination.EncodeToJPG(95);

                Destroy(Destination);
                System.IO.File.WriteAllBytes(outputLocation, bytesOut);
            }
        }

        /// <summary> Saves the texture to a location <summary>
        private void OnCompleteReadbackRight(AsyncGPUReadbackRequest request)
        {
            if (!request.hasError)
            {
                string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff");
                string outputLocation = loggingManager.GetPictureLocationStorage() + "rightEye/" + timeStamp + loggingManager.fileExtension;
                //string outputLocation = loggingManager.GetPictureLocationStorage() + "rightEye/" + timeStamp;
                Texture2D Destination = new Texture2D(request.width, request.height, TextureFormat.RGBA32, false);  // Create CPU texture array

                // Copy the data
                for (var i = 0; i < request.layerCount; i++)
                    Destination.SetPixels32(request.GetData<Color32>(i).ToArray(), i);

                byte[] bytesOut;
                Destination.Apply();

                //Destination = InvertAndMirrorImage(Destination);
                //bytesOut = Destination.GetRawTextureData();
                if (loggingManager.fileExtension == ".png")
                    bytesOut = Destination.EncodeToPNG();
                else
                    bytesOut = Destination.EncodeToJPG(95);

                Destroy(Destination);
                System.IO.File.WriteAllBytes(outputLocation, bytesOut);
            }
        }

        /// <summary> Collects the texture <summary>
        public Texture2D CollectTextureFromEye(EVREye eye)
        {
            var tex = new Texture2D(2, 2);
            var nativeTex = System.IntPtr.Zero;
            var status = OpenVR.Compositor.GetMirrorTextureD3D11(eye, tex.GetNativeTexturePtr(), ref nativeTex);
            if (status == EVRCompositorError.None)
            {
                try
                {
                    uint width = 0, height = 0;
                    OpenVR.System.GetRecommendedRenderTargetSize(ref width, ref height);
                    tex = Texture2D.CreateExternalTexture((int)width, (int)height, TextureFormat.RGBA32, true, true, nativeTex);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                Debug.LogError("Something happened while collecting the eye texture!");
                Debug.LogError(status);
            }
            return tex;
        }





        /// <summary> Updates tracker bindings <summary>
        private void UpdateSteamVrTrackerBindings()
        {
            var trackerBindings = GetSteamVrTrackerBindings();
            SteamVrTrackerBindings = trackerBindings;
            OVRT_Events.TrackerRolesChanged.Invoke();
        }

        private static bool runningTemporarySession = false;
        /// <summary> Initializws a temporary VR session within Unity editor if certain conditions are met <summary>
        public static bool InitializeTemporarySession()
        {
            if (Application.isEditor)
            {
                //bool needsInit = (!active && !usingNativeSupport && !runningTemporarySession);

                EVRInitError initError = EVRInitError.None;
                OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref initError);
                bool needsInit = initError != EVRInitError.None;

                if (needsInit)
                {
                    EVRInitError error = EVRInitError.None;
                    OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);

                    if (error != EVRInitError.None)
                    {
                        Debug.LogError("[OVRT] Could not initialize OpenVR tracking: " + error.ToString());
                        return false;
                    }

                    runningTemporarySession = true;
                }


                return needsInit;
            }

            return false;
        }
        /// <summary> Exits and shuts down a temporary VR session if one is active <summary>
        public static void ExitTemporarySession()
        {
            if (runningTemporarySession)
            {
                OpenVR.Shutdown();
                runningTemporarySession = false;
            }
        }
    }



    class StringComparator : IComparer<string>
    {
        private readonly HashSet<string> preferredOrder;
        private readonly Dictionary<string, int> originalIndices;

        public StringComparator(IEnumerable<string> _inputOrder, Dictionary<string, int> _orgIndexes)
        {
            this.preferredOrder = new HashSet<string>(_inputOrder, StringComparer.OrdinalIgnoreCase);
            this.originalIndices = _orgIndexes;
        }

        public int Compare(string x, string y)
        {
            bool xIsSpecial = preferredOrder.Contains(x);
            bool yIsSpecial = preferredOrder.Contains(y);

            if (xIsSpecial && yIsSpecial)
            {
                // If both x and y are special, maintain their original order
                return 0;
            }
            else if (xIsSpecial)
            {
                // x is special, so it comes before y
                return -1;
            }
            else if (yIsSpecial)
            {
                // y is special, so it comes before x
                return 1;
            }
            else
            {
                // Neither x nor y is special, use default string comparison
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
            }
        }
    }


}


