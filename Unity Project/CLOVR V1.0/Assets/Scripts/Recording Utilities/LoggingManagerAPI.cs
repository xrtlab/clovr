/// <summary> 
/// This script aims to log and manage the data using the OpenVR framework. 
/// </summary> 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Valve.VR;
using UnityEngine.Events;
using System.IO;


namespace XRT_OVR_Grabber
{
    /// <summary> Responsible for managing various logging functionalities  </summary>
    public partial class LoggingManagerAPI : MonoBehaviour
	{
        [Space(10)]
        [Header("Project Specific Settings")]
        [Space(10)]

        [SerializeField]
		public string outputFolder = "";
		[SerializeField]
		public string projectName = "";
		[SerializeField]
		public int trialNumber = 1;
		[SerializeField]
		public string fileExtension = ".png";
        [SerializeField]
        public float timeStartedRecording = 0;

        [Space(10)]
        [Header("Recording Settings")]
        [Space(10)]

        [SerializeField]
        public bool enableRecordingTimer = false;
        [SerializeField]
        public int maxRecordingTimer = 60;

        [Space(10)]
        [Header("File Locations")]
        [Space(10)]

        [SerializeField]
        public string xmlQuestionnaireLocation = "";
        [SerializeField]
        public string openVRRuntimeConfig = "";
        [SerializeField]
        public string xmlFileForProjectSettings = "";

        [SerializeField]
        public string OBSProfileLocation = "";

        [SerializeField]
        public string OBSSceneLocation = "";

        [SerializeField]
        public string OBSexecutableLocation = "C:/Program Files/obs-studio/bin/64bit";



        [Space(10)]
        [Header("Rate Settings")]
        [Space(10)]


        /// <summary> In frames  - ~~Every 10 fixed frames ~= 1 second of recoding.~~ - No longer used as a metric for image recording. </summary>
        [SerializeField]
        public float recordRateInterval = 10.0f;  


        ///Buffer for the amount of records to keep in the buffers to store before moving to store the next buffer. </summary>
        [SerializeField]
        int bufferSize = 1000;
        public int numberOfRecordedSessions = 0;


        [Space(10)]
        [Header("Microphone Settings")]
        [Space(10)]

        [SerializeField]
        public int overrideMicIndex = 0;
        [SerializeField]
        public bool overrideTheIndexForMicrophone = false;
        [SerializeField]
        public bool recordMicrophone = false;
        [SerializeField]
        public int microphoneDurationInSecs = 60;
        [SerializeField]
        public int microphoneIndex = 0;

        [Space(10)]
        [Header("Trial Settings")]
        [Space(10)]

        /// Runtime variables. 
        [SerializeField]
        public bool isRecording = false;
        [SerializeField]
        public bool pauseRecording = false;
        [SerializeField]
        public bool manualPauseRecording = false; 
        [SerializeField]
        public int maxRecordingTime = 60; //Seconds

        [Space(10)]
        [Header("Extra Settings")]
        [Space(10)]

        [SerializeField]
        public OBSWebSocketWatcher obsWebsocket; 

        [SerializeField]
        public bool enablePictureCapture = true;

        [SerializeField]
        public bool enableOpenVRDetailExport = false;

        [SerializeField]
        public bool enableMicrophoneWavExport = false;

        [SerializeField]
        public bool exportQuestionnairesOnRecEnd = true;

        [SerializeField]
        public bool enableOBSVideoCapture = true;

        [SerializeField]
        public GameObject pivotSpawnLocation; 


        public enum UpdateMode
        {
            FixedUpdate,
            Update,
            LateUpdate,
            OnPreCull
        }
        public UpdateMode updateRate = UpdateMode.FixedUpdate;

        ///Singleton and actions area. 
        public ProjectUniqueProperties _projectProperties; 
        private IOManager FileIO;
        private XML_Reader readerAndWriterXML; 

        private UnityAction<ControllerInteraction> _storingControllerRecord;
        private UnityAction<bool> _headsetOnHead; 
        private UnityAction _appendControllerRecord; 
        private MicrophoneRecordingAPI _microphoneAPI;

        /// <summary> Class that holds all the settings used in the project.  </summary> 
        ProjectSettings settingsClass;
        OBSManager obsManager = new OBSManager(); 


        /// <summary>
        /// This section will be setting up for handling CONTROLLER-BASED interactions. These include whatever includes button presses or joystick toggles. 
        /// This section will be mostly regarding InputDigitalActionData_t data that is native to OpenVR and includes useful information regarding the controllers. 
        /// </summary>

        List<ControllerInteraction> controllerRecords = new List<ControllerInteraction>();           /// <summary> Special note that these are BUFFERS, meaning they should be progressively emptied and added as the record moves. <summary> 
        List<List<XRT_OVR_Grabber.Pose>> poseRecordsBuffer = new List<List<XRT_OVR_Grabber.Pose>>(); /// <summary> Special note that these are BUFFERS, meaning they should be progressively emptied and added as the record moves. <summary>                                /// <summary> Special note that these are BUFFERS, meaning they should be progressively emptied and added as the record moves. <summary> 
        List<PoseInteractionLog> interactionPoseBuffer = new List<PoseInteractionLog>();             /// <summary> Special note that these are BUFFERS, meaning they should be progressively emptied and added as the record moves. <summary> 

        /// <summary> Initializes the properties of the class with values from the provided 'ProjectSettings' object </summary>
        private void SetupProjectSettingsClass(ProjectSettings _inSettings)
        {
            microphoneIndex         = _inSettings.microphoneIndex;
            microphoneDurationInSecs= _inSettings.microphoneDurationSecs;
            openVRRuntimeConfig     = _inSettings.openvrRuntimeLocation;
            fileExtension           = _inSettings.pictureFileExtension;
            projectName             = _inSettings.projectName;
            outputFolder            = _inSettings.projectOutputLocation;
            recordRateInterval      = _inSettings.recordingFrameInterval;
            bufferSize              = _inSettings.bufferSize;
            trialNumber             = _inSettings.trialCount;
            xmlQuestionnaireLocation= _inSettings.xmlQuestionnaireLocation;
            OBSProfileLocation      = _inSettings.obsProfileLocation;
            OBSSceneLocation        = _inSettings.obsSceneLocation;

            QuestionnaireEvents.ProjectInitialized.Invoke();
        }

        /// <summary> Validates and sets up neccesary file paths for project. </summary>
        private void CheckProjectPaths()
        {
            if (!Directory.Exists(xmlQuestionnaireLocation))
            {
                xmlQuestionnaireLocation = Path.Combine(Application.streamingAssetsPath, "XML_Questionnaires");
            }
            if (!Directory.Exists(outputFolder))
            {
                string currDrive = Path.GetPathRoot(System.Environment.GetFolderPath(System.Environment.SpecialFolder.System));

                outputFolder = currDrive + "\\CLOVR_Output";
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }
            }

            if (!File.Exists(openVRRuntimeConfig))
            {
                openVRRuntimeConfig = Path.Combine(Application.streamingAssetsPath, "Config/actions.json");
            }

            if (!Directory.Exists(OBSProfileLocation))
            {
                OBSProfileLocation = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "obs-studio/basic/profiles/CLEVER_OpenVR_OBS_Capture"); //Path.Combine(Application.streamingAssetsPath, "OBS_Settings/CLEVER_OpenVR_OBS_Capture");
            }

            if (!File.Exists(OBSSceneLocation))
            {
                OBSSceneLocation    = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "obs-studio/basic/scenes/VideoandMicrophoneVRCapture.json"); //Path.Combine(Application.streamingAssetsPath, "OBS_Settings/VideoandMicrophoneVRCapture.json");
            }
        }


        /// <summary> Returns a new ProjectSettings object with the properties of the class </summary> 
        public ProjectSettings PackageProjectSettingsClass()
        {
            return new ProjectSettings(
                xmlFileForProjectSettings,
                outputFolder,
                openVRRuntimeConfig,
                xmlQuestionnaireLocation,
                fileExtension,
                projectName,
                microphoneDurationInSecs,
                recordRateInterval,
                microphoneIndex,
                bufferSize,
                trialNumber,
                OBSProfileLocation,
                OBSSceneLocation,
                OBSexecutableLocation);
        }

        /// <summary> Saves the current project settings to an XML file. If no file is specified, it defaults to saving to a file named "PersistentProjectSave.xml" in the "Resources" directory within the application's persistentDataPath' </summary> 
        public void UI_SaveProjectSettingsClass()
        {

            string outputName = xmlFileForProjectSettings;
            if (outputName == "")
            {
                /*
                if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath, "Resources")))
                {
                    Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "Resources"));
                }*/
                outputName = Path.Combine(Application.streamingAssetsPath, "PersistentProjectSave.xml");
            }
            xmlFileForProjectSettings = outputName;

            settingsClass = PackageProjectSettingsClass();
            //Debug.Log(outputName);
            readerAndWriterXML.Write_XML_ProjectSettings(settingsClass, outputName);
        }

        /// <summary> Loads the project settings from the XML file and sets up the properties of the class using the "SetupProjectSettingsClass method </summary>
        public void UI_LoadProjectSettingsClass()
        {
            settingsClass = readerAndWriterXML.Load_XML_ProjectSettings(xmlFileForProjectSettings);
            SetupProjectSettingsClass(settingsClass);
        }

        /// <summary>Saves the current project settings to a default XML file named "PersistentProjectSave.xml" in the "Resources" directory.  </summary> 
        public void UI_StoreStartupProjectSettingsToDefault()
        {
            string saveLocation = Path.Combine(Application.streamingAssetsPath, "");

            if (!Directory.Exists(saveLocation))
            {
                Directory.CreateDirectory(saveLocation);
            }


            settingsClass = PackageProjectSettingsClass();
            readerAndWriterXML.Write_XML_ProjectSettings(settingsClass, saveLocation + "/PersistentProjectSave.xml");
        }

        /// <summary> 
        ///  should only be used to load the default settings. This loads up the default "last stored" XML file if it exists </summary> 
        private void StartupProjectSettings()
        {
            string saveLocation = Path.Combine(Application.streamingAssetsPath, "PersistentProjectSave.xml");
            if (!File.Exists(saveLocation))
                saveLocation = Path.Combine(Application.streamingAssetsPath, "DefaultConfig.xml");

            CheckProjectPaths();
            settingsClass = readerAndWriterXML.Load_XML_ProjectSettings(saveLocation);
            SetupProjectSettingsClass(settingsClass);
            
        }

        /// Buffer adders. 
        public void _AddControllerRecord(ControllerInteraction _record)
        {
            if (isRecording && !pauseRecording)
                controllerRecords.Add(_record);
        }

        /// <summary> Buffer adder <summary> 
        public void AddPoseRecord(List<XRT_OVR_Grabber.Pose> _record)
        {
            if (isRecording && !pauseRecording)
                poseRecordsBuffer.Add(_record);
        }

        /// <summary> If recording is ongoing and not paused, adds the provided pose interaction record to the interactionPoseBuffer. </summary>
        public void AddInteractionPoseRecord(PoseInteractionLog _record)
        {
            if (isRecording && !pauseRecording)
                interactionPoseBuffer.Add(_record);
        }

        /// <summary> Retains current records <summary> 
		public ArrayList poseHistory = new ArrayList();
		int numOfDevices {
			get { return numOfDevices; }
			set { numOfDevices = value; }
		}

        /// <summary> Clears various buffers and records such controllerRecords, poseRecordsBuffer, pictureBuffer, and interactionPoseBuffer.</summary> 
		public void ClearHistory()
        {
            controllerRecords.Clear();
            poseRecordsBuffer.Clear();
            //pictureBuffer.Clear();
            interactionPoseBuffer.Clear(); 
        }

        /// <summary> Legacy initializer function. No longer used. <summary>
		public void InitLogger()
        {
        }


        public bool firstPassInitialization = false; 
        /// <summary>Initiates the recording process based on certain conditions. For instance, it checks if the outputFolder and projectName are set and if trialNumber is greater than zero. The method also sets up various other settings related to recording.  </summary>
        public void StartRecording()
        {
            timeStartedRecording = Time.frameCount;
            if (isRecording)
            {
                Debug.LogWarning("Recording already in progress!!!");
                return;
            }

            if (outputFolder != "" && projectName != "" && trialNumber > 0)
            {
                FileIO.UpdateLoggerDetails(fileExtension, outputFolder, projectName, trialNumber);
                FileIO.timeStartedLogging = timeStartedRecording;
                FileIO.DirectoryCreator();

                if(enableOBSVideoCapture)
                {
                    obsManager.SetOutputLocation(OBSProfileLocation + "/basic.ini", FileIO.GetVideoOutputLocation());
                    obsManager.StartOBS();
                    StartCoroutine(obsWebsocket.TryConnecting());
                    //obsWebsocket.TryConnectingUnsafe();
                    lockRecordingOBSStatus = false;
                }
                

                Debug.Log("File locations set!");
                isRecording = true;
                numberOfRecordedSessions++;
                _microphoneAPI.SetMicIndex(microphoneIndex); 
                _microphoneAPI.StartRecordingMic(microphoneDurationInSecs);
                trialNumber++;
                UpdateTotalDevicesConnected(0);
                firstPassInitialization = true;
                //XRT_OVR_Grabber.QuestionnaireEvents.GlobalSave.Invoke();
            }
            else
            {
                Debug.LogError("Invalid recording settings.");
                return;
            }
             
            XRT_OVR_Grabber.QuestionnaireEvents.ToggleRecordingMode.Invoke(); 
        }

        /// <summary>  
        /// If the left input is anything to it, set according to it. 
        /// 
        /// If the right is set to true. Set the manual pause override. 
        /// 
        /// With the manual override, the inputter can set the pause value and leave
        /// 
        /// </summary>
        public void _TogglePauseRecording(bool input, bool manualOverride)
        {
            if(manualOverride)
            {
                manualPauseRecording = !manualPauseRecording;
                pauseRecording = input;
                return;
            }

            if (manualPauseRecording)
            {
                return;
            }

            pauseRecording = input;
        }


        public void _TogglePauseRecording(bool input)
        {
            pauseRecording = input;
        }

        ///<summary> Sets the pauseRecording value based on the boolean input provided, which indicates if the recording should be paused or not  </summary>
        public void _TogglePauseRecording()
        {
            pauseRecording = !pauseRecording;
        }

        /// <summary> Stops the recording process by setting the "isRecording" flag to false, stopping the microphone and toggling the recording mode.  </summary>
        public void StopRecording()
        {
            isRecording = false;
            _microphoneAPI.StopRecordingMic(FileIO.storageLocMicrophone);

            //if (exportQuestionnairesOnRecEnd)
            {
                XRT_OVR_Grabber.QuestionnaireEvents.ToggleRecordingMode.Invoke();
                XRT_OVR_Grabber.QuestionnaireEvents.QuestionnaireSaveAll.Invoke();
                XRT_OVR_Grabber.QuestionnaireEvents.StopRecordingSignal.Invoke();
            }


            //Do this only if we're checking to stop OBS
            if (enableOBSVideoCapture)
            {
                obsManager.StopOBS();
                obsWebsocket.webSocketStatus = OBSWebSocketWatcher.WebSocketStateThread.Disconnected;
            }

            timeSpentRecording = 0.0f;
        }

        /// <summary>  
        ///Recording cylces are cycles from which we can control the recording rate. This can be in Hz or other types of refresh rates.
        /// OVRT has more details on how fast it can allegedly record at. https://github.com/ValveSoftware/openvr/issues/1616#issuecomment-993642242
        ///https://github.com/biosmanager/unity-openvr-tracking 
        ///</summary> 
        [SerializeField]
        public float timeSpentRecording = 0.0f;


        bool lockRecordingOBSStatus = false;
        public bool recordingSuccess = false;
        private void RecordCycle()
        {
            recordingSuccess = false;
            //Automatic turn off if the enabled recording max time is reached. 
            if ((timeSpentRecording > (float) maxRecordingTimer) && enableRecordingTimer)
            {
                if (!overlay.isVisible)
                {
                    StopRecording();
                    enableRecordingTimer = false;
                    timeSpentRecording = 0.0f;
                }
            }


            if (isRecording)
            {
                //Only check our OBS flags and stuff if we're actually OBS recording. 
                if (enableOBSVideoCapture)
                {
                    if (!(obsWebsocket.webSocketStatus == OBSWebSocketWatcher.WebSocketStateThread.PriorConnection))// && !lockRecordingOBSStatus)
                    {
                        Debug.Log("OBS status not ready.");
                        return;
                    }
                }

                if (pauseRecording)
                {
                    Debug.Log("Recording Paused");
                    return;
                }

                fileIsDone = false; 
                Debug.Log("Recording...");
                timeSpentRecording += Time.deltaTime;
                if (interactionPoseBuffer.Count >= bufferSize)
                {
                    var tempBuffer = interactionPoseBuffer.GetRange(0, bufferSize);
                    interactionPoseBuffer.RemoveRange(0, bufferSize);
                    FileIO.RunSave(tempBuffer);
                }
                recordingSuccess = true;
            }
            else if (interactionPoseBuffer.Count > 0 && !isRecording)//Store the remainder of the buffers after recording 
            {
                Debug.Log("Post recording");
                var tempBuffer = interactionPoseBuffer.GetRange(0, interactionPoseBuffer.Count);
                interactionPoseBuffer.RemoveRange(0, interactionPoseBuffer.Count);
                FileIO.RunSave(tempBuffer);
            }
            else if(fileIsDone)
            {
                //A skip block once everything has been completed. 
                return;
            }
            else if (interactionPoseBuffer.Count == 0 && !isRecording)
            {
                fileIsDone = true; 
                FileIO.FixDeviceHeader(connectedDevices); 
            }
        }

        private bool fileIsDone = false; 

        private int connectedDevices = 0; 
        public void UpdateTotalDevicesConnected(int num)
        {
            //If for some reason we got a higher number during testing, update this number. 
            if (num > connectedDevices)
                connectedDevices = num;

            //Send zero to reset the counter
            if (num == 0)
                connectedDevices = 0; 
        }


        /// <summary>
        /// Gets the current folder we're exporting data to
        /// 
        /// 
        /// </summary>
        public string GetCurrentProjectOutput()
        {
            return FileIO.GetCurrentProjectOutputLocation();
        }

        /// <summary> Returns the storage location for questionnaires </summary>
        public string GetQuestionnaireOutputLocation()
        {
            return FileIO.storageLocQuestionnaire; 
        }


        /// <summary> 
        /// Obsolete, we're not going to use this anymore. </summary>
        public string GetPictureLocationStorage()
        {
            return FileIO.storageLocPictures;
        }

        /// <summary> Sets the storage location for pictures based on the provided input string  </summary>
        public void SetPictureLocationStorage(string input)
        {
            FileIO.storageLocPictures = input;
        }

        /// <summary>
        ///     Default Unity functions sections. 
        /// </summary>
        private void OnDestroy()
        {
            ClearHistory();
            UI_SaveProjectSettingsClass();
            //obsManager.StopOBS();


            /*
            if (_projectProperties != null)
            {
                _projectProperties.ExportProjectProperties(FileIO.storageLocProjectProperties);
            }*/
        }


        public void _ExportProjectProperties()
        {
            _projectProperties.ExportProjectProperties(FileIO.storageLocProjectProperties);
        }
        /// <summary> Unity lifecycle method that gets called when the object is first initialized. It sets up various components, listeners, and tries to load default settings. </summary>
        private void Awake()
        {
            //obsWebsocket = new OBSWebSocketWatcher();
            readerAndWriterXML = new XML_Reader();
            _storingControllerRecord += _AddControllerRecord;
            _headsetOnHead += _TogglePauseRecording; // I don't think we're using this anyt
            obsManager.SetLoggerManager(this);
            //Try to load the default settings for the project.
            StartupProjectSettings();
            CheckProjectPaths();

            //UI_SaveProjectSettingsClass();
            
            FileIO = new IOManager(fileExtension, outputFolder, projectName, trialNumber);
            _microphoneAPI = new MicrophoneRecordingAPI();
            overlay = GameObject.Find("Unity_Overlay In Game").GetComponent<Unity_Overlay>();
            overlayGameObj = GameObject.Find("Unity_Overlay In Game");
        }


        /// <summary>
        /// We seriously have to force the overlay to stay hidden. 
        /// </summary>
        public void Start()
        {
            ToggleOverlayVisibility(false);
        }

        /// <summary> Unity lifecycle method that gets called when the object becomes inactive. It removes event listeners. </summary>
        private void OnDisable()
        {
            QuestionnaireEvents.AddControllerRecord.RemoveListener(_storingControllerRecord);
            QuestionnaireEvents.HeadsetStatusChange.RemoveListener(_headsetOnHead); 
        }

        /// <summary> Unity lifecycle method that gets called when the object becomes active. It adds event listeners. </summary>
        private void OnEnable()
        {
            QuestionnaireEvents.AddControllerRecord.AddListener(_storingControllerRecord);
            QuestionnaireEvents.HeadsetStatusChange.AddListener(_headsetOnHead); 

        }

        //Below this line are just functions that control the rate of data collection.

        /// <summary> Controls rate of data collection </summary>
        private void FixedUpdate()
        {
            if (updateRate == UpdateMode.FixedUpdate)
            {
                RecordCycle();
            }
        }

        /// <summary>  Controls rate of data collection </summary>
        private void Update()
        {
            if (updateRate == UpdateMode.Update)
            {
                RecordCycle();
            }
        }


        /// <summary>  Controls rate of data collection </summary>
        private void LateUpdate()
        {
            if (updateRate == UpdateMode.LateUpdate)
            {
                RecordCycle();
            }
        }

        /// <summary>  Controls rate of data collection </summary>
        private void OnPreCull()
        {
            if (updateRate == UpdateMode.OnPreCull)
            {
                RecordCycle();
            }
        }
    }
}
