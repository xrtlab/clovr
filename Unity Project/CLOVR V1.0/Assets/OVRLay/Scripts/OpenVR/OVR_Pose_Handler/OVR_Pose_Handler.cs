//MIT License

//Copyright (c) 2017 Ben Otter

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
using System.Collections;
using UnityEngine;
using Valve.VR;

public partial class OVR_Pose_Handler 
{
    static private OVR_Pose_Handler _instance;
    static public OVR_Pose_Handler instance 
    {
        get {
            if(_instance == null)
                _instance = new OVR_Pose_Handler();

            return _instance;
        }
    }

    static private OVR_Handler OVR { get { return OVR_Handler.instance; } }

    static private CVRSystem VRSystem { get { return OVR.VRSystem; } }
    static private CVRCompositor Compositor { get { return OVR.Compositor; } }
    
    static private bool CompExists { get { return Compositor != null; } }
    static private bool SysExists { get { return VRSystem != null; } }

    public ETrackingUniverseOrigin trackingSpace = ETrackingUniverseOrigin.TrackingUniverseStanding;
    public TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
    public TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];

	public uint hmdIndex = OpenVR.k_unTrackedDeviceIndex_Hmd;
	public uint rightIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
	public uint leftIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

    public bool rightActive { get { return rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid; } }
    public bool leftActive { get { return leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid; } }

    public void UpdatePoses()
    {
        if(!CompExists || !SysExists)
            return;

		rightIndex = VRSystem.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
		leftIndex = VRSystem.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);

        Compositor.GetLastPoses(poses, gamePoses);
    }

    public void SetTransformToTrackedDevice(Transform t, uint ind)
    {
        var pose = new OVR_Utils.RigidTransform(poses[ind].mDeviceToAbsoluteTracking);

        t.position = pose.pos;
        t.rotation = pose.rot;
    }
}