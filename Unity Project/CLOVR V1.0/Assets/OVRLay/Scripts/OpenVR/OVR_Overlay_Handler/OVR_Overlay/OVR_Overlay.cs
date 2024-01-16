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


public partial class OVR_Overlay 
{
    public bool UpdateCurrentOverlay()
    {
        overlayHighQuality = _overlayHighQuality;
        overlayColor = _overlayColor;
        overlayAlpha = _overlayAlpha;
        overlayWidthInMeters = _overlayWidthInMeters;
        overlayTextureBounds = _overlayTextureBounds;

        overlayTransformType = _overlayTransformType;
        overlayTransform = _overlayTransform;
        overlayTransformAbsoluteTrackingOrigin = _overlayTransformAbsoluteTrackingOrigin;
        overlayTransformTrackedDeviceRelativeIndex = _overlayTransformTrackedDeviceRelativeIndex;

        overlayInputMethod = _overlayInputMethod;
        overlayVisible = _overlayVisible;

        overlayTexture = _overlayTexture;

        return !ErrorCheck(error);
    }

    public bool HideOverlay() 
    {
        overlayVisible = false;
        return !ErrorCheck(error);
    }
    public bool ShowOverlay() 
    {
        overlayVisible = true;
        return !ErrorCheck(error);
    }

    public bool ClearOverlayTexture()
    {
        if(OverlayExists && validHandle)
            error = Overlay.ClearOverlayTexture(_overlayHandle);
        
        return !ErrorCheck(error);
    }

    public bool ClearOverlayThumbnailTexture()
    {
        if(OverlayExists && validHandle && overlayIsDashboard)
            error = Overlay.ClearOverlayTexture(_overlayThumbnailHandle);

        return !ErrorCheck(error);
    }

    private bool _isMinimal = false;


    public bool OpenKeyboard(string dscrp = "", string fillTxt = "", GameObject KeyboardSpawnLocation = null)
    {
        bool status = false;
        if (OverlayExists)
        {
            error = Overlay.ShowKeyboardForOverlay(_overlayHandle, 1, 1, 0, dscrp, 2000, fillTxt, 0);
            status = !ErrorCheck(error);
            if (status)
            {
                //HmdRect2_t pos = new HmdRect2_t();
                //pos.vBottomRight.v0 = 0.0f;
                //pos.vTopLeft.v0 = 0.0f;
                HmdMatrix34_t transform;
                if (KeyboardSpawnLocation != null)
                {
                    transform = Input_ToHmdMatrix34(KeyboardSpawnLocation.transform.position, KeyboardSpawnLocation.transform.rotation);
                }
                else
                {
                    transform = Input_ToHmdMatrix34(Vector3.zero, Quaternion.identity);
                }

                //Overlay.SetKeyboardPositionForOverlay(_overlayHandle, pos);
                Overlay.SetKeyboardTransformAbsolute(ETrackingUniverseOrigin.TrackingUniverseStanding, ref transform);
            }
            else
            {
                return status;
            }
        }
        //    error = Overlay.ShowKeyboard(0, 0, dscrp, 256, fillTxt, minimal, 0);
        return status;
    }

    public HmdMatrix34_t Input_ToHmdMatrix34(Vector3 _pos, Quaternion _rot)
    {
        var m = Matrix4x4.TRS(_pos, _rot, Vector3.one);
        var pose = new HmdMatrix34_t();

        pose.m0 = m[0, 0];
        pose.m1 = m[0, 1];
        pose.m2 = -m[0, 2];
        pose.m3 = m[0, 3];

        pose.m4 = m[1, 0];
        pose.m5 = m[1, 1];
        pose.m6 = -m[1, 2];
        pose.m7 = m[1, 3];

        pose.m8 = -m[2, 0];
        pose.m9 = -m[2, 1];
        pose.m10 = m[2, 2];
        pose.m11 = -m[2, 3];

        return pose;
    }

    public void CloseKeyboard()
    {
        if(OverlayExists)
            Overlay.HideKeyboard();
    }

    protected EVROverlayError error;
    protected bool ErrorCheck(EVROverlayError error)
    {
        bool err = (error != EVROverlayError.None);

        if(err)
            Debug.Log("Error: " + Overlay.GetOverlayErrorNameFromEnum(error));

        return err;
    }

    public void VRShutdown() 
    {
       
    }

    public OVR_Overlay()
    {
        OVR_Overlay_Handler.instance.RegisterOverlay(this);
    }

    ~OVR_Overlay()
    {
        OVR_Overlay_Handler.instance.DeregisterOverlay(this);
        DestroyOverlay();
    }

    public virtual bool CreateOverlay()
    {
        if(!OverlayExists)
            return ( _created = false );

        if(_overlayIsDashboard)
            error = Overlay.CreateDashboardOverlay(_overlayKey, _overlayName, ref _overlayHandle, ref _overlayThumbnailHandle);
        else
            error = Overlay.CreateOverlay(_overlayKey, _overlayName, ref _overlayHandle);

        bool allGood = !ErrorCheck(error);

        return ( _created = allGood );
    }

    public void UpdateOverlay() 
    {
        while(PollNextOverlayEvent(ref pEvent))
            DigestEvent(pEvent);
    }

    public bool DestroyOverlay()
    {
        if(!_created || !OverlayExists || !validHandle)
            return true;   

        error = Overlay.DestroyOverlay(_overlayHandle);
        _created = false;

        return _created;
    }
}
