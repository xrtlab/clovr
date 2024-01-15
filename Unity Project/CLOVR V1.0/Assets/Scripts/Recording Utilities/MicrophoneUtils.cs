using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XRT_OVR_Grabber
{
    /// <summary>
    // "Borrowing" code from https://www.universityofgames.net/articles/capturing-audio-from-a-microphone-in-unity3d/
    // https://github.com/srcnalt/OpenAI-Unity/blob/6548d0d2ae0885b5fa557e1d800f44f0ea9a47fd/Samples~/Whisper/SaveWav.cs
    // https://gist.github.com/darktable/2317063#file-savwav-cs
    /// Represents functionality related to microphone recording using Unity's 'Microphone' API
    /// <summary>

    public class MicrophoneRecordingAPI : MonoBehaviour
    {
        //The maximum and minimum available recording frequencies    
        private int minFreq; /// <summary> the minimum available recording frequency <summary>
        private int maxFreq; /// <summary> the maximum available recording frequency <summary> 
        private bool micConnected = false;
        private bool isRecording = false;

        private int micIndex = 0;
        private AudioSource goAudioSource;

        //private SavWav savingUtility = new SavWav(); 

        /// <summary> default constructor for the class, initializes class by checking if theres atleast one microphone connected. 
        /// if connected, it fetches the default microphone capabilities and assigns an AudioSource.
        /// <summary>
        public MicrophoneRecordingAPI()
        {
            //Check if there is at least one microphone connected    
            if (Microphone.devices.Length <= 0)
            {
                //Throw a warning message at the console if there isn't    
                Debug.LogWarning("Microphone not connected!");
            }
            else //At least one microphone is present    
            {
                //Set our flag 'micConnected' to true    
                micConnected = true;
                //Get the default microphone recording capabilities    
                Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

                //According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...    
                if (minFreq == 0 && maxFreq == 0)
                {
                    //...meaning 44100 Hz can be used as the recording sampling rate    
                    maxFreq = 44100;
                }
                //Get the attached AudioSource component    
                goAudioSource = GameObject.Find("LoggingManger").GetComponent<AudioSource>();
            }
        }

        /// <summary> Saves the currently recorded audio clip to a file <summary>
        public void SaveClipToFile(string fileLocation)
        {
            try
            {
                SavWav.Save(fileLocation, goAudioSource.clip);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary> Begins recording from the microphone if it's connected and not already recording. <summary>
        public void StartRecordingMic(int duration)
        {
            if (micConnected && !isRecording)
            {
                goAudioSource.clip = Microphone.Start(Microphone.devices[micIndex], false, duration, maxFreq);
                isRecording = true;
            }
        }

        /// <summary> tops the recording and saves the audio clip to the provided output location <summary>
        public void StopRecordingMic(string outputLocation)
        {
            if (isRecording)
            {
                isRecording = false;
                Microphone.End(Microphone.devices[micIndex]);
                SavWav.Save(outputLocation, goAudioSource.clip);
            }
        }

        /// <summary> sets the index of the microphone <summary>
        public void SetMicIndex(int index)
        {
            if (Microphone.devices.Length < index)
            {
                micIndex = index;
                micConnected = true;
                Microphone.GetDeviceCaps(Microphone.devices[micIndex], out minFreq, out maxFreq);
            }
            else
            {
                Debug.Log("Microphone index invalid.");
            }
        }
    }
}
