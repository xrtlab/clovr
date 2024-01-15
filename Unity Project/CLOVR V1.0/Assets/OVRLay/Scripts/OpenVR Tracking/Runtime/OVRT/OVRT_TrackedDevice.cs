using UnityEngine;
using UnityEngine.Events;
using XRT_OVR_Grabber;

namespace OVRT
{
    public abstract class OVRT_TrackedDevice : MonoBehaviour
    {
        public Transform origin;
        public int DeviceIndex { get; protected set; } = -1;

        public int HandleIndex { get; protected set; } = -1;
        public bool IsValid { get; protected set; }
        public bool IsConnected { get; protected set; }

        public UnityEvent<int> onDeviceIndexChanged;

        protected UnityAction<int, bool> _onDeviceConnectedAction;
    }
}