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