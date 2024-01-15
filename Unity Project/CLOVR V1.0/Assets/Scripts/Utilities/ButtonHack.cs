using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.UI
{
    public partial class  ButtonMash : Selectable 
    {
        ///Copy and paste into line 1175 on Selectable


        // Change the button to the correct state
        public void EvaluateAndTransitionToSelectionStateOut()
        {
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(currentSelectionState, false);
        }
        public void EvaluateAndTransitionToNormalStateOut()
        {
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Normal, true);
        }

        public void EvaluateAndTransitionToHighlighedStateOut()
        {
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Highlighted, true);
        }
    }
}

/*
public class ButtonHack : MonoBehaviour
{
    private void Start()
    {
        UnityEngine.UI.ButtonHackOut.EvaluateAndTransitionToHighlighedStateOut(); 
    }

}*/