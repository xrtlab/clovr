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

public partial class OVR_Handler 
{
    static private OVR_Handler _instance;
    static public OVR_Handler instance 
    {
        get {
            if(_instance == null)
                _instance = new OVR_Handler();

            return _instance;
        }
    }
    public bool OpenVRConnected { get { return (_VRSystem != null); } }

    private CVRSystem _VRSystem;
    public CVRSystem VRSystem { get { return _VRSystem; } }

    private CVRCompositor _Compositor;
    public CVRCompositor Compositor { get { return _Compositor; } }

    private CVRChaperone _Chaperone;
    public CVRChaperone Chaperone { get { return _Chaperone; } }

    private CVRChaperoneSetup _ChaperoneSetup;
    public CVRChaperoneSetup ChaperoneSetup { get { return _ChaperoneSetup; } }

    private CVROverlay _Overlay;
    public CVROverlay Overlay { get { return _Overlay; } }

    private CVRSettings _Settings;
    public CVRSettings Settings { get { return _Settings; } }

    private CVRApplications _Applications;
    public CVRApplications Applications { get { return _Applications; } }

    private CVRRenderModels _RenderModels;
    public CVRRenderModels RenderModels {get { return _RenderModels; }}

    public CVRTrackedCamera _TrackedCam;
    public CVRTrackedCamera TrackedCam { get { return _TrackedCam; } }



    private EVRApplicationType _applicationType = EVRApplicationType.VRApplication_Background;
    public EVRApplicationType applicationType { get { return _applicationType; } }

    

    private OVR_Pose_Handler _poseHandler;
    public OVR_Pose_Handler poseHandler 
    { 
        get 
        { 
            if(_poseHandler == null)
                _poseHandler = OVR_Pose_Handler.instance;

            return _poseHandler; 
        }
    }

    private OVR_Overlay_Handler _overlayHandler;
    public OVR_Overlay_Handler overlayHandler 
    { 
        get 
        { 
            if(_overlayHandler == null)
                _overlayHandler = OVR_Overlay_Handler.instance;

            return _overlayHandler; 
        } 
    }
}