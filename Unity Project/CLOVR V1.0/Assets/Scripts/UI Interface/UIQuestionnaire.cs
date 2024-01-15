using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace XRT_OVR_Grabber
{

    public class UIQuestionnaire : MonoBehaviour
    {
        //Event action
        private UnityAction _FinishedQuestionnaire;
        SurveyInterface QuestionnaireInterface; 

        //Gameobjects used for prefabs 
        public GameObject titleScreenPrefab;
        public GameObject questionScreenPrefab;
        public GameObject finishedScreenPrefab;
        //public GameObject answerButtonPrefab;

        Text titleScreenUI;
        Text questionScreenUI;
        List<GameObject> AnswerButtons;

        UISubcategory questionnaireInterfaceUI; 

        public void _InitializeQuestionnaire()
        {
            titleScreenPrefab.SetActive(true); 
        }

        public void _StartQuestionnaire()
        {
            titleScreenPrefab.SetActive(false);
            questionScreenPrefab.SetActive(true); 
        }

        public void SetupNextQuestion()
        {
            QuestionnaireInterface._GetCurrentQuestionValues();
        }            

        //This was changed and is usually performed in the Surveys script instead. 
        public void _MoveToNextQuestion()
        {
            //string buttonValue = ""; 
            //QuestionnaireEvents.QuestionnaireButtonPressedNextQ.Invoke(buttonValue);
        }

        public void _MoveToCompletedScreen()
        {

            questionScreenPrefab.SetActive(false);
            finishedScreenPrefab.SetActive(true);
        }

        public void _CloseQuestionnaireFinal()
        {
            titleScreenPrefab.SetActive(false);
            questionScreenPrefab.SetActive(false);
            finishedScreenPrefab.SetActive(false);
        }

        public void Awake()
        {
            _CloseQuestionnaireFinal();
            questionnaireInterfaceUI = GameObject.Find("Subcategory Question").GetComponent<UISubcategory>();

            //_FinishedQuestionnaire += _MoveToNextQuestion;

            //_onNewPosesAction += OnNewPoses;
            // _onDeviceConnectedAction += OnDeviceConnected;
        }


    }
}