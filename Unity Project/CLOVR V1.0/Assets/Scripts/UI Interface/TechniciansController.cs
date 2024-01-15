using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRT_OVR_Grabber
{
    public class TechniciansController : MonoBehaviour
    {
        UIQuestionnaire questionnaireInterface;
        SurveyInterface surveyInterface;
        private void Start()
        {
            //questionnaireInterface = GameObject.Find("Dashboard_Interface").GetComponent<UIQuestionnaire>();
            surveyInterface = GameObject.Find("Dashboard_Interface").GetComponent<SurveyInterface>();

            //surveyInterface._LoadAndPrepareQuestionnaires();
            //questionnaireInterface._InitializeQuestionnaire();
        }


        private void Update()
        {
            if(Input.GetButtonDown("SetupQ"))
            {
                surveyInterface.UI_SetupNextQuestionnaire(0);
            }
            if (Input.GetButtonDown("StartQ"))
            {
                surveyInterface.UI_StartQuestionnaire();
            }

            //Input.GetKey("k");
            if (Input.GetButtonDown("NextQ"))
            {
                surveyInterface.UI_MoveToNextQuestion();
            }
            if (Input.GetButtonDown("FinishQ"))
            {
                surveyInterface.UI_CloseQuestionnaireFinal();
            }
        }
    }
}