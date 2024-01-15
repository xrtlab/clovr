using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace XRT_OVR_Grabber
{
    public class VRInputMod : MonoBehaviour
    {
        [SerializeField]
        public Camera _camera;
        [SerializeField]
        public SteamVR_Input_Sources targetSources;
        [SerializeField]
        public SteamVR2Input actionManager;

        [SerializeField]
        public GameObject objectHit;
        [SerializeField]
        public bool isSelecting; 

        public void ProcessRaycast()
        {

            var Vector2 = new Vector2(_camera.pixelWidth / 2, _camera.pixelHeight / 2);
            

        }


    }
}