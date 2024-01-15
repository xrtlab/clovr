using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Valve.VR;
using UnityEngine.Events;
using System.IO;

namespace XRT_OVR_Grabber
{
    /// <summary> This class manages the unique properties about the project such as area, size, runtime version, number of applications, etc. There is a method to export these properties to a CSV format <summary>
    public class ProjectUniqueProperties
    {
        public HmdQuad_t roomAreaRectangle = new HmdQuad_t();
        public float roomSizeX = 0.0f, roomSizeY = 0.0f; /// <summary> represent the size of the room / playable area in the VR space <summary> 
        public int numberOfApplications = 0; /// <summary> Tracks number of VR applications loaded within project. <summary>
        string runtimeVer = ""; /// <summary> stores version of VR runtime <summary>
        public int numberOfRecordedSessions = 0; /// <summary> number of sessions that have been logged. <summary>


        /// <summary> When an instance of this class is created, this function is called to initialize the class variables <summary>
        public ProjectUniqueProperties(HmdQuad_t roomRec, float roomX, float roomY, int numApps, string _runtime)
        {
            runtimeVer = _runtime;
            roomAreaRectangle = roomRec;
            roomSizeX = roomX;
            roomSizeY = roomY;
            numberOfApplications = numApps;
        }


        /// <summary> Constructs a string with room size, number of applications, runtime version, and number of recorded sessions. 
        /// Attempts to write this constructed string to the specific file location using a 'StreamWriter'. 
        /// If this process encounters any issues (like file permissions, unavailable paths, etc.), it catches the exception and logs an error message. <summary>
        public void ExportProjectProperties(string fileOutputLocation)
        {
            /// <summary> Converting the room area rectangle's corner 1 <summary>
            string outString = "";
            string header = "Room Corner 0, Room Corner 1, Room Corner 2, Room Corner 3,Room Size X, Room Size Y, Number of Applications active, OpenVR Runtime Version, Amount of Recorded Sessions\n";
            string corner0 = roomAreaRectangle.vCorners0.v0.ToString() + ";" + roomAreaRectangle.vCorners0.v1.ToString() + ";" + roomAreaRectangle.vCorners0.v2.ToString();             /// <summary> Converting the room area rectangle's corner 1 <summary>
            string corner1 = roomAreaRectangle.vCorners1.v0.ToString() + ";" + roomAreaRectangle.vCorners1.v1.ToString() + ";" + roomAreaRectangle.vCorners1.v2.ToString();            /// <summary> Converting the room area rectangle's corner 2 <summary>
            string corner2 = roomAreaRectangle.vCorners2.v0.ToString() + ";" + roomAreaRectangle.vCorners2.v1.ToString() + ";" + roomAreaRectangle.vCorners2.v2.ToString();            /// <summary> Converting the room area rectangle's corner 3 <summary>
            string corner3 = roomAreaRectangle.vCorners3.v0.ToString() + ";" + roomAreaRectangle.vCorners3.v1.ToString() + ";" + roomAreaRectangle.vCorners3.v2.ToString();            /// <summary> Converting the room area rectangle's corner 4 <summary>


            // String otuput. 
            outString += header;
            outString += corner0 + "," + corner1 + "," + corner2 + "," + corner3 + "," + roomSizeX + "," + roomSizeY + "," +
                numberOfApplications + "," + runtimeVer + "," + numberOfRecordedSessions + "\n";

            //This file is literally one line, its creation shouldn't impact the program too harshly. 
            try
            {
                StreamWriter writer = new StreamWriter(fileOutputLocation, false);
                writer.Write(outString);
                writer.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }



    /// <summary> Designed to store various configuration and settings related to the project <summary>
    public class ProjectSettings
    {
        public string xmlConfigFilesLocation = "";
        public string projectOutputLocation = "";
        public string openvrRuntimeLocation = "";
        public string xmlQuestionnaireLocation = "";
        public string pictureFileExtension = ".jpg";
        public string obsProfileLocation = "";
        public string obsSceneLocation = "";
        public string obsExeLocation = ""; 
        public string projectName = "";
        public float recordingFrameInterval = 10;
        public int microphoneDurationSecs = 60;
        public int microphoneIndex = 1;
        public int bufferSize = 1000;
        public int trialCount = 0;

        /// <summary> default constructor <summary>
        public ProjectSettings(){}

        /// <summary> Parameterized constructor that initializes an instance of 'Project settings' class and sets it's variables based on the provided parameters' <summary>
        public ProjectSettings(
            string _config = "",
            string _output = "",
            string _openvr = "",
            string _questionnaires = "",
            string _extensionPicture = "",
            string _projectName = "",
            int _microphoneDurationSecs = 60,
            float _recordInterval = 10,
            int _micIndex = 0,
            int _buffSize = 1000,
            int _trialCount = 0,
            string _obsProfile = "",
            string _obsScene = "",
            string _obsExe = "")
        {
            xmlConfigFilesLocation = _config;
            projectOutputLocation = _output;
            openvrRuntimeLocation = _openvr;
            xmlQuestionnaireLocation = _questionnaires;
            pictureFileExtension = _extensionPicture;
            projectName = _projectName;
            recordingFrameInterval = _recordInterval;
            microphoneDurationSecs = _microphoneDurationSecs;
            microphoneIndex = _micIndex;
            bufferSize = _buffSize;
            trialCount = _trialCount;
            obsProfileLocation = _obsProfile;
            obsSceneLocation = _obsScene;
            obsExeLocation = _obsExe; 

    }

    /// <summary> Updates the settings of an already created ProjectSettings object. <summary>
    public void UpdateProjectSettings(
            string _config = "",
            string _output = "",
            string _openvr = "",
            string _questionnaires = "",
            string _extensionPicture = "",
            string _projectName = "",
            int _microphoneDurationSecs = 60,
            float _recordInterval = 10,
            int _micIndex = 0,
            int _buffSize = 1000,
            int _trialCount = 0,
            string _obsProfile = "",
            string _obsScene = "",
            string _obsExe = "")
        {
            xmlConfigFilesLocation = _config;
            projectOutputLocation = _output;
            openvrRuntimeLocation = _openvr;
            xmlQuestionnaireLocation = _questionnaires;
            pictureFileExtension = _extensionPicture;
            projectName = _projectName;
            recordingFrameInterval = _recordInterval;
            microphoneDurationSecs = _microphoneDurationSecs;
            microphoneIndex = _micIndex;
            bufferSize = _buffSize;
            trialCount = _trialCount;
            obsProfileLocation = _obsProfile;
            obsSceneLocation = _obsScene;
            obsExeLocation = _obsExe;
        }
    }
}
