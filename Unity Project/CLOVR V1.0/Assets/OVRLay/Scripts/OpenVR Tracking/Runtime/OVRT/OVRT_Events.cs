using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using System.Collections;
using XRT_OVR_Grabber;
namespace OVRT
{
    public static class OVRT_Events
    {
        public static UnityEvent<int, bool> TrackedDeviceConnected = new UnityEvent<int, bool>();

        //public static UnityEvent<TrackedDevicePose_t[], XRT_OVR_Grabber.Pose[]> NewPoses = new UnityEvent<TrackedDevicePose_t[], XRT_OVR_Grabber.Pose[]>();
        public static UnityEvent<TrackedDevicePose_t[]> NewPoses = new UnityEvent<TrackedDevicePose_t[]>();
        public static UnityEvent<string, TrackedDevicePose_t, int> NewBoundPose = new UnityEvent<string, TrackedDevicePose_t, int>();

        public static UnityEvent TrackerRolesChanged = new UnityEvent();

        public static UnityEvent<bool> HideRenderModelsChanged = new UnityEvent<bool>();
        public static UnityEvent ModelSkinSettingsHaveChanged = new UnityEvent();

        public static UnityEvent<OVRT_RenderModel, bool> RenderModelLoaded = new UnityEvent<OVRT_RenderModel, bool>();
    }
}