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
    public OpenVRChange onOpenVRChange = delegate(bool connected){};
    public StandbyChange onStandbyChange = delegate(bool inStandbyMode){};
    public DashboardChange onDashboardChange = delegate(bool open){};
    public ChaperoneChange onChaperoneChange = delegate(){};

    private bool PollNextEvent(ref VREvent_t pEvent)
    {
        if(VRSystem == null)
            return false;

		var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.VREvent_t));
		return VRSystem.PollNextEvent(ref pEvent, size);
    }

    public delegate void OpenVRChange(bool connected);
    public delegate void StandbyChange(bool inStandbyMode);
    public delegate void DashboardChange(bool open);

    public delegate void ChaperoneChange();

    private void DigestEvent(VREvent_t pEvent) 
    {
        EVREventType type = (EVREventType) pEvent.eventType;
        switch(type)
        {
            case EVREventType.VREvent_Quit:
                Debug.Log("VR - QUIT - EVENT");
                onOpenVRChange(false);
            break;
            
            case EVREventType.VREvent_DashboardActivated:
                onDashboardChange(true);
            break;
            case EVREventType.VREvent_DashboardDeactivated:
                onDashboardChange(false);
            break;

            case EVREventType.VREvent_EnterStandbyMode:
                onStandbyChange(true);
            break;
            case EVREventType.VREvent_LeaveStandbyMode:
                onStandbyChange(false);
            break;

            case EVREventType.VREvent_ChaperoneSettingsHaveChanged:
                onChaperoneChange();
            break;
        }

        onVREvent.Invoke(pEvent);
    }
}