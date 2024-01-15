using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace XRT_OVR_Grabber
{ 
    public partial class InstructorsInterface
    {

        [SerializeField]
        GameObject pictureFixerObject;

        [SerializeField]
        TMPro.TMP_InputField fileLocationField;

        [SerializeField]
        XRT_OVR_Grabber.ImageFlipper imageFixer;

        [SerializeField]
        Toggle toggleDualCameraFix;

        public void UI__ClosePictureFixer()
        {
            fileLocationField.text = "";
            pictureFixerObject.SetActive(false);
        }

        public void UI__OpenPictureFixer()
        {
            fileLocationField.text = loggerManager.GetPictureLocationStorage();
            pictureFixerObject.SetActive(true);
        }

        public void UI__StartPictureFixer()
        {
            //Might not be necessary...
            //loggerManager.SetPictureLocationStorage(fileLocationField.text);

            var location = loggerManager.GetPictureLocationStorage();
            if (location == "" || location == null)
            {
                location = fileLocationField.text;
            }

            imageFixer.FixAndProcessImages(location, toggleDualCameraFix.isOn);
            pictureFixerObject.SetActive(false);
        }
    }
}


