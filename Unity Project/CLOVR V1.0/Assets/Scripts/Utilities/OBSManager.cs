using UnityEngine;
using UnityProcessPassthrough;
using System.IO;
using System;
using XRT_OVR_Grabber;

/// <summary> This class manages various functions related to OBS within the Unity project. It contains a script that directly calls the UPP library for an OBS Windows process and destroys it when no longer needed. </summary> 
public class OBSManager : MonoBehaviour
{
    string obsExecutable = "";
    string arguments = "";
    string directory = "";
    [SerializeField]
    private LoggingManagerAPI loggingManager;
    private OBSWebSocketWatcher websocket;

    /// <summary> Sets the logging manager. </summary> 
    public void SetLoggerManager(LoggingManagerAPI api)
    {
        loggingManager = api;
    }
    /// <summary> Configures OBS by setting up executable pats, profiles, scenes, and arguments.  </summary>
    public void SetupOBS()
    {
        obsExecutable = loggingManager.OBSexecutableLocation + "/obs64.exe";
        if (!File.Exists(obsExecutable))
        {
            UnityEngine.Debug.LogError("WARNING: OBS DOES NOT EXIST, SKIPPING THIS FEATURE.");
            return;
        }

        string profileInput = "--profile " + "CLEVER_OpenVR_OBS_Capture"; // loggingManager.OBSProfileLocation; //Path.Combine(Application.streamingAssetsPath, " / OBS_Settings/CLEVER_OpenVR_OBS_Capture");
        string sceneInput = "--scene " + "VideoandMicrophoneVRCapture"; //loggingManager.OBSSceneLocation;// Path.Combine(Application.streamingAssetsPath, " / OBS_Settings/VideoandMicrophoneVRCapture.json");
        arguments = "--startrecording --minimize-to-tray -m " + sceneInput + " " + profileInput;//"--startrecording --scene C:\\OBS_Settings\\VideoandMicrophoneVRCapture.json --profile C:\\OBS_Settings\\Tomatoes";
        directory = loggingManager.OBSexecutableLocation;
        UPP.CreateProcess(obsExecutable, arguments, directory);
        //UPP.StoreValue(23456789);
        //UnityEngine.Debug.Log(UPP.RecallValue());
    }

    /// <summary> Copies OBS profile and scene settings from a default location to the user's OBS settings directory </summary>
    private void CopyOBSOverrides()
    {
        string targetOBSProfileDir = Path.Combine(Application.streamingAssetsPath, "DefaultOBSSettings/CLEVER_OpenVR_OBS_Capture");
        string targetOBSSceneFile = Path.Combine(Application.streamingAssetsPath, "DefaultOBSSettings/VideoandMicrophoneVRCapture.json");
        string profileLocation = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "obs-studio/basic/profiles/CLEVER_OpenVR_OBS_Capture");
        string sceneLocation = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "obs-studio/basic/scenes/");

        //Copying over the correct global settings
        string globalSettingsLocation = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "obs-studio/");
        string targetGlobalSettings = Path.Combine(Application.streamingAssetsPath, "DefaultOBSSettings/global.ini");

        OverrideGlobalSettings(targetGlobalSettings, globalSettingsLocation, false);


        if (!Directory.Exists(profileLocation))
        { 
            CopyDirectory(targetOBSProfileDir, profileLocation);
            UnityEngine.Debug.Log("Creating the profiles directory");
        }

        if (!File.Exists(loggingManager.OBSSceneLocation)) 
        {
            UnityEngine.Debug.Log("Creating the scenes file");
            if (!Directory.Exists(sceneLocation))
            {
                Directory.CreateDirectory(sceneLocation);
            }
            File.Copy(targetOBSSceneFile, loggingManager.OBSSceneLocation);
        }
    }

    /// <summary> Provides a way of overriding the OBS settings. </summary> 
    public void OverrideGlobalSettings(string targetGlobalSettings, string globalSettingsLocation, bool overwrite)
    {
        //Overwrites the global settings every time?
        if (!File.Exists(Path.Combine(globalSettingsLocation, "global.ini")))
        {
            if (Directory.Exists(globalSettingsLocation))
            {
                Directory.CreateDirectory(globalSettingsLocation);
            }

            File.Copy(targetGlobalSettings, globalSettingsLocation);
        }

        //If true, OBS's global settings will be reset to this default profile by CLEVER 
        if(overwrite)
        {
            File.Copy(targetGlobalSettings, globalSettingsLocation, true);
        }
    }

    /// <summary> A method to copy files from one directory to another </summary>
    private void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        string[] files = Directory.GetFiles(source);
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destination, fileName);
            File.Copy(file, destFile, true);
        }
    }


    /// <summary> Modifies global OBS settings to activate a webservice for remote monitoring / control. Assumes you have OBS somewhere already. </summary>
    public void SetWebserviceActive()
    {
        string globalSettingsLocation = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "obs-studio/global.ini");

        try
        {
            string[] lines = File.ReadAllLines(globalSettingsLocation);

            // Replace the line with the new value
            //var correctedOutput = newOutputLocation.Replace("\\", "\\\\");
            //correctedOutput = correctedOutput.Replace("/", "\\\\");
            for (int i = 0; i < lines.Length; i++)
            {

                if (lines[i].Contains("Profile="))
                {
                    lines[i] = "Profile=CLEVER_OpenVR_OBS_Capture";
                }
                else if (lines[i].Contains("ProfileDir="))
                {
                    lines[i] = "ProfileDir=CLEVER_OpenVR_OBS_Capture";
                }
                else if (lines[i].Contains("SceneCollection="))
                {
                    lines[i] = "SceneCollection=Video-and-Microphone-VR-Capture";
                }
                else if (lines[i].Contains("SceneCollectionFile="))
                {
                    lines[i] = "SceneCollectionFile=VideoandMicrophoneVRCapture";
                }
                else if (lines[i].Contains("ServerEnabled="))
                {
                    lines[i] = "ServerEnabled=true";
                }
                else if (lines[i].Contains("ServerPort="))
                {
                    lines[i] = "ServerPort=4455";
                }
                else if (lines[i].Contains("AlertsEnabled="))
                {
                    lines[i] = "AlertsEnabled=true";
                }
                else if (lines[i].Contains("AuthRequired="))
                {
                    lines[i] = "AuthRequired=true"; //Doesn't matter, so long as we can connect and immediately disconnect. 
                }
            }

            // Write the modified lines back to the file
            File.WriteAllLines(globalSettingsLocation, lines);
            Console.WriteLine("File updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    /// <summary> Sets new locations for OBS output files.  </summary>
    public void SetOutputLocation(string profileLocation, string newOutputLocation)
    {
        if (!File.Exists(loggingManager.OBSexecutableLocation + "/obs64.exe"))
        {
            Debug.LogError("WARNING: OBS DOES NOT EXIST, SKIPPING THIS FEATURE.");
            return;
        }    

        CopyOBSOverrides();
        SetWebserviceActive();
        try
        {
            string[] lines = File.ReadAllLines(profileLocation);

            // Replace the line with the new value
            var correctedOutput = newOutputLocation.Replace("\\", "\\\\");
            correctedOutput = correctedOutput.Replace("/", "\\\\");
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("FilePath="))
                {
                    lines[i] = "FilePath="+ correctedOutput;  
                }
                else if (lines[i].Contains("RecFilePath="))
                {
                    lines[i] = "RecFilePath=" + correctedOutput;
                }
                else if (lines[i].Contains("FFFilePath="))
                {
                    lines[i] = "FFFilePath=" + correctedOutput;
                }
            }

            // Write the modified lines back to the file
            File.WriteAllLines(profileLocation, lines);
            Console.WriteLine("File updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }


    /// <summary>
    /// On closing Unity, CLOVR, or either one crashing; the UPP library will attempt to close the process for OBS. 
    /// </summary>
    private void OnApplicationQuit()
    { 
        StopOBS();
    }

    /// <summary>
    /// On calling this, UPP is called to start a OBS process. 
    /// </summary>
    public void StartOBS()
    {
        SetupOBS();
        websocket = new OBSWebSocketWatcher();
    }

    /// <summary>
    /// On closing Unity, CLOVR, or either one crashing; the UPP library will attempt to close the process for OBS. 
    /// </summary>
    public void StopOBS()
    {
        UPP.StopProcess();
    }

    /// <summary>
    /// On closing Unity, CLOVR, or either one crashing; the UPP library will attempt to close the process for OBS. 
    /// </summary>
    public void KillOBS()
    {
        UPP.StopProcess();
    }

}
