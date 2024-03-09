using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// Will be using https://github.com/sh-akira/VirtualMotionCapture/blob/master/Assets/Scripts/Avatar/HandTracking/HandTracking_Skeletal.cs#L305
// as a starter reference. This primarily is just for the data structure of the skeleton
// data. SteamVR as of right now, reports 31 bones in its OpenVR.Input.GetSkeletalBoneData call.
namespace XRT_OVR_Grabber 
{
    public struct SteamVRHandBoneJoint
    {
        public HmdVector4_t position;
        public HmdQuaternionf_t orientation;
    }

    public class SkeletonHandData 
    {
        Vector3[] skeletonJointPositions = new Vector3[31];
        Quaternion[] skeletonJointRotations = new Quaternion[31];
        bool handIsLeft = false;
        int maxBones = 31; //If using the SteamVR API, this number is 31. If you decide to upgrade this, with
                           // the official API, update this.
        public void UpdateMaxBoneSupport(int index)
        {
            maxBones = index; 
        }

        //Default empty. 
        public SkeletonHandData()
        {
            for(int i = 0; i < maxBones; i++)
            {
                skeletonJointPositions[i] = Vector3.zero;
            }

            for (int i = 0; i < maxBones; i++)
            {
                skeletonJointRotations[i] = new Quaternion(0,0,0,0);
            }
        }

        // The data goes from top to bottom. 
        // For now I will only refer to GetSkeletalBoneData. However I will leave this note from
        // VMC in its use. 
        public SkeletonHandData(bool isLeft, VRBoneTransform_t[] jointData)
        {
            handIsLeft = isLeft;
            // SteamVR's coordinate system is right handed, and Unity's is left handed. I will leave it
            // as-is. This is because this project primarily centers around OpenVR, not unity. 
            for (int i = 0; i < maxBones; i++)
            {
                //Positions
                skeletonJointPositions[i].x = jointData[i].position.v0;
                skeletonJointPositions[i].y = jointData[i].position.v1;
                skeletonJointPositions[i].z = jointData[i].position.v0;

                //Orientation
                skeletonJointRotations[i].x = jointData[i].orientation.x;
                skeletonJointRotations[i].y = jointData[i].orientation.y;
                skeletonJointRotations[i].z = jointData[i].orientation.z;
                skeletonJointRotations[i].w = jointData[i].orientation.w;
            }
        }

        //Call this to get a header. 
        public static string PrintHeader()
        {
            return "Position X, Position Y, Position Z, Rotation X, Rotation Y, Rotation Z, Rotation W";
        }

        //Convert the hand to string.
        string HandToString()
        {
            string output = "";

            for(int i = 0; i < maxBones; i++)
            {
                // Using direct float to string conversion three times... 
                // I feel like Vector3.ToString could be better?

                //Position
                output += skeletonJointPositions[i].x.ToString() + ",";
                output += skeletonJointPositions[i].y.ToString() + ",";
                output += skeletonJointPositions[i].z.ToString() + ",";

                //Orientation
                output += skeletonJointRotations[i].x.ToString() + ",";
                output += skeletonJointRotations[i].y.ToString() + ",";
                output += skeletonJointRotations[i].z.ToString() + ",";
                output += skeletonJointRotations[i].w.ToString() + ",";
            }
            return output;
        }
    }
}


