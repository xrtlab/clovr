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
using UnityEngine.Events;

using Valve.VR;

public class Unity_SteamVR_Handler : MonoBehaviour 
{
	public float steamVRPollTime = 0.05f;

	public bool connectedToSteam = false;

	[Space(10)]

	public GameObject hmdObject;
	public GameObject rightTrackerObj;
	public GameObject leftTrackerObj;

	[Space(10)]

	public bool autoUpdate = true;

	[Space(10)]

	public bool debugLog = false;

	[Space(10)]

	public UnityEvent onSteamVRConnect = new UnityEvent();
	public UnityEvent onSteamVRDisconnect = new UnityEvent();

	[Space(10)]

	public UnityEvent onDashboardOpen = new UnityEvent();
	public UnityEvent onDashboardClose = new UnityEvent();


	[HideInInspector] public OVR_Handler ovrHandler = OVR_Handler.instance;

	[HideInInspector] public OVR_Overlay_Handler overlayHandler { get { return ovrHandler.overlayHandler; } }
	[HideInInspector] public OVR_Pose_Handler poseHandler { get { return ovrHandler.poseHandler; } }

	private float lastSteamVRPollTime = 0f;

	public static UnityEvent SteamVRNotInitializedEvent = new UnityEvent();

	void Start()
	{
		// Will always do a check on start first, then use timer for polling
		lastSteamVRPollTime = steamVRPollTime + 1f;
		ovrHandler.onOpenVRChange += OnOpenVRChange;

		Application.targetFrameRate = 91;

		ovrHandler.onVREvent += VREventHandler;
	}

	void OnOpenVRChange(bool connected) 
	{
		connectedToSteam = connected;

		if(!connected)
		{
			onSteamVRDisconnect.Invoke();
			ovrHandler.ShutDownOpenVR();
		}
			
	}

	void OnDashboardChange(bool open)
	{
		if(open)
			onDashboardOpen.Invoke();
		else
			onDashboardClose.Invoke();
	}

	void Update() 
	{
		if(autoUpdate)
			UpdateHandler();
	}

	public void UpdateHandler()
	{
		if(!SteamVRStartup())
			return;

		ovrHandler.UpdateAll();

		if(hmdObject)
			poseHandler.SetTransformToTrackedDevice(hmdObject.transform, poseHandler.hmdIndex);

		if(poseHandler.rightActive && rightTrackerObj)
		{
			rightTrackerObj.SetActive(true);
			poseHandler.SetTransformToTrackedDevice(rightTrackerObj.transform, poseHandler.rightIndex);
		}
		else if(rightTrackerObj)
			rightTrackerObj.SetActive(false);
		
		if(poseHandler.leftActive && leftTrackerObj)
		{
			leftTrackerObj.SetActive(true);
			poseHandler.SetTransformToTrackedDevice(leftTrackerObj.transform, poseHandler.leftIndex);
		}
		else if(leftTrackerObj)
			leftTrackerObj.SetActive(false);
	}

	public void VREventHandler(VREvent_t e)
	{
		if(debugLog)
			Debug.Log("VR Event: " + e);
	}

	bool SteamVRStartup()
	{
		lastSteamVRPollTime += Time.deltaTime;

		if(ovrHandler.OpenVRConnected)
			return true;
		else if(lastSteamVRPollTime >= steamVRPollTime)
		{
			lastSteamVRPollTime = 0f;

			Debug.Log("Checking to see if SteamVR Is Running...");
			if(System.Diagnostics.Process.GetProcessesByName("vrserver").Length <= 0)
			{
				Debug.Log("VRServer not Running!");
				SteamVRNotInitializedEvent.Invoke();
				return false;
			}

			Debug.Log("Starting Up SteamVR Connection...");

			if( !ovrHandler.StartupOpenVR() )
			{
				Debug.Log("Connection Failed :( !");
				SteamVRNotInitializedEvent.Invoke();
				return false;
			}
			else
			{
				Debug.Log("Connected to SteamVR!");
				
				onSteamVRConnect.Invoke();
				ovrHandler.onDashboardChange += OnDashboardChange;

				return true;
			}		
		}
		else
			return false;
	}
	void OnApplicationQuit()
	{
		if(ovrHandler.OpenVRConnected)
			ovrHandler.ShutDownOpenVR();
	}
}
