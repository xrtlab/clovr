using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XRT_OVR_Grabber
{
    public class UIAnswerButton : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI response;
        public TMPro.TextMeshProUGUI label;
        public GameObject spacer;
        public GameObject labelObject; 
        public Button button;


        public void SetButtonResponse(string _label)
        {

            response.text = _label;
        }

        public void SetButtonLabel(string name)
        {
            if(name == "")
            {
                spacer.SetActive(true);
                labelObject.SetActive(false);
            }
            else
            {
                spacer.SetActive(false);
                labelObject.SetActive(true);
                label.text = name;
            }
        }

        public void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(ButtonAction);
        }

        public void ButtonAction()
        {
            //Debug.LogWarning("Screamo");
            QuestionnaireEvents.QuestionnaireButtonPressedNextQ.Invoke(response.text);
            //Debug.LogWarning("Screamo");
        }
    }
}