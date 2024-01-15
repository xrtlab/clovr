using UnityEngine.Events;

namespace XRT_OVR_Grabber
{ 
    public static class QuestionnaireEvents
    {
        public static UnityEvent<string> QuestionnaireButtonPressedNextQ = new UnityEvent<string>();                        // Sent when the user or instruction press to move to the next question in the questionnaire. 
        public static UnityEvent<ControllerInteraction> AddControllerRecord = new UnityEvent<ControllerInteraction>();      // Sent whenever a pose is generated per OVRT TrackedObject. 
        public static UnityEvent<string> ControllerTrigger = new UnityEvent<string>();                                      // Sent when the controller has the trigger button pressed. Unused. 
        public static UnityEvent ToggleRecordingMode = new UnityEvent();                                                    // Toggles the recording mode of the project. 
        public static UnityEvent QuestionnaireSaveAll = new UnityEvent(); 
        public static UnityEvent QuestionnaireFinished = new UnityEvent();
        public static UnityEvent<bool> HeadsetStatusChange = new UnityEvent<bool>();
        public static UnityEvent<string> TriggerAction = new UnityEvent<string>();
        public static UnityEvent InvertControllerBiding = new UnityEvent();
        public static UnityEvent<bool> ToggleKeyboard = new UnityEvent<bool>();
        public static UnityEvent StopRecordingSignal = new UnityEvent();
        public static UnityEvent ProjectInitialized = new UnityEvent();
    }
}
