/// <summary>
/// This file provides a comprehensive interface for executing and managing the surveys.
/// It is designed to create an interactive environemnet for participants to take surveys and store their responses for further analysis
/// </summary>


using System;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.Events;
using UnityEngine.UI;

namespace XRT_OVR_Grabber
{

/// <summary>
/// High level flowchart gives a brief overview of how a user progresses through the questionare from start to finish
    /* Basic logic path for someone doing a full survey/questionnaire portion. 
     *         
     *        Restart questionnaire
     *        |     
     *  o --- 0 --- o --- o --- 0 --- o 
     *  |     |     |     |     |     |
     *  Load questionnaire|     |     |
     *        |     |     |     |     |
     *        Questionnaire loop starts (Instructor assigns survey)
     *              |     |     |     |
     *              Questionnaire taker saves response (Next question)
     *                    |     |     |
     *                    Questionnaire complete (Instructor saves)
     *                          |     |
     *                          Clear all reponses and questions
     *                                |
     *                                Save all reponses 
     *                          
     */
    /// <summary>

    /// <summary>
    /// Main class managing entire questionare process. 
    /// <summary>
    public class SurveyInterface : MonoBehaviour
    {
        
        public GameObject titleScreenPrefab;        /// <summary> This is the gameobject of the title screen presented to the participant. <summary>
        public GameObject questionScreenPrefab;     /// <summary> This is the gameobject of the question presented to the participant. <summary>
        public GameObject finishedScreenPrefab;     /// <summary> This is the gameobject of the last screen of the questionnaires     <summary>
        public Text titleTextbox;                   /// <summary>  Used for the title - description of the questionnaire on the first screen of the questionnaire.<summary>
        public Text questionnaireNameTextbox;       ///<summary>  This is the text component of the question.<summary>
        public Text finishScreenTextbox;            /// <summary> This is the text component of the finishing portion <summary> 
        public UISubcategory questionnaireBox;      /// <summary> This controls how many questions are displayed to the participant through the questionnaire screen. <summary>
        string folderLocationForSurveys = "";       /// <summary> This is the location where the surveys are located. The user can specify any location on their computer. <summary>
        int currentlyActiveQuestionnaire = 0;       /// <summary> Sets which questionnaire is being used. 

       
        public LoggingManagerAPI LoggerManager;    
        XML_Reader XMLReader = new XML_Reader();
        List<Questionnaire> assignedQuestionnaires = new List<Questionnaire>();
        public List<string> questionnairesAsString = new List<string>(); 

 
        private UnityAction<string> _nextQuestionAction;
        private UnityAction saveAllQuestionnaireResults;
        private UnityAction finishTheQuestionnaire; 

        /// <summary>
        ///  Reads survey XML files from a specified folder. It checks if the directory exists, and if so, loads all of the XML files in that directory into the 'assignedQuestionnaires'"
        /// <summary>
        /// <returns> boolean, confirmation of success of reading files from folder. </returns>
        bool ReadSurveysFromFolder()
        {
            //List<string> filedSurveys;
            if (!Directory.Exists(folderLocationForSurveys))
            {
                Debug.LogError("Directory not valid");
                return false;
            }
            var files = Directory.GetFiles(folderLocationForSurveys);
            if (files.Length == 0)
            {
                Debug.LogError("Survey directory is empty.");
                return false;
            }

            //Load all files via XML loading. 
            questionnairesAsString.Clear();
            assignedQuestionnaires.Clear();
            foreach (string file in files)
            {
                if (file.Contains(".meta"))
                    continue;

                var survey = XMLReader.Load_XML_Questionnaire(file);

                questionnairesAsString.Add(survey.questionnaireName);
                assignedQuestionnaires.Add(survey);
            }
            //GetStringsForQuestionnaires(); 
            return true;
        }
        
        /// <summary>
        /// This function populates the 'questionaresAsString' list with the names of the questionaires
        /// <summary>
        void GetStringsForQuestionnaires()
        {
            questionnairesAsString.Clear();
            foreach(Questionnaire q in assignedQuestionnaires)
            {
                questionnairesAsString.Add(q.questionnaireName);
            }
        }

        /// <summary>
        /// This is for the UI shown on the PARTICIPANT UI. This simply updates the UI graphics.
        /// </summary>
        /// <returns></returns>
        bool UpdateQuestionnaireQuestion()
        {
            questionnaireBox.ClearButtons(); 
            var label = assignedQuestionnaires[currentlyActiveQuestionnaire].GetCurrentQuestion();
            questionnaireBox.SetQuestionLabel(label[0]);
            questionnaireBox.SetQuestionButtons(label[2], label[3]);
            return true;
        }

        /// <summary>
        /// This is for getting the current question on base of the API's current question index. 
        /// </summary>
        /// <returns></returns>
        public string[] _GetCurrentQuestionValues()
        {
            //Need to get the question, subcategories, and answers
            return assignedQuestionnaires[currentlyActiveQuestionnaire].GetCurrentQuestion();
        }

        /// <summary>
        /// This is the safe approach to loading the questionnaires and preparing them to be used by the API. 
        /// </summary>
        public void _LoadAndPrepareQuestionnaires()
        {
            var status = ReadSurveysFromFolder();
            if (!status)
                Debug.LogError("Error in loading and preparing the surveys.");

            status = UpdateQuestionnaireQuestion();
            if (!status)
                Debug.LogError("Error in loading and preparing the surveys.");
        }

        

        //////////////////////////////////////// Direct Questionnaire Controls /////////////////////////
        /// <summary>
        /// This function instructs the API to load a index-specified questionnaire. These will be loaded as they are sorted in the folder the XML files they reside in. 
        /// </summary>

        public void UI_SetupNextQuestionnaire(int index)
        {
            //QuestionnaireInterface._GetCurrentQuestionValues(); ????
            currentlyActiveQuestionnaire = index;
            _StartQuestionnaire(index);
            titleScreenPrefab.SetActive(true);
            questionScreenPrefab.SetActive(false);
            finishedScreenPrefab.SetActive(false);

            //LoggerManager.ToggleOverlayVisibility(true, LoggerManager.overlayPointerL);
            //LoggerManager.ToggleOverlayVisibility(true, LoggerManager.overlayPointerR);
        }

        /// <summary>
        /// This triggers the API to move to the next question in the questionnaire.
        /// </summary>
        public void UI_MoveToNextQuestion()
        {
            UpdateQuestionnaireQuestion();
        }
        /// <summary>
        /// This starts the Questionnaire so the participant can start viewing the questions. 
        /// </summary>
        public void UI_StartQuestionnaire()
        {
            titleScreenPrefab.SetActive(false);
            questionScreenPrefab.SetActive(true);
            UpdateQuestionnaireQuestion();

        }

        /// <summary>
        /// This triggers upon completing all the questions and show a final screen to the user, confirming they completed the questionnaire. 
        /// </summary>
        public void UI_MoveToCompletedScreen()
        {
            questionnaireBox.ClearButtons(); 
            questionScreenPrefab.SetActive(false);
            finishedScreenPrefab.SetActive(true);
        }

        /// <summary>
        /// This closes off the UI for the questionnaire and closes off the UI. 
        /// </summary>
        public void UI_CloseQuestionnaireFinal()
        {
            titleScreenPrefab.SetActive(false);
            questionScreenPrefab.SetActive(false);
            finishedScreenPrefab.SetActive(false);
            LoggerManager.ToggleOverlayVisibility(false);

            //LoggerManager.ToggleOverlayVisibility(false, LoggerManager.overlayPointerL);
            //.ToggleOverlayVisibility(false, LoggerManager.overlayPointerR);
            QuestionnaireEvents.ToggleKeyboard.Invoke(false);
        }        
        ///////////////////////////////////////////

        /// <summary>
        /// This function initializes the start of a particular questionnaire based on the given index (option)
        /// <summary>
                public void _StartQuestionnaire(int option)
        {
            currentlyActiveQuestionnaire = option;
            //TODO: I think we're binding a gameobject per each of the questionnaire's responses or somehow what will show up on screen for the participant at this point. 
            titleTextbox.text = assignedQuestionnaires[currentlyActiveQuestionnaire].GetTitle();
            questionnaireNameTextbox.text = assignedQuestionnaires[currentlyActiveQuestionnaire].GetQuestionnaireName();
        }

        [SerializeField]
        float topTimer =0.05f;
        float currentTimer = 0.0f;
        bool ghostingLock = false;
        private void Update()
        {

            if (currentTimer > topTimer)
            {
                ghostingLock = false;
            }
            else
            {
                currentTimer += Time.deltaTime;
            }

        }


        /// <summary>
        /// Saves responses and moves onto next question 
        /// <summary>
        public void _SaveResponseAndMoveToNextQuestion(string value)
        {
            if (ghostingLock)
            {
                return; 
            }
            else
            {
                ghostingLock = true;
            }

            //string inValue = "";
            assignedQuestionnaires[currentlyActiveQuestionnaire].SaveResponse(value);
            UpdateQuestionnaireQuestion();
        }


        [SerializeField]
        Material backgroundMaterial;
        IEnumerator PanelSwitchAnimation()
        {
            float colorStep = 0.0f; 

            //for (int i=0; i < 1000; i++)
            while(colorStep < 1.0f)
            {
                backgroundMaterial.color = new Color(colorStep, colorStep, colorStep);
                colorStep += 0.001f;
                yield return new WaitForSeconds(0.01f);
            }

            //for (int i = 1000; i < 1000; i++)
            while (colorStep > 0)
            {
                backgroundMaterial.color = new Color(colorStep, colorStep, colorStep);
                colorStep -= 0.001f;
                yield return new WaitForSeconds(0.01f);
            }
        } 

        /// <summary>
        /// Clears responses of the currently active questionnaire
        /// <summary>
        public void _ClearCurrentQuestionnaire()
        {
            assignedQuestionnaires[currentlyActiveQuestionnaire].ClearResponses();
        }

        public void _ClearAllQuestionnaireResponses()
        {
            foreach (Questionnaire q in assignedQuestionnaires)
            {
                q.ClearResponses(); 
            }
        }


        /// <summary>
        /// Retrieves the name of the currently active questionnare 
        /// <summary>
        public string _GetCurrentlyAssignedQuestionnaireName()
        {
            return assignedQuestionnaires[currentlyActiveQuestionnaire].questionnaireName;
        }

        /// <summary> 
        /// This clears the data of all assigned questionnaires
        /// <summary>
        public void _CancelQuestionnaires()
        {
            foreach (Questionnaire q in assignedQuestionnaires)
            {
                q.ClearQuestionnaire();
            }
        }

        /// <summary> 
        /// Exports the data out to a single unique file that contains the questionnaire data.
        /// <summary>
        public void _SaveAllReponsesAndExport()
        {
            string exportLocation = LoggerManager.GetQuestionnaireOutputLocation();
           // Debug.Log(assignedQuestionnaires.Count);
            foreach (Questionnaire q in assignedQuestionnaires)
            {
                string outputLocation =  exportLocation + q.GetQuestionnaireName() + ".csv";
                try
                {
                    StreamWriter writer = new StreamWriter(outputLocation, true);
                    string questionnairesToString = q.GetQuestionnaireHeader();
                    questionnairesToString += q.GetStringVer();
                    //Debug.Log(questionnairesToString);

                    writer.Write(questionnairesToString);
                    writer.Close();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }

                q.ClearQuestionnaire();
            }
        }

        public void DirectExportToFolder(string location)
        {
            foreach (Questionnaire q in assignedQuestionnaires)
            {
                string outputLocation = location+ "\\" + q.GetQuestionnaireName() + ".csv";
                try
                {
                    StreamWriter writer = new StreamWriter(outputLocation, true);
                    string questionnairesToString = q.GetQuestionnaireHeader();
                    questionnairesToString += q.GetStringVer();
                    //Debug.Log(questionnairesToString);

                    writer.Write(questionnairesToString);
                    writer.Close();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
                q.ClearQuestionnaire();
            }
        }


        /// <summary>
        /// Used just for clearing out files that may not work (E.g. not XML files.) 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<string> CheckIfContainsValidXMLFiles(string location)
        {
            List<string> outputFiles = new List<string>();
            foreach (string s in System.IO.Directory.GetFiles(location))
            {
                if (!s.Contains(".xml"))
                {
                    outputFiles.Add(s); 
                }
            }

            return outputFiles; 
        }


        /// <summary>
        /// Initializes survey by loading the necessary configurations and setting up event listeners. It prepaers the environment for surveys
        /// <summary>
        bool initialized = false; 
        private void _Init()
        {
            try
            {
                folderLocationForSurveys = LoggerManager.xmlQuestionnaireLocation; //Application.dataPath + "/Resources/XML_Questionnaires";
                //Debug.Log(folderLocationForSurveys);
                if (folderLocationForSurveys == "" || (!Directory.Exists(folderLocationForSurveys)))
                {
                    Debug.Log("Nothing loaded or invalid directory"); 
                    initialized = false;
                    return;
                }
                _LoadAndPrepareQuestionnaires();
                
                //Try to find the Logger Manager to administer the current experiment settings. 

                 
                UI_CloseQuestionnaireFinal();
                initialized = true;
                
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Questionnaire location not valid. Nothing loaded");
                initialized = false;
            }
        }

        /// <summary>
        /// Debugging tool for creating and writing a predefined questionnaire to an XML file
        /// <summary>
        void DebugQuestionnaire()
        {
            List<string> questions = new List<string>
            {"I felt like I was actually there in the environment of the presentation.",
            "It seemed as though I actually took part in the action of the presentation.",
            "It was as though my true location had shifted into the environment of the presentation.",
            "I felt as though I was physically present in the environment of the presentation.",
            "I experienced the environment in the presentation as though I had stepped into a different place.",
            "I was convinced that things were actually happening around me.",
            "I had the feeling that I was in the middle of the action rather than merely observing.",
            "I felt like the objects in the presentation surrounded me.",
            "I experienced both the confined and open spaces in the presentation as though I was really there.",
            "I was convinced that the objects in the presentation were located on the various sides of my body.",

            "The objects in the presentation gave me the feeling that I could do things with them.",
            "I had the impression that I could be active in the environment of the presentation.",
            "I had the impression that I could act in the environment of the presentation.",
            "I had the impression that I could reach for the objects in the presentation.",
            "I felt like I could move around among the objects in the presentation.",
            "I felt like I could jump into the action.",
            "The objects in the presentation gave me the feeling that I could actually touch them.",
            "It seemed to me that I could do whatever I wanted in the environment of the presentation.",
            "It seemed to me that I could have some effect on things in the presentation, as I do in real life.",
            "I felt that I could move freely in the environment of the presentation."};
            List<string> answers = new List<string> { "1", "2", "3", "4", "5" };
            List<string> subQuestions = new List<string> { "Self-localization", "Possible Actions" };
            List<string> labels = new List<string> { "Strongly Disagree", "Disagree","Neutral","Agree","Strongly Agree"}; 
            string title = "Please take some time for this questionnaire.";
            string questionnaireName = "Spatial Precence Experience Scale";

            Questionnaire _q = new Questionnaire(questionnaireName, title, questions, subQuestions, answers, labels);
            string outputLoc = Application.dataPath + "/Resources/output.xml";
            XMLReader.Write_XML_Questtionaire(_q, outputLoc);
        }

        
        public void Awake()
        {
            _InitializeSurveyer += ManualInitialize;
            _nextQuestionAction += _SaveResponseAndMoveToNextQuestion;
            saveAllQuestionnaireResults += _SaveAllReponsesAndExport;
            finishTheQuestionnaire += UI_MoveToCompletedScreen;
            //_Init();
            //DebugQuestionnaire();
        }
        

        UnityAction _InitializeSurveyer; 

        public void ManualInitialize()
        {
            _Init(); 
            //DebugQuestionnaire();
        }
        
        public void ManualInitializationWithLocation(string location)
        {
            try
            {
                folderLocationForSurveys = location; 
                if (folderLocationForSurveys == "") 
                {
                    Debug.Log("Nothing loaded or invalid directory");
                    initialized = false;
                    return;
                }
                _LoadAndPrepareQuestionnaires();

                //Try to find the Logger Manager to administer the current experiment settings. 
                UI_CloseQuestionnaireFinal();
                LoggerManager.xmlQuestionnaireLocation = location;
                initialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Questionnaire location not valid. Nothing loaded");
                initialized = false;
            }
        }


        /// <summary>
        /// Called when a script or object becomes active. This function is being used to set up event listeners
        /// <summary>
        private void OnEnable()
        {
            QuestionnaireEvents.ProjectInitialized.AddListener(_InitializeSurveyer);
            QuestionnaireEvents.QuestionnaireButtonPressedNextQ.AddListener(_nextQuestionAction);
            QuestionnaireEvents.QuestionnaireSaveAll.AddListener(saveAllQuestionnaireResults);
            QuestionnaireEvents.QuestionnaireFinished.AddListener(finishTheQuestionnaire);
        }

        /// <summary>
        /// Called when a script becomes inactive. Used to remove the event listeners set up in 'OnEnable()'
        /// <summary>
        private void OnDestroy()
        {
            QuestionnaireEvents.ProjectInitialized.RemoveListener(_InitializeSurveyer);
            QuestionnaireEvents.QuestionnaireButtonPressedNextQ.RemoveListener(_nextQuestionAction);
            QuestionnaireEvents.QuestionnaireSaveAll.RemoveListener(saveAllQuestionnaireResults);
            QuestionnaireEvents.QuestionnaireFinished.RemoveListener(finishTheQuestionnaire);
        
        }
    }

    

    /// <summary>
    /// Represents a questionnaire with a list of questions, answers, and other metadata. Also provides methods for managing and retrieving questionnaire details
    /// <summary> 
    public class Questionnaire
    {

        public string title;
        public string questionnaireName;
        public List<string> questions;
        public List<string> subquestions;
        public List<string> answers;
        public List<string> labels;
        public List<string> timeStamps; 

        //Responses are the ones given by the user. 
        //List<List<string>> storedQuestionnaires = new List<List<string>>();
        //List<string> tempStoredResponses = new List<string>();
        List<string> storedQuestionnaires = new List<string>();
        string tempStoredResponses = ""; 


        int questionIndex    = 0;
        int subCategoryIndex = 0;
        string subcategoryResponses; 

        /// <summary> Default Constructor<summary> 
        public Questionnaire(){}
        
        /// <summary>
        /// Initializes a questionnaire with given details 
        /// <summary> 
        public Questionnaire(string _questionnaireName, string _title, List<string> _questions, List<string> _subQ, List<string> _answers, List<string> _labels)
        {
            questionnaireName = _questionnaireName;
            title = _title;
            questions = _questions;
            subquestions = _subQ;
            answers = _answers;
            labels = _labels;
        }

        /// <summary>
        /// returns the question, it's subquestions and possible answers in an array format
        /// <summary>
        public string[] GetCurrentQuestion()
        {
            string[] values = {
                (string) questions[questionIndex],
                (string) ListToString(subquestions),
                (string) ListToString(answers),
                (string) ListToString(labels)
            };
            return values; 
        }


        /// <summary>
        /// Saves the user's response. If the user has answered all sub-questions of the current question, it moves to next question 
        /// If all questions are answered, the questionnaire is considered completed, and related events are invoked
        /// <summary> 
        public void SaveResponse(string input)
        {

            //Subcategory is going to be disabled.
            /*
            if(subCategoryIndex >= subquestions.Count)
            {
                //Stores all responses given to the category. 
                subcategoryResponses += "," + input;
                subCategoryIndex = subquestions.Count;
                
                tempStoredResponses.Add(subcategoryResponses);
                questionIndex++;
            }
            else
            {
                //This takes a subcategory and pins it to a string.
                subCategoryIndex++;
                subcategoryResponses += "," + input;
                return;
            }*/

            if (questionIndex == 0)
            {
                tempStoredResponses += input;
            }
            else
            {
                tempStoredResponses += "," + input;
            }
            questionIndex++;

            if (questionIndex >= questions.Count)
            {
                SaveCompletedQuestionnaire();
                QuestionnaireEvents.QuestionnaireFinished.Invoke();
                tempStoredResponses = "";
            }
        }


        /// <summary>
        /// Saves all responses from 'tempStoredResponses' to 'storedQuestionnaires' and then clears up the temp storage
        /// <summary> 
        public void SaveCompletedQuestionnaire()
        {
            storedQuestionnaires.Add(tempStoredResponses);
            timeStamps.Add(DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
            questionIndex = 0; 
        }

        /// <summary>
        /// Creates and returns a string of all the questions in a comma separated format. 
        /// <summary> 
        public string GetQuestionnaireHeader()
        {
            string outString = "";
            bool first = true;
            foreach (string s in questions)
            {
                if (first)
                {
                    first = false;
                    outString += s;
                }
                else
                {
                    outString += "," + s;
                }
            }

            return outString + ",timestamp" + "\n";
        }

        /////////////////////////////////////////////////////////////////// These are printers for instances in the questionnaire. 
        
        /// <summary>
        /// Prints the header of the file - basically only the questions. All other information will be on the title of the questionnaire. 
        /// </summary>
        /// <returns></returns>
        public string PrintHeader()
        {
            string outputValues = "";
            foreach (string s in questions)
            {
                outputValues += s;
            }
            return outputValues + ",timestamp" + "\n";
        }

        /// <summary>
        /// This converts a given string into a spaced out version 
        /// </summary>
        /// <param name="stringIn"></param>
        /// <param name="spacer"></param>
        /// <returns></returns>
        public string ListToString(List<string> stringIn, string spacer = ",")
        {
            string stringOut = "";
            bool firstString = true;

            foreach (string s in stringIn)
            {
                if (firstString)
                {
                    stringOut += s;
                    firstString = false;
                    continue;
                }
                stringOut += spacer + s;
            }
            return stringOut;
        }
        ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clears the questionnaire of all results from all participants. Use this only if you want to erase all results.
        /// </summary>
        public void ClearQuestionnaire()
        {
            storedQuestionnaires.Clear();
            timeStamps.Clear();
            tempStoredResponses = ""; 
            ///tempStoredResponses.Clear(); 
        }

        /// <summary>
        /// Clears the currently executed questionnaire. Use this if you want to cancel the results from one participant from one instance. 
        /// </summary>
        public void ClearResponses()
        {
            tempStoredResponses = "";
        }
        
        /// <summary> Getter Function <summary> 
        public string GetQuestionnaireName()
        {
            return questionnaireName;
        }

        /// <summary> Getter Function <summary> 
        public string GetTitle()
        {
            return title; 
        }

        /// <summary> Getter Function <summary> 
        public List<string> GetQuestions()
        {
            return questions;
        }

        /// <summary> Getter Function <summary> 
        public List<string> GetSubQuestions()
        {
            return subquestions;
        }

        /// <summary> Getter Function <summary> 
        public List<string> GetAnswers()
        {
            return answers;
        }

        public List<string> GetLabels()
        {
            return labels;
        }

        /// <summary>
        /// returns all stored responses in a formatted string
        /// <summary> 
        public string GetStringVer()
        {
            string varOut = "";
            //Each column is a question and each row is a trial. 
            /*foreach(List<string> arr in storedQuestionnaires)
            {
                string tempVar = "";
                bool first = true;
                foreach (string s in arr)
                {
                    if (first)
                    {
                        tempVar += s;
                        first = false;
                    }
                    else
                    {
                        tempVar += "," + s;
                    }
                }
                varOut += tempVar + "\n";
            }*/
            int counter = 0;
            foreach(string s in storedQuestionnaires)
            {
                varOut += s +"," + timeStamps[counter] + "\n";
                counter++; 
            }

            return varOut;
        }
    }
}


// Use only for reference in manually creating a new questionnaire. 
