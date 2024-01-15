using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


/// <summary>
/// This is a log for a single record at a frame recorded for a device. 
/// </summary>
namespace XRT_OVR_Grabber
{
    public class Pose
    {
        Vector3 position;
        Vector3 velocity;
        Vector3 angularVelocity;
        Quaternion rotation;
        public int frameCollected = 0;
        public float timeCollected = 0.0f;
        string deviceType  = "";
        string deviceRole  = "";
        string deviceClass = ""; 


        public Pose(){}
        public Pose(TrackedDevicePose_t tracker, Vector3 pos, Quaternion rot, int frame, float _timeCollected) 
        {
            SetPoseDetails(tracker, pos, rot,frame);
            timeCollected = _timeCollected;
        }

        public void SetPoseDetails(TrackedDevicePose_t tracker, Vector3 pos, Quaternion rot, int _frameCollected)
        {
            frameCollected = _frameCollected;
            position = new Vector3(pos.x,pos.y,pos.z);
            rotation = new Quaternion(rot.x,rot.y,rot.z,rot.w);
            velocity = new Vector3(tracker.vVelocity.v0, tracker.vVelocity.v1, tracker.vVelocity.v2);
            angularVelocity = new Vector3(tracker.vAngularVelocity.v0, tracker.vAngularVelocity.v1, tracker.vAngularVelocity.v2);
            
        }

        public void SetPoseDetails(Vector3 pos, Quaternion rot)
        {
            position = new Vector3(pos.x, pos.y, pos.z);
            rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w); ; 
        }

        public void SetDeviceClass(string _classDevice)
        {
            deviceClass = _classDevice;
        }
        public void SetDeviceRole(string role)
        {
            deviceRole = role; 
        }

        public int GetFrameRecorded()
        {
            return frameCollected;
        }

        public float GetTimeRecorded()
        {
            return timeCollected;
        }

        public static string PrintHeader()
        {
            return "Device Type and Role, Position X, Position Y, Position Z, Rotation Z, Rotation Y, Rotation Z, Rotation W, Velocity X, Velocity Y, Velocity Z, Ang. Velocity X, Ang. Velocity Y, Ang. Velocity Z";
        }

        public string SendToString()
        {
            return  deviceClass + " " +deviceRole + "," + 
                position.x + "," + position.y + "," + position.z + "," +
                rotation.x + "," + rotation.y + "," + rotation.z + "," + rotation.w + "," +
                velocity.x + "," + velocity.y + "," + velocity.z + "," +
                angularVelocity.x + "," + angularVelocity.y + "," + angularVelocity.z;
        }
    }

    public struct PoseStruct
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public Quaternion rotation;
        public int frameCollected;
        public float timeCollected;
        public string deviceType;
        public string deviceRole;
        public string deviceClass;
    }

    /// <summary> Represents a log of poses and controller interactions <summary> 
    public class PoseInteractionLog
    {
        public List<XRT_OVR_Grabber.Pose> poses;
        public List<ControllerInteraction> interactions;
        public ControllerInteraction interaction;

        //Overloading for similar actions. 
        public PoseInteractionLog(List<XRT_OVR_Grabber.Pose> _poses, ControllerInteraction _interaction)
        {
            poses = _poses;
            interaction = _interaction;
        }

        public PoseInteractionLog(List<XRT_OVR_Grabber.Pose> _poses, List<ControllerInteraction> _interaction)
        {
            poses = _poses;
            interactions = _interaction;
        }
    }
}