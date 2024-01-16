//MIT License

//Copyright (c) 2018 sh - akira

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

/// <summary> 
/// This file is focused on gathering input events from the SteamVR interface. The script takes in various user interactions in virtual reality (VR) such as buttons or analog stick movements
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Valve.VR;
using XRT_OVR_Grabber;
using UnityEngine.Events;
/* This script was borrowed from the VirtualMotionCapture Project found at https://github.com/sh-akira/VirtualMotionCapture
 * 
 * As of October 19, 2023 this repository was MIT licensed and thus this script was used for this. 
 */


namespace XRT_OVR_Grabber
{
    /// <summary> This is the main class handling interactions with SteamVR. It is focused particularly on tracking input events and translating them for use within Unity </summary>
    public class SteamVR2Input : MonoBehaviour
    {
        public static SteamVR2Input Instance;

        [SerializeField]
        public bool leftController = false;
        [SerializeField]
        public bool rightController = false;

        [SerializeField]
        private LoggingManagerAPI loggerMang;

        public EventHandler<OVRKeyEventArgs> KeyDownEvent;
        public EventHandler<OVRKeyEventArgs> KeyUpEvent;
        public EventHandler<OVRKeyEventArgs> AxisChangedEvent;

        private bool initialized = false;

        private static uint activeActionSetSize = 0;
        private static uint skeletalActionData_size = 0;
        private static uint digitalActionData_size = 0;
        private static uint analogActionData_size = 0;

        public EVRSkeletalMotionRange rangeOfMotion;
        public EVRSkeletalTransformSpace skeletalTransformSpace;
        //public HandTracking_Skeletal handTracking_Skeletal;

        public static bool EnableSkeletal = true;

        private VRActiveActionSet_t[] rawActiveActionSetArray;
        private List<VRActionSet> ActionSetList = new List<VRActionSet>();

        private SteamVRActions CurrentActionData;

        public bool handSwap = false;



        /// External interaction pose record.
        public XRT_OVR_Grabber.ControllerInteraction interactionUp;
        public XRT_OVR_Grabber.ControllerInteraction interactionDown;

        public Queue interactionQueue = new Queue();
        public Queue analogInteractionsQueue = new Queue(); 

        /// <summary> Stores the current instance of the script in a static variable for global acess </summary>
        void OnEnable()
        {
            Instance = this;
        }

        /// <summary> Represents a group of actions related to some specific purpose </summary>
        [Serializable]
        private class VRActionSet
        {
            public string actionSetPath;
            public ulong ulActionSet;
            public ulong ulRestrictedToDevice;
            public bool IsLeft;
            public string InputSourcePath;
        }

        /// <summary> Represents a specific command or task the user performs such as pressing a button </summary>
        [Serializable]
        public class SteamVRAction
        {
            public string name;
            public string type;
            public string skeleton;
            public ulong handle;

            private string shortName = null;
            public string ShortName
            {
                get
                {
                    if (shortName == null) shortName = name.Split('/').LastOrDefault();
                    return shortName;
                }
            }

            public InputDigitalActionData_t digitalActionData = new InputDigitalActionData_t();
            public InputDigitalActionData_t lastDigitalActionData = new InputDigitalActionData_t();
            public InputAnalogActionData_t analogActionData = new InputAnalogActionData_t();
            public InputAnalogActionData_t lastAnalogActionData = new InputAnalogActionData_t();
        }

        /// <summary> Defines a set of VR actions </summary>
        [Serializable]
        public class SteamVRActionSet
        {
            public string name;
            public string usage;
        }

        /// <summary> Defines default bidning information for a VR action. </summary>
        [Serializable]
        public class SteamVRDefaultBinding
        {
            public string controller_type;
            public string binding_url;
        }

        /// <summary> Collection of multiple VR related configurations </summary>
        [Serializable]
        public class SteamVRActions
        {
            public List<SteamVRAction> actions;
            public List<SteamVRActionSet> action_sets;
            public List<SteamVRDefaultBinding> default_bindings;
        }

        private Dictionary<string, Vector3> LastPositions = new Dictionary<string, Vector3>();

        /// <summary>  <summary>
        private void Awake()
        {
            loggerMang = GameObject.Find("LoggingManger").GetComponent<LoggingManagerAPI>();
            interactionUp = new ControllerInteraction();
            interactionDown = new ControllerInteraction();
        }
        /// <summary> Returns last known postion associated with a given action  </summary>
        private Vector3 GetLastPosition(string shortName)
        {
            Vector3 axis = Vector3.zero;
            var partname = shortName.Substring("Touch".Length);
            var key = LastPositions.Keys.FirstOrDefault(d => d.Contains(partname));
            if (key != null)
            {
                axis = LastPositions[key];
            }
            return axis;
        }
        /// <summary> Retrieves a string property for a tracked VR device <summary>
        public string GetStringProperty(ETrackedDeviceProperty prop, uint deviceId)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var capacity = OpenVR.System.GetStringTrackedDeviceProperty(deviceId, prop, null, 0, ref error);
            if (capacity > 1)
            {
                var result = new System.Text.StringBuilder((int)capacity);
                OpenVR.System.GetStringTrackedDeviceProperty(deviceId, prop, result, capacity, ref error);
                return result.ToString();
            }
            return (error != ETrackedPropertyError.TrackedProp_Success) ? error.ToString() : "<unknown>";
        }
        /// <summary> Updates the current state of the SteamVR input system </summary>
        public void FrameUpdate()
        {
            EVRInputError err;

            if (initialized == false)
            {
                //var fullPath = Path.Combine(Application.dataPath, "Resources/Config/actions.json");// = SteamVR_Input.GetActionsFilePath();
                string fullPath = ""; 
                if (loggerMang.openVRRuntimeConfig != "")
                {
                    fullPath = loggerMang.openVRRuntimeConfig;

                    if (!fullPath.Contains("actions.json"))
                    {
                        fullPath = Path.Combine(fullPath, "actions.json");
                    }

                }    
                else
                    fullPath = Path.Combine(Application.streamingAssetsPath, "Config/actions.json");




                if (OpenVR.Input == null)
                {
                    return;
                }

                //We need to check if the OpenVR Config file exists or not. 
                if(!File.Exists(fullPath))
                {
                    return;
                }

                if (activeActionSetSize == 0)
                    activeActionSetSize = (uint)(Marshal.SizeOf(typeof(VRActiveActionSet_t)));
                if (skeletalActionData_size == 0)
                    skeletalActionData_size = (uint)Marshal.SizeOf(typeof(InputSkeletalActionData_t));
                if (digitalActionData_size == 0)
                    digitalActionData_size = (uint)Marshal.SizeOf(typeof(InputDigitalActionData_t));
                if (analogActionData_size == 0)
                    analogActionData_size = (uint)Marshal.SizeOf(typeof(InputAnalogActionData_t));

                rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
                skeletalTransformSpace = EVRSkeletalTransformSpace.Parent;

                
                err = OpenVR.Input.SetActionManifestPath(fullPath);
                if (err != EVRInputError.None)
                    Debug.LogError($"<b>[SteamVR]</b> Error loading action manifest into SteamVR: {err}");

                //parse actions.json
                var json = File.ReadAllText(fullPath);// + "/actions.json");
                CurrentActionData = JsonUtility.FromJson<SteamVRActions>(json);

                foreach (var action in CurrentActionData.actions)
                {
                    err = OpenVR.Input.GetActionHandle(action.name, ref action.handle);
                    if (err != EVRInputError.None)
                        Debug.LogError($"<b>[SteamVR]</b> GetActionHandle error ({action.name}): {err}");
                }

                initialized = true;

                var actionSetPath = CurrentActionData.action_sets.First().name;
                ulong actionSetHandle = 0;
                err = OpenVR.Input.GetActionSetHandle(actionSetPath, ref actionSetHandle);
                if (err != EVRInputError.None)
                    Debug.LogError($"<b>[SteamVR]</b> GetActionSetHandle error ({actionSetPath}): {err}");


                var inputSourceNames = System.Enum.GetNames(typeof(SteamVR_Input_Sources));
                foreach (var inputSourceName in inputSourceNames)
                {
                    ulong inputSourceHandle = 0;
                    var inputSourcePath = GetPath(inputSourceName); // Any,LeftHand,RightHand,...
                    err = OpenVR.Input.GetInputSourceHandle(inputSourcePath, ref inputSourceHandle);
                    if (err != EVRInputError.None)
                        Debug.LogError($"<b>[SteamVR]</b> GetInputSourceHandle error ({inputSourcePath}): {err}");
                    else
                    {
                        ActionSetList.Add(new VRActionSet
                        {
                            actionSetPath = inputSourceName,
                            ulActionSet = actionSetHandle,
                            ulRestrictedToDevice = inputSourceHandle,
                            InputSourcePath = inputSourcePath,
                            IsLeft = inputSourcePath.Contains("left") != handSwap,
                        });
                    }
                }

                rawActiveActionSetArray = ActionSetList.Select(d => new VRActiveActionSet_t
                {
                    ulActionSet = d.ulActionSet,
                    nPriority = 0,//同プライオリティのアクションセットが複数ある場合同時に実行される
                    ulRestrictedToDevice = d.ulRestrictedToDevice
                }).ToArray();

                if (OpenVR.Compositor != null) OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseStanding);
            }

            //すべてのActionSetに対して新しいイベントがないか更新する
            err = OpenVR.Input.UpdateActionState(rawActiveActionSetArray, activeActionSetSize);
            if (err != EVRInputError.None)
                Debug.LogError($"<b>[SteamVR]</b> UpdateActionState error: {err}");

            foreach (var actionset in ActionSetList)
            {
                foreach (var action in CurrentActionData.actions)
                {

                    //
                    if (action.type == "boolean")
                    {
                        //オンオフ系のデータ取得(クリックやタッチ)
                        action.lastDigitalActionData = action.digitalActionData;
                        err = OpenVR.Input.GetDigitalActionData(action.handle, ref action.digitalActionData, digitalActionData_size, actionset.ulRestrictedToDevice);
                        if (err != EVRInputError.None)
                        {
                            Debug.LogWarning($"<b>[SteamVR]</b> GetDigitalActionData error ({action.name}): {err} handle: {action.handle}");
                            continue;
                        }
                        //Debug.Log($"<b>[SteamVR]</b> ANY ACTION ({action.name}): {err} handle: {action.handle} : {action.ShortName} : {action.digitalActionData.bState}");
                        if (IsKeyDown(action.digitalActionData))
                        {
                            Debug.Log($"<b>[SteamVR]</b> GetDigitalActionData IsKeyDown ({action.name}): {err} handle: {action.handle} : {action.ShortName}");

                            bool isTouch = action.ShortName.StartsWith("Touch") && action.ShortName.Contains("Trigger") == false;
                            Vector3 axis = isTouch ? GetLastPosition(action.ShortName) : Vector3.zero;
                            KeyDownEvent?.Invoke(this, new OVRKeyEventArgs(action.ShortName, axis, actionset.IsLeft != handSwap, axis != Vector3.zero, isTouch));

                            interactionUp = new ControllerInteraction(action.digitalActionData, Time.realtimeSinceStartup, Time.frameCount, action.ShortName, actionset.actionSetPath);
                            interactionUp.SetHeaderActionSet(System.Enum.GetNames(typeof(SteamVR_Input_Sources)));
                            interactionQueue.Enqueue(interactionUp);
                        }

                        if (IsKeyUp(action.digitalActionData))
                        {
                            Debug.Log($"<b>[SteamVR]</b> GetDigitalActionData IsKeyUp ({action.name}): {err} handle: {action.handle} : {action.ShortName} : {actionset.actionSetPath}");
                            bool isTouch = action.ShortName.StartsWith("Touch") && action.ShortName.Contains("Trigger") == false;
                            Vector3 axis = isTouch ? GetLastPosition(action.ShortName) : Vector3.zero;
                            KeyUpEvent?.Invoke(this, new OVRKeyEventArgs(action.ShortName, axis, actionset.IsLeft != handSwap, axis != Vector3.zero, isTouch));
                            
                            interactionDown = new ControllerInteraction(action.digitalActionData, Time.realtimeSinceStartup, Time.frameCount, action.name, actionset.actionSetPath);
                            interactionDown.SetHeaderActionSet(System.Enum.GetNames(typeof(SteamVR_Input_Sources)));
                            interactionQueue.Enqueue(interactionDown); 
                        }

                    }
                    else if (action.type == "vector1" || action.type == "vector2" || action.type == "vector3")
                    {
                        //アナログ入力のデータ取得(スティックやタッチパッド)
                        action.lastAnalogActionData = action.analogActionData;
                        err = OpenVR.Input.GetAnalogActionData(action.handle, ref action.analogActionData, analogActionData_size, actionset.ulRestrictedToDevice);
                        if (err != EVRInputError.None)
                        {
                            Debug.LogWarning($"<b>[SteamVR]</b> GetAnalogActionData error ({action.name}): {err} handle: {action.handle}");
                            continue;
                        }
                        //Debug.Log($"<b>[SteamVR]</b> GetAnalogActionData Position:{action.analogActionData.x},{action.analogActionData.y} ({action.name}): {err} handle: {action.handle} : {actionset.actionSetPath}");
                        var axis = new Vector3(action.analogActionData.x, action.analogActionData.y, action.analogActionData.z);
                        if (axis != Vector3.zero)
                        {
                            LastPositions[action.name] = axis;
                            AxisChangedEvent?.Invoke(this, new OVRKeyEventArgs(action.ShortName, axis, actionset.IsLeft != handSwap, true, false));
                        

                            //Composition of an analog interaction.
                            ControllerInteraction analogInteraction = new ControllerInteraction(
                                action.analogActionData, 
                                Time.realtimeSinceStartup, 
                                Time.frameCount, 
                                action.name, 
                                actionset.IsLeft, 
                                action.handle,
                                action.name,
                                actionset.actionSetPath
                                );
                            analogInteraction.SetHeaderActionSet(System.Enum.GetNames(typeof(SteamVR_Input_Sources)));
                            interactionQueue.Enqueue(analogInteraction);
                        }
                        //AnalogInteraction a = new AnalogInteraction(action.analogActionData, action.ShortName, actionset.IsLeft, action.handle);
                        //analogInteractionsQueue.Enqueue(a); 
                    }
                    else if (action.type == "skeleton")
                    {

                        //if (EnableSkeletal)
                        {
                            //実際にBoneのTransformを取得する
                            //rangeOfMotionは実際のコントローラーの形に指を曲げる(WithController)か、完全にグーが出来るようにする(WithoutController)か
                            
                            //Please assume this is running on the latest version of whatever SteamVR is running on...
                            var tempBoneTransforms = new VRBoneTransform_t[31]; //[SteamVR_Action_Skeleton.numBones];
                            err = OpenVR.Input.GetSkeletalBoneData(action.handle, skeletalTransformSpace, rangeOfMotion, tempBoneTransforms);
                            if (err != EVRInputError.None)
                            {
                                //特定の条件においてものすごい勢いでログが出る
                                Debug.LogWarning($"<b>[SteamVR]</b> GetDigitalActionData error ({action.name}): {err} handle: {action.handle}");
                                continue;
                            }
                            //Debug.Log($"<b>[SteamVR]</b> GetDigitalActionData error ({tempBoneTransforms[0]}): {err} handle: {action.handle}");

                            //handTracking_Skeletal.SetSkeltalBoneData(action.name.Contains("Left") != handSwap, tempBoneTransforms);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This gets the last interaction done by the user. This is primarily used to collect the moment the button press goes up as well. 
        /// </summary>
        /// <returns></returns>
        public ControllerInteraction GetLastInteraction()
        {
            ControllerInteraction outInteraction = new ControllerInteraction();
            if (interactionUp.controllerIndex != "")
                outInteraction = interactionUp;
            else if (interactionDown.controllerIndex != "")
                outInteraction = interactionDown;
 
            interactionUp = new ControllerInteraction();
            interactionDown = new ControllerInteraction();

            return outInteraction;
        }


        /// <summary> Setting up a function for pulling information using the queue instead of the flipflop interaction. </summary> 
        public List<ControllerInteraction> GetLastInteractions()
        {
            List<ControllerInteraction> outInteractions = new List<ControllerInteraction>();
            while (interactionQueue.Count > 0)
            {
                ControllerInteraction c = (ControllerInteraction) interactionQueue.Dequeue();
                outInteractions.Add(c);
            }
            return outInteractions;
        }


        float timePassed = 0.0f;
        float rateLimit = 1.0f;

        /// <summary> Update is called several times per frame </summary>
        void Update()
        {
            if (timePassed > rateLimit)
            {
                StartCoroutine(CallFrameUpdate());
                timePassed = 0.0f;
            }
            else
            {
                timePassed += Time.deltaTime;
                return;
            }
        }

        /// <summary> This version of the update loop will run in corotine instead. </summary> 
        int rateHz = 72;
        IEnumerator CallFrameUpdate()
        {
            int currentTick = 0;
            while (currentTick < rateHz)
            {
                FrameUpdate();
                //Debug.Log("Tomatoes are potatoes")
                currentTick++;
                yield return null;
            }
        }


        /// <summary> Returns true if a digital action is currently active and has just changed state </summary> 
        private bool IsKeyDown(InputDigitalActionData_t actionData)
        {
            return actionData.bState && actionData.bChanged;
        }
        /// <summary> Returns true if a digital action is currently not active and has just changed state </summary> 
        private bool IsKeyUp(InputDigitalActionData_t actionData)
        {
            return actionData.bState == false && actionData.bChanged;
        }
        //Valve.VR.Input

        private static Type enumType = typeof(SteamVR_Input_Sources);
        private static Type descriptionType = typeof(DescriptionAttribute);
        private static string GetPath(string inputSourceEnumName)
        {
            return ((DescriptionAttribute)enumType.GetMember(inputSourceEnumName)[0].GetCustomAttributes(descriptionType, false)[0]).Description;
        }
    }

    /// <summary> Taken from SteamVR </summary> 
    public enum SteamVR_Input_Sources
    {
        [Description("/unrestricted")] //todo: check to see if this gets exported: k_ulInvalidInputHandle
        Any,

        [Description("/user/hand/left")]
        LeftHand,

        [Description("/user/hand/right")]
        RightHand,

        [Description("/user/foot/left")]
        LeftFoot,

        [Description("/user/foot/right")]
        RightFoot,

        [Description("/user/shoulder/left")]
        LeftShoulder,

        [Description("/user/shoulder/right")]
        RightShoulder,

        [Description("/user/waist")]
        Waist,

        [Description("/user/chest")]
        Chest,

        [Description("/user/head")]
        Head,

        [Description("/user/gamepad")]
        Gamepad,

        [Description("/user/camera")]
        Camera,

        [Description("/user/keyboard")]
        Keyboard,

        [Description("/user/treadmill")]
        Treadmill,
    }


    public class OVRKeyEventArgs : EventArgs
    {
        public string Name { get; }
        public Vector3 Axis { get; }
        public bool IsLeft { get; }
        public bool IsAxis { get; }
        public bool IsTouch { get; }

        public OVRKeyEventArgs(string name, Vector3 axis, bool isLeft, bool isAxis, bool isTouch) : base()
        {
            Name = name; Axis = axis; IsLeft = isLeft; IsAxis = isAxis; IsTouch = isTouch;
        }
    }
}


/*
TrackedDevicePose_t[] _poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0.0f, _poses);
for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
{
    if (_poses[i].bDeviceIsConnected && _poses[i].bDeviceIsConnected)
    {
        string serialNumber = GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String, i);
        if (action.digitalActionData.activeOrigin.ToString() == serialNumber)
        {
            Debug.Log("Inside Sublink");
        }
        Debug.Log(serialNumber);

    }

    Debug.Log(action.digitalActionData.activeOrigin.ToString());

}*/