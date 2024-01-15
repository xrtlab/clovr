using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XRT_OVR_Grabber
{
    public class UISubcategory : MonoBehaviour
    {
        Text subcategoryLabel;
        public GameObject labelBox;
        public GameObject spawnBar;
        public GameObject buttonPrefab;
        public List<GameObject> trackedButtons;

       
        public void SetQuestionButtons(string answers, string labels)
        {
            var answerList = answers.Split(",");
            var labelList = labels.Split(",");
            subcategoryLabel = labelBox.GetComponent<Text>();

            int index = 0; 
            foreach (string s in answerList)
            {
                var gameObj = Instantiate(buttonPrefab, spawnBar.transform);
                var buttonClass = gameObj.GetComponent<UIAnswerButton>();
                buttonClass.SetButtonResponse(s);
                if (index < labelList.Length)
                {
                    //Plops the label with the adjacent label name. 
                    buttonClass.SetButtonLabel(labelList[index]);
                }
                else
                {
                    //Hides the label marker. 
                    buttonClass.SetButtonLabel("");
                }
                trackedButtons.Add(gameObj);
                index++; 
            }
        }

        public void SetQuestionLabel(string categoryName = "")
        {
            //Debug.Log(categoryName);
            subcategoryLabel = labelBox.GetComponent<Text>();
            subcategoryLabel.text = categoryName;
        }

        public void ClearButtons()
        {
            foreach (GameObject g in trackedButtons)
            {
                Destroy(g);
            }
            trackedButtons.Clear();
        }

    }
}