//MIT License

//Copyright (c) 2023 biosmanager

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using XRT_OVR_Grabber;
using System.Collections;

namespace OVRT
{
    /// <summary>
    /// Maps tracked OpenVR poses to transform by device index.
    /// </summary>
    public sealed class OVRT_TrackedObject : OVRT_TrackedDevice
    {
        public enum EIndex
        {
            None = -1,
            Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
            Device1,
            Device2,
            Device3,
            Device4,
            Device5,
            Device6,
            Device7,
            Device8,
            Device9,
            Device10,
            Device11,
            Device12,
            Device13,
            Device14,
            Device15,
            Device16
        }



        public EIndex index;
        [Tooltip("If not set, relative to parent")]

        //private UnityAction<TrackedDevicePose_t[], XRT_OVR_Grabber.Pose[]> _onNewPosesAction;
        private UnityAction<TrackedDevicePose_t[]> _onNewPosesAction;
        private void OnDeviceConnected(int index, bool connected)
        {
            if ((int)this.index == index)
            {
                DeviceIndex = index;
                IsConnected = connected;

                onDeviceIndexChanged.Invoke(DeviceIndex);
            }
        }

        

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            //Debug.Log("I was invoked");


            if (index == EIndex.None)
                return;

            var i = DeviceIndex;

            IsValid = false;

            if (i < 0 || poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            IsValid = true;

            var pose = new OVRT_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                transform.position = origin.transform.TransformPoint(pose.pos);
                transform.rotation = origin.rotation * pose.rot;
            }
            else
            {
                transform.localPosition = pose.pos;
                transform.localRotation = pose.rot;
            }

            //Insert collection code after here. 
            //XRT_OVR_Grabber.Pose logPose = new XRT_OVR_Grabber.Pose(poses[i], pose.pos, pose.rot, Time.frameCount,Time.realtimeSinceStartup);
            //loggedPose[i] = logPose; 
        }

        private void Awake()
        {
            _onNewPosesAction += OnNewPoses;
            _onDeviceConnectedAction += OnDeviceConnected;
        }

        private void OnEnable()
        {
            DeviceIndex = (int)index;
            onDeviceIndexChanged.Invoke(DeviceIndex);

            OVRT_Events.NewPoses.AddListener(_onNewPosesAction);
            OVRT_Events.TrackedDeviceConnected.AddListener(_onDeviceConnectedAction);
        }

        private void OnDisable()
        {
            OVRT_Events.NewPoses.RemoveListener(_onNewPosesAction);
            IsValid = false;
            IsConnected = false;
        }
    }
}