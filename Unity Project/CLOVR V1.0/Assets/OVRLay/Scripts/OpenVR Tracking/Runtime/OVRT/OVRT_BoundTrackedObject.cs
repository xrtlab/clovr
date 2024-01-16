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
//using UnityEngine.XR;
namespace OVRT
{
    /// <summary>
    /// Maps tracked OpenVR poses to transform by serial number bindings. Can use tracker roles defined by SteamVR.
    /// </summary>
    public sealed class OVRT_BoundTrackedObject : OVRT_TrackedDevice
    {
        public string binding;
        [Tooltip("If not set, relative to parent")]

        private UnityAction<string, TrackedDevicePose_t, int> _onNewBoundPoseAction;
        private UnityAction _onTrackerRolesChanged;

        private void OnDeviceConnected(int index, bool connected)
        {
            if (DeviceIndex == index && !connected)
            {
                IsConnected = false;
            }
        }

        private void OnNewBoundPose(string binding, TrackedDevicePose_t pose, int deviceIndex)
        {
            if (this.binding != binding)
                return;

            IsValid = false;

            if (DeviceIndex != deviceIndex)
            {
                DeviceIndex = deviceIndex;
                onDeviceIndexChanged.Invoke(DeviceIndex);
            }
            IsConnected = pose.bDeviceIsConnected;

            if (!pose.bDeviceIsConnected)
                return;

            if (!pose.bPoseIsValid)
                return;

            IsValid = true;

            var rigidTransform = new OVRT_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                transform.position = origin.transform.TransformPoint(rigidTransform.pos);
                transform.rotation = origin.rotation * rigidTransform.rot;
            }
            else
            {
                transform.localPosition = rigidTransform.pos;
                transform.localRotation = rigidTransform.rot;
            }
        }

        private void OnTrackerRolesChanged()
        {
            IsValid = false;
            IsConnected = false;
        }

        private void Awake()
        {
            _onNewBoundPoseAction += OnNewBoundPose;
            _onDeviceConnectedAction += OnDeviceConnected;
            _onTrackerRolesChanged += OnTrackerRolesChanged;
        }

        private void OnEnable()
        {
            OVRT_Events.NewBoundPose.AddListener(_onNewBoundPoseAction);
            OVRT_Events.TrackedDeviceConnected.AddListener(_onDeviceConnectedAction);
            OVRT_Events.TrackerRolesChanged.AddListener(_onTrackerRolesChanged);
        }

        private void OnDisable()
        {
            OVRT_Events.NewBoundPose.RemoveListener(_onNewBoundPoseAction);
            OVRT_Events.TrackedDeviceConnected.RemoveListener(_onDeviceConnectedAction);
            OVRT_Events.TrackerRolesChanged.RemoveListener(_onTrackerRolesChanged);
            IsValid = false;
            IsConnected = false;
        }
    }
}