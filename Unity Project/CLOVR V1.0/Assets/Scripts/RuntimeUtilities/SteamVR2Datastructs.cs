using Valve.VR;
using System.Linq;
using System.ComponentModel;
 
using System.Collections.Generic;
using System;
 
namespace XRT_OVR_Grabber 
{
    [Serializable]
    public class DeviceActionSet 
    {
        /// <summary> Represents a group of actions related to some specific purpose </summary>        private class VRActionSet
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

    /// <summary> Taken from SteamVR </summary> 
    public enum SteamVR_Input_Sources_VR
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
}
