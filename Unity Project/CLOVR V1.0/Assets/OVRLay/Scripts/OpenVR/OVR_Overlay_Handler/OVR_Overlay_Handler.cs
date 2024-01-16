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
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class OVR_Overlay_Handler 
{
    private static OVR_Overlay_Handler _instance;
    public static OVR_Overlay_Handler instance 
    {
        get 
        {
            if(_instance == null)
                _instance = new OVR_Overlay_Handler();
            
            return _instance;
        }
    }

    private HashSet<OVR_Overlay> overlays = new HashSet<OVR_Overlay>();

    public void VRShutdown()
    {
        foreach(OVR_Overlay overlay in overlays)
            overlay.VRShutdown();

        DestroyAllOverlays();
    }

    public void UpdateOverlays()
    {
        foreach(OVR_Overlay overlay in overlays)
            overlay.UpdateOverlay();
    }

    public void DestroyAllOverlays()
    {
        foreach(OVR_Overlay overlay in overlays)
            overlay.DestroyOverlay();
    }

    public bool RegisterOverlay(OVR_Overlay overlay)
    {
        return overlays.Add(overlay);
    }

    public bool DeregisterOverlay(OVR_Overlay overlay)
    {
        return overlays.Remove(overlay);
    }
}