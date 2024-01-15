/// <summary>
/// This script is used to control the primary GUI of CLOVR. A separate script is used for the image fixer.  
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; 
namespace XRT_OVR_Grabber
{
    public partial class InstructorsInterface : MonoBehaviour
    {

        [Space(10)]
        [Header("Recording Status")]
        [Space(10)]

        [SerializeField]
        Image recordingDot;

        [SerializeField]
        GameObject pauseMessage; 
        /// This controls the timer for recording time. 

        [SerializeField]
        TMPro.TMP_InputField recordingTrialTime;

        [SerializeField]
        TMPro.TMP_InputField currentRecordingTime;

        ////////////////////////////


        [Space(10)]
        [Header("Toggles")]
        [Space(10)]

        [SerializeField]
        Toggle isRecordingToggle;

        [SerializeField]
        Toggle recordTheMicrophoneToggle;

        [SerializeField]
        Toggle imageRecordingToggle;

        [SerializeField]
        Toggle exportOpenVRDetailsToggle;

        [SerializeField]
        Toggle exportQuestionnaireAfterRecordingToggle;

        [SerializeField]
        Toggle saveAsMicrophoneFileTypeToggle;

        [SerializeField]
        Toggle useJPGForPictureRecording;

        [SerializeField]
        Toggle enableRecordingTimer;

        [SerializeField]
        Toggle toggleAutoPictureFixer;

        [SerializeField]
        Toggle toggleOBSRecording;

        [Space(10)]
        [Header("Questionnaire Settings")]
        [Space(10)]

        [SerializeField]
        TMPro.TMP_Dropdown dropdownUI;

        [SerializeField]
        TMPro.TMP_InputField questionnaireInputField;

        [SerializeField]
        TMPro.TMP_InputField xmlQuestionnaireLocField;

        [SerializeField]
        TMPro.TMP_InputField xmlQuestionnaireLocFieldFrontEnd;


        [Space(10)]
        [Header("Microphone Settings")]
        [Space(10)]

        [SerializeField]
        TMPro.TMP_InputField microphoneInSeconds; 

        [SerializeField]
        TMPro.TMP_InputField microphoneIndexField;

        [SerializeField]
        TMPro.TMP_Dropdown microphoneSelectionDropdown;

        [Space(10)]
        [Header("Project Settings")]
        [Space(10)]

        [SerializeField]
        TMPro.TMP_InputField openVRXDeviceConfigsField;

        [SerializeField]
        TMPro.TMP_InputField xmlProjectSettings;

        [SerializeField]
        TMPro.TMP_InputField outputFileField;

        [SerializeField]
        TMPro.TMP_InputField questionnaireExportLocation; 

        [SerializeField]
        TMPro.TMP_InputField trialNumberField; 

        [SerializeField]
        TMPro.TMP_InputField projectNameField;

        [Space(10)]
        [Header("HMD Preview Sections")]
        [Space(10)]

        [SerializeField]
        Material leftFrameImage;

        [SerializeField]
        Material rightFrameImage;


        [Space(10)]
        [Header("Other Settings")]
        [Space(10)]

        [SerializeField]
        GameObject optionsPanel;

        XRT_OVR_Grabber.LoggingManagerAPI loggerManager;
        XRT_OVR_Grabber.SurveyInterface surveyInterface; 
        OVRT.OVRT_Manager ovrtManagerInstance;
        private UnityAction _recordingStopped; 



        /// <summary> Initializes refrences to other components. Called when script instance is being loaded <summary>
        private void Awake()
        {
            loggerManager = GameObject.Find("LoggingManger").GetComponent<LoggingManagerAPI>();
            ovrtManagerInstance = GameObject.Find("OVRT_Manager").GetComponent<OVRT.OVRT_Manager>();
            surveyInterface = GameObject.Find("Dashboard_Interface").GetComponent<SurveyInterface>();
            SetupDropdownUI();
            _recordingStopped += StopRecordingToggle;
            //imageFixer = new ImageFlipper();
        }

        /// <summary> Configures the dropdown element based on avaialable questionaires <summary>
        void SetupDropdownUI()
        {
            List<string> options = surveyInterface.questionnairesAsString;
            dropdownUI.ClearOptions();
            dropdownUI.AddOptions(options);
            questionnaireInputField.text = dropdownUI.itemText.text;
        }

        /// <summary> Updates the loggerManager's settings related to microphone recording and image format based on UI toggles  <summary>
        public void UI___ToggleAction()
        {
            if (recordTheMicrophoneToggle.isOn)
            {
                loggerManager.recordMicrophone = true;
            }
            else
            {
                loggerManager.recordMicrophone = false;
            }

            if (useJPGForPictureRecording.isOn)
            {
                loggerManager.fileExtension = ".jpg";
            }
            else
            {
                loggerManager.fileExtension = ".png";
            }
        }

        public void UI__TogglePictureCapture()
        {
            loggerManager.enablePictureCapture = imageRecordingToggle.isOn;
        }

        //public void UI__ToggleOpenVRDetails()// I don't think we even need this, this should be a 
        //{
        //    ///loggerManager.enableMicrophoneWavExport = exportOpenVRDetailsToggle.isOn; ????


        //    //loggerManager
        //}

        public void UI__ToggleQuestionnaireExport()
        {
            if(exportQuestionnaireAfterRecordingToggle.isOn)
                loggerManager.exportQuestionnairesOnRecEnd = true;
            else
                loggerManager.exportQuestionnairesOnRecEnd = false;
        }

        public void UI__InvertControllersFix()
        {
            QuestionnaireEvents.InvertControllerBiding.Invoke();
        }


        /////////////////////////////////////////////////////////////////////////////////QUESTIONNAIRE CALLS////////////////


        /// <summary> Sets up and starts the selected questionaire from the dropdown menu on Instructor's Interface <summary>
        public void UI___SetupAndStartQuestionnaire()
        {
            //Check if we are recording and not presetting a questionnaire outside of recording time. 
            //if (!loggerManager.isRecording)
            //   return;


            int selectedName = dropdownUI.value;
            //var names = surveyInterface.questionnairesAsString[selectedName];
            surveyInterface.UI_SetupNextQuestionnaire(selectedName);
            questionnaireInputField.text = surveyInterface._GetCurrentlyAssignedQuestionnaireName();
            loggerManager.ToggleOverlayVisibility(true);
            QuestionnaireEvents.ToggleKeyboard.Invoke(true); 
        }

        /// <summary> Clears the current questionaire <summary>
        public void UI___CancelLastQuestionnaire()
        {
            surveyInterface._ClearCurrentQuestionnaire();
        }

        /// <summary> Cancels all questionaires <summary>
        public void UI___CancelEntireRecordingTrial()
        {
            surveyInterface._CancelQuestionnaires();
        }

        public void UI__ClearAllQuestionnaires()
        {
            surveyInterface._ClearAllQuestionnaireResponses();
        }

        public void UI__ExportQuestionnaireResponses()
        {
            var location = questionnaireExportLocation.text;
            if (System.IO.Directory.Exists(location))
            {
                surveyInterface.DirectExportToFolder(location); 
            }
            else
            {
                Debug.LogError("Invalid Export location, nothing exported.");
            }
        }

        public void UI__ReloadQuestionnairesDirectory()
        {
            string fileLocation = xmlQuestionnaireLocField.text;
            if (System.IO.Directory.Exists(fileLocation))
            {
                surveyInterface.ManualInitializationWithLocation(fileLocation);
            }
            else
            {
                Debug.LogError("Bad directory given, nothing changed or reloaded.");
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////Microphone options////////////////////
        ///
        ///When triggered, the options provided on the dropdown will be reset with currently active microphones (hopefully) 
        public void UI__ReloadandResetMicrophoneDropdown()
        {
            microphoneSelectionDropdown.ClearOptions();
            var listIn = new List<string>();
            
            foreach(string s in Microphone.devices)
                listIn.Add(s);
            microphoneSelectionDropdown.AddOptions(listIn);
        }

        public void UI__SetMicrophoneIndex()
        {
            loggerManager.microphoneIndex = microphoneSelectionDropdown.value;
        }

        /// <summary> Opens the default file locations and shows them in the fields at the bottom of the Instructor's Interface <summary>
        public void UI___OpenDefaultFileLocation()
        {
            xmlQuestionnaireLocField.text = loggerManager.xmlQuestionnaireLocation;
            openVRXDeviceConfigsField.text = loggerManager.openVRRuntimeConfig;
            xmlProjectSettings.text = loggerManager.xmlFileForProjectSettings;
            projectNameField.text = loggerManager.projectName;
            microphoneIndexField.text = loggerManager.overrideMicIndex.ToString();
            microphoneInSeconds.text = loggerManager.microphoneDurationInSecs.ToString();
            optionsPanel.SetActive(true);
        }

        /// <summary> sets the loggerManager's properties based on the fields in the UI <summary>
        public void UI___ConfirmOutputDirectory()
        {
            loggerManager.projectName = projectNameField.text;
            loggerManager.outputFolder = outputFileField.text;
            loggerManager.trialNumber = int.Parse(trialNumberField.text);
        }

        /// <summary> Loads project settings from an XML file <summary>
        public void UI__LoadProjectSettings()
        {
            loggerManager.xmlFileForProjectSettings = xmlProjectSettings.text;
            loggerManager.UI_LoadProjectSettingsClass();

            xmlQuestionnaireLocField.text = loggerManager.xmlQuestionnaireLocation;
            openVRXDeviceConfigsField.text = loggerManager.openVRRuntimeConfig;
            xmlProjectSettings.text = loggerManager.xmlFileForProjectSettings;
            projectNameField.text = loggerManager.projectName;
            //microphoneIndexField.text = loggerManager.overrideMicIndex.ToString();
            //microphoneInSeconds.text = loggerManager.microphoneDurationInSecs.ToString();
        }

        /// <summary> Confirms directory options updates settinfs in loggerManager, and hides the options panel  <summary> 
        public void UI__ConfirmDirectoryOptions()
        {
            loggerManager.xmlQuestionnaireLocation = xmlQuestionnaireLocField.text;
            loggerManager.openVRRuntimeConfig = openVRXDeviceConfigsField.text;
            loggerManager.xmlFileForProjectSettings = xmlProjectSettings.text;
            //loggerManager.overrideMicIndex = int.Parse(microphoneIndexField.text);
            //loggerManager.microphoneDurationInSecs = int.Parse(microphoneInSeconds.text);
            //loggerManager.projectName = projectNameField.text;

            optionsPanel.SetActive(false);
            SetupDropdownUI();
        }

        /// <summary> Clears the text fields and hides the options panel <summary>
        public void UI__CancelDirectoryOptions()
        {
            xmlQuestionnaireLocField.text = "";
            openVRXDeviceConfigsField.text = "";
            xmlProjectSettings.text = "";
            //projectNameField.text = "";
            microphoneIndexField.text = "";

            optionsPanel.SetActive(false);
        }

        /// <summary> Initiates the recording process <summary>
        public void UI___StartRecording()
        {
            loggerManager.enableRecordingTimer = enableRecordingTimer.isOn;
            loggerManager.microphoneIndex = microphoneSelectionDropdown.value;
            //Handle Microphone stuff. 
            loggerManager.recordMicrophone = recordTheMicrophoneToggle.isOn;
            int microphoneTotalTime = TMPProTimerPhraser(microphoneInSeconds);

            int totalTime = 10;
            if(recordingTrialTime.text.Contains(":"))
            {
                try
                {
                    System.TimeSpan time = new System.TimeSpan();
                    System.TimeSpan.TryParse(recordingTrialTime.text, out time);
                    totalTime = (int)time.TotalSeconds;
                }
                catch
                {
                    Debug.LogWarning("Invalid time format.");
                }
            }
            else
            {
                totalTime = int.Parse(recordingTrialTime.text);
            }


            if (enableRecordingTimer.isOn)
                loggerManager.microphoneDurationInSecs = totalTime; 
            else
                loggerManager.microphoneDurationInSecs = microphoneTotalTime;

            //Regular startup ops. 
            loggerManager.StartRecording();
            questionnaireExportLocation.text = loggerManager.GetQuestionnaireOutputLocation();
            isRecordingToggle.isOn = loggerManager.isRecording;
        }

        /// <summary> Terminates the recording process <summary>
        public void UI___StopRecording()
        {
            //This is to export the project settings. Yes I know it's jank.
            if(exportOpenVRDetailsToggle.isOn)
            {
                loggerManager._ExportProjectProperties(); 
            }
            loggerManager.StopRecording();
            isRecordingToggle.isOn = loggerManager.isRecording;
        }

        private void StopRecordingToggle()
        {
            isRecordingToggle.isOn = false;
        }

        /// <summary> Updates the image shown in the left and right frames based on OVRT Manager. <summary>
        public void UI___UpdateImageFrames()
        {
            leftFrameImage.mainTexture = ovrtManagerInstance.leftEyeTexture;
            rightFrameImage.mainTexture = ovrtManagerInstance.rightEyeTexture;
        }

        /// <summary> Clears the directory related text fields and hides the options panel  <summary>
        public void UI__CloseConfigurationDirectory()
        {
            xmlQuestionnaireLocField.text = "";
            openVRXDeviceConfigsField.text = "";
            xmlProjectSettings.text = "";
            projectNameField.text = "";
            microphoneIndexField.text = "";

            optionsPanel.SetActive(false);
        }

        /// <summary> Saves settings and quits application <summary>
        public void UI__CloseApplication()
        {
            //loggerManager.UI_SaveProjectSettingsClass();
            loggerManager.UI_StoreStartupProjectSettingsToDefault();
            Application.Quit();
        }


        public void UI_ToggleRecordingTimer()
        {
            var inputText = recordingTrialTime.text;
            int outSeconds = 0;
            if (inputText.Contains(":"))
            {
                try
                {
                    System.TimeSpan time = new System.TimeSpan();
                    System.TimeSpan.TryParse(inputText, out time);
                    outSeconds = (int) time.TotalSeconds;
                }
                catch
                {
                    Debug.LogWarning("Invalid time format.");
                }
            }
            else
            {
                //This can be made prettier but just copy pasting...
                outSeconds = int.Parse(recordingTrialTime.text);
                ConvertSecondsToStringOutput(outSeconds, recordingTrialTime);
            }
 
            loggerManager.maxRecordingTimer = outSeconds;
        }

        int TMPProTimerPhraser(TMPro.TMP_InputField field)
        {
            var inputText = field.text;
            int outSeconds = 0;

            Debug.Log(inputText);

            if (inputText.Contains(":"))
            {
                try
                {
                    System.TimeSpan time = new System.TimeSpan();
                    System.TimeSpan.TryParse(inputText, out time);
                    outSeconds = (int)time.TotalSeconds;
                }
                catch
                {
                    Debug.LogWarning("Invalid time format.");
                }
            }
            else
            {
                //This can be made prettier but just copy pasting...
                outSeconds = int.Parse(field.text);
                ConvertSecondsToStringOutput(outSeconds, field);
            }

            return outSeconds;
        }

        private void ConvertSecondsToStringOutput(int secondsIn, TMPro.TMP_InputField field)
        {
            var timeFormatted = System.TimeSpan.FromSeconds((double)
                secondsIn);

            string hr = "";
            string min = "";
            string sec = "";

            if (timeFormatted.Hours < 10)
                hr = "0" + timeFormatted.Hours.ToString();
            else
                hr = timeFormatted.Hours.ToString();

            if (timeFormatted.Minutes < 10)
                min = "0" + timeFormatted.Minutes.ToString();
            else
                min = timeFormatted.Minutes.ToString();

            if (timeFormatted.Seconds < 10)
                sec = "0" + timeFormatted.Seconds.ToString();
            else
                sec = timeFormatted.Seconds.ToString();

            field.text = hr + ":" + min + ":"
                + sec;
        }



        /// <summary> Initializes some UI fields and settings  <summary>
        private void Start()
        {
            outputFileField.text = loggerManager.outputFolder;
            projectNameField.text = loggerManager.projectName;
            trialNumberField.text = loggerManager.trialNumber.ToString();
            isRecordingToggle.isOn = loggerManager.isRecording;
            UI__ReloadandResetMicrophoneDropdown();
            optionsPanel.SetActive(false);
        }

        private void OnEnable()
        {
            QuestionnaireEvents.StopRecordingSignal.AddListener(_recordingStopped);
        }

        private void OnDisable()
        {
            QuestionnaireEvents.StopRecordingSignal.RemoveListener(_recordingStopped);
        }

        public void UI__TogglePause()
        {
            var state = !loggerManager.pauseRecording;
            loggerManager._TogglePauseRecording(state, true);
        }

        private void UpdateTotalTimeSpentRecording()
        {
            var timeFormatted = System.TimeSpan.FromSeconds((double)
                    loggerManager.timeSpentRecording);

            string hr  = "";
            string min = ""; 
            string sec = ""; 

            if (timeFormatted.Hours < 10)
                hr = "0" + timeFormatted.Hours.ToString();
            else
                hr = timeFormatted.Hours.ToString();

            if (timeFormatted.Minutes < 10)
                min = "0" + timeFormatted.Minutes.ToString();
            else
                min = timeFormatted.Minutes.ToString();

            if (timeFormatted.Seconds < 10)
                sec = "0" + timeFormatted.Seconds.ToString();
            else
                sec = timeFormatted.Seconds.ToString();

            currentRecordingTime.text = hr + ":" + min + ":" 
                + sec;
           
        }


        float recordingDotTimer = 0.75f;
        float recordingTime = 0.0f;
        //bool toggleColor = false; 

        /// <summary> Called once per frame and updates the image every frame.  <summary>
        private void Update()
        {
            UI___UpdateImageFrames();
            UpdateTotalTimeSpentRecording();

            if (loggerManager.pauseRecording)
            {
                pauseMessage.SetActive(true);
            }
            else
            {
                pauseMessage.SetActive(false);
            }

            if (isRecordingToggle.isOn)
            {
                if (recordingTime > recordingDotTimer)
                {
                    if (recordingDot.color == Color.red)
                    {
                        recordingDot.color = Color.gray;
                    }
                    else
                    {
                        recordingDot.color = Color.red;
                    }
                    recordingTime = 0.0f;
                }


                 recordingTime += Time.deltaTime;
            }
            else
            {
                recordingDot.color = Color.grey;
            }
        }
    }
}