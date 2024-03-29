﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.IO;

namespace XRT_OVR_Grabber
{
	public class IOManager : MonoBehaviour
	{
		public string fileExtension;
		string outputFolder;
		string projectName;
		string partUID;
		string techniqueName;
		int trialNumber;
		public float timeStartedLogging;
		public string currentProjectOutput = ""; 

		///Current physical locations storing data.
		private string storageLocPoses;
		private string storageLocInteractions;
		public string storageLocPictures;
		public string storageLocQuestionnaire; 
		public string storageLocPoseAndInteractions;
		public string storageLocSkeletons;
		public string storageLocMicrophone;
		public string storageLocVideo; 
		public string storageLocProjectProperties;
		public string storageOutputLocationMicrophone;


		/// <summary> Constructor, initializes with details about file extensions, output folder, project name, and trial number  </summary>
		public IOManager(string fE, string outFolder, string projName, int trialNum)
        {
			fileExtension = fE;
			outputFolder = outFolder;
			projectName = projName;
			trialNumber = trialNum;
        }
		/// <summary> Updates file extension, output folder, project name, and trial number.  </summary>
		public void UpdateLoggerDetails(string fE, string outFolder, string projName, int trialNum, string _participantUID, string _techniqueName)
        {
			fileExtension = fE;
			outputFolder = outFolder;
			projectName = projName;
			trialNumber = trialNum;
			partUID = _participantUID;
			techniqueName = _techniqueName;

		}



		/// <summary> Sets up and verifies directories for storing output data. Returns true if successful. </summary>
		public bool DirectoryCreator()
        {
			var outputLocation = outputFolder + "/Data/" + 
				projectName + "_" + partUID + "_" + techniqueName + "_" + trialNumber + "/";



			VerifyOrCreateDirectory(outputLocation);
			string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff");		
			currentProjectOutput = outputLocation;
 
			//Create and set pose and interaction location
			storageLocPoseAndInteractions = Path.Combine(outputLocation, "Poses");
			VerifyOrCreateDirectory(storageLocPoseAndInteractions);
			storageLocSkeletons = Path.Combine(storageLocPoseAndInteractions, "Skeleton_Data_" + timeStamp + ".csv");
			storageLocPoseAndInteractions = Path.Combine(storageLocPoseAndInteractions, "Poses_and_Interactions_" + timeStamp + ".csv");

			//Create and set questionnaire location.
			storageLocQuestionnaire			= Path.Combine(outputLocation, "Questionnaires");  //"questionnaire_"  + timeStamp;
			VerifyOrCreateDirectory(storageLocQuestionnaire);
			storageLocQuestionnaire			= Path.Combine(storageLocQuestionnaire, timeStamp + "_");


			//Project properties, nothing too special here. 
			storageLocProjectProperties		= outputLocation + "Project_Properties_" + timeStamp + ".csv"; ;

			//Video outputs.
			storageLocVideo = Path.Combine(outputLocation, "Videos");
			VerifyOrCreateDirectory(storageLocVideo);

			//Create Eye Directories
			storageLocPictures				= outputLocation + "Pictures/";
			var eyePath = Path.Combine(storageLocPictures + "leftEye");
			VerifyOrCreateDirectory(eyePath);

			eyePath = Path.Combine(storageLocPictures + "rightEye");
			VerifyOrCreateDirectory(eyePath);

			//Create directory for microphone output
			storageLocMicrophone = outputLocation;
			var microphonePath = Path.Combine(storageLocMicrophone + "Microphone_recordings");
			VerifyOrCreateDirectory(microphonePath);
			storageOutputLocationMicrophone = microphonePath;
			storageLocMicrophone = microphonePath + "/Audio_Recording_" + timeStamp + ".wav";

			return true;
		}

		/// <summary> Checks if directory exists. Creates it if it doesn't <summary> 
		private void VerifyOrCreateDirectory(string address)
        {
			if (!System.IO.Directory.Exists(address))
			{
				System.IO.Directory.CreateDirectory(address);
			}
		}

		/// <summary> Returns the current project output location. </summary>
		public string GetCurrentProjectOutputLocation()
        {
			return currentProjectOutput;
        }

		/// <summary> Returns storage location for microphone recordings. </summary>
		public string GetMicrophoneOutputlocation()
		{
			return storageOutputLocationMicrophone;
		}

		/// <summary> Returns storage location for video recordings.  </summary>
		public string GetVideoOutputLocation()
        {
			return storageLocVideo;
        }


		public async Task<bool> SaveSkeletonDataThread(List<SkeletonBone> skeletonPoses)
        {
			//Output location for the file
			string outputLocation = storageLocSkeletons;
			bool fileExists = File.Exists(outputLocation);
			string header = ""; 
			
			if(!fileExists)
            {
				//header = 
            }




			return true; 
		}

		/// <summary> The number of poses in the starting pose will be the number of headers that will be printed out as well as information avaliable to the recording API. </summary>
		public async Task<bool> SavePosesAndInteractionsThread(List<PoseInteractionLog> posesAndInteractions)
		{
			//Output location for the file
			string outputLocation = storageLocPoseAndInteractions;
			bool fileExists = File.Exists(outputLocation);

			//Appending the frame and time collected time stamps. 
			string header = "";

			//Check if the file exists, if it doesn't create the header and append. 
			if (!fileExists)
			{
				header += "Frame,Time Collected," + ControllerInteraction.PrintHeaderDefault();

				List<XRT_OVR_Grabber.Pose> poseList = posesAndInteractions[0].poses;

				for (int i = 0; i < poseList.Count; i++)
				{
					//XRT_OVR_Grabber.Pose p = poseList[i];
					header += "," + XRT_OVR_Grabber.Pose.PrintHeader();
				}
				header += "\n";
			}

			try
			{
				StreamWriter writer = new StreamWriter(outputLocation, true);
				string outputToWriter = "";
				//If the file already existed, we shouldn't add another header to the same file. 
				if (!fileExists)
					outputToWriter = header;

				foreach (PoseInteractionLog record in posesAndInteractions)
                {
					string poseStringlet = "";
					var poses = record.poses;
					poseStringlet += poses[0].GetFrameRecorded() + ",";//Prints the frame this item was collected at. 
					poseStringlet += poses[0].GetTimeRecorded()  + ",";

					if(record.interactions.Count > 0)
                    {
						poseStringlet += record.interactions[0].SendToString();
                    }
                    else
                    {
						poseStringlet += ControllerInteraction.PrintEmptyLineDefault();  //record.interaction.SendToString();
					}


					//Compress all the poses into one record
					foreach (XRT_OVR_Grabber.Pose p in poses)
                    {
						poseStringlet += "," + p.SendToString();
                    }
					//Per record export.
					outputToWriter += poseStringlet + "\n";
					
					if(record.interactions.Count > 1)
                    {
						for (int i = 1; i < record.interactions.Count; i++)
						{
							outputToWriter += ",," + record.interactions[i].SendToString() + "\n";
						}
					}
                }

				//	Debug.Log(posesSentToString);
				writer.Write(outputToWriter);
				writer.Close();
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
			}
            finally
            {
				posesAndInteractions = null;
            }

			return true;
		}


		string[] preferredDeviceOrder = { "HMD","LeftHand","RightHand" };

		/// <summary> The number of poses in the starting pose will be the number of headers that will be printed out as well as information avaliable to the recording API. </summary>
		public async Task<bool> SavePosesAndInteractionsThread(List<PoseInteractionLog> posesAndInteractions, int deviceCount)
		{
			//Output location for the file
			string outputLocation = storageLocPoseAndInteractions;
			bool fileExists = File.Exists(outputLocation);

			//Appending the frame and time collected time stamps. 
			string header = "";


			// Timestamp, Active devices, <Device poses>, Interactions, <Interactions in that timestamp>
			//Data output to file. 
			try
			{
				StreamWriter writer = new StreamWriter(outputLocation, true);
				string outputToWriter = "";
				
				//Check if the file exists, if it doesn't create the header and append. 
				if (!fileExists)
				{
					header += "timestamp,number_of_devices";

					//Dynamic Pose header. 
					header += posesAndInteractions[0].PrintPoseHeader();


					header += ",number_of_interactions";

					//Need to update this function in order to just print the relevant controllers, not every device. 
					//header += ControllerInteraction.PrintHeaderDefault();
					foreach(PoseInteractionLog p in posesAndInteractions)
                    {
						if(p.interactions.Count == 0)
                        {
							continue;
                        }
						else
                        {
							header += p.ObtainInteractionPattern();
							break;
                        }
                    }
					//header += posesAndInteractions[0].ObtainInteractionPattern();

					header += "\n";
				}
				//If the file already existed, we shouldn't add another header to the same file. 
				if (!fileExists)
					outputToWriter = header;


				foreach (PoseInteractionLog record in posesAndInteractions)
				{
					string poseStringlet = "";
					var poses = record.poses;
					//poseStringlet += poses[0].GetFrameRecorded() + ",";//Prints the frame this item was collected at. 
					poseStringlet += poses[0].GetTimeRecorded() + ",";
					poseStringlet += deviceCount;
                    for (int i = 0; i < deviceCount; i++)
                    {
                        if (i < poses.Count)
                        {
                            poseStringlet += "," + poses[i].SendToString2();
                        }
                        else
                        {
                            poseStringlet += "," + Pose.PrintEmpty2();

                        }
                    }
                    poseStringlet += "," + record.interactions.Count;

					//Interaction Block.
					foreach (ControllerInteraction action in record.interactions)
                    {
						poseStringlet += "," + action.SendToString2();
					}

					//Per record export.
					outputToWriter += poseStringlet + "\n";
				}

				//	Debug.Log(posesSentToString);
				writer.Write(outputToWriter);
				writer.Close();
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
			}
			finally
			{
				posesAndInteractions = null; // .Clear();
			}

			return true;
		}



		///// <summary> Saves interaction data </summary>
		//IEnumerator SaveInteractionsThreading(List<ControllerInteraction> interactionBuffer)
		//      {
		//	string outputLocation = storageLocInteractions;
		//	Debug.Log(outputLocation);
		//	Debug.Log(interactionBuffer.Count);
		//	try
		//	{
		//		StreamWriter writer = new StreamWriter(outputLocation, true);
		//		string interactionsSentToString = "";
		//		foreach (ControllerInteraction interaction in interactionBuffer)
		//		{
		//			interactionsSentToString += interaction.SendToString() + "\n";
		//		}
		//		Debug.Log(interactionsSentToString);
		//		writer.Write(interactionsSentToString);
		//		writer.Close();
		//	}
		//	catch (System.Exception e)
		//	{
		//		Debug.LogError(e);
		//	}
		//	return interactionBuffer.GetEnumerator();
		//      }

		///// <summary> Saves interaction data </summary>
		//public async Task<bool> SaveInteractionsThread(List<ControllerInteraction> interactionBuffer)
		//{
		//	Debug.Log("Save interactions thread");
		//	Debug.Log(interactionBuffer[0]);
		//	string outputLocation = storageLocInteractions;
		//	//Debug.Log(outputLocation);
		//	try
		//	{
		//		StreamWriter writer = new StreamWriter(outputLocation, true);

		//		string interactionsSentToString = "";
		//		foreach (ControllerInteraction interaction in interactionBuffer)
		//		{
		//			interactionsSentToString += interaction.SendToString() + "\n";
		//		}
		//		writer.Write(interactionsSentToString); 
		//		writer.Close();
		//	}
		//	catch (System.Exception e)
		//	{
		//		Debug.LogError(e);
		//	}
		//	return true; 
		//}

		/// <summary> This method will be used to save the results obtained using the survey add-on </summary> 
		public bool SaveSurveyResults()
        {
			bool status = true;
			Debug.LogError("If you are using this method, you're doing something wrong.");
			return status; 
        }

		///// <summary> Handles saving of poses. </summary>
		//public void RunSave(List<List<XRT_OVR_Grabber.Pose>> poses)
  //      {
  //          try
  //          {
		//		Task.Run(() => SavePosesThread(poses));
		//	}
		//	catch (System.Exception e)
		//	{
		//		Debug.LogError(e);
		//	}
		//}

		///// <summary> Handles saving of poses. </summary>
		//public void RunSave(List<ControllerInteraction> interactionBuffer)
		//{
		//	try
		//	{
		//		Task.Run(() => SaveInteractionsThread(interactionBuffer));
		//	}
		//	catch (System.Exception e)
		//	{
		//		Debug.LogError(e);
		//	}
		//}

		///// <summary> Handles saving of poses. </summary>
		//public void RunSave(List<List<XRT_OVR_Grabber.Pose>> poses, List<ControllerInteraction> interactionBuffer)
		//{
		//	try
		//	{
		//		Task.Run(() => SavePosesAndInteractionsThread(poses, interactionBuffer));
		//	}
		//	catch (System.Exception e)
		//	{
		//		Debug.LogError(e);
		//	}
		//}

		/// <summary> Handles saving of poses. </summary>
		public void RunSave(List<PoseInteractionLog> posesAndInteractions, int deviceCount)
		{
			try
			{
				Task.Run(() => SavePosesAndInteractionsThread(posesAndInteractions, deviceCount));
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
			}
		}


		/// This will be fixed under a different manner. 
		/// <summary> Adjusts the header of the file to match the total count of devices </summary>
		public void FixDeviceHeader(int totalCount)
		{
			string headerOut = "Frame,Time Collected,";  

			headerOut += ControllerInteraction.PrintHeaderDefault();
		 
			for (int i = 0; i < totalCount; i++)
			{
				headerOut += "," + XRT_OVR_Grabber.Pose.PrintHeader();
			}
			headerOut += "\n";

			//Hopefully this will export the header when needed. 
			try
			{
				// Open the file in read-write mode
				using (FileStream fs = new FileStream(storageLocPoseAndInteractions, FileMode.Open, FileAccess.ReadWrite))
				{
					// Convert the modified text to bytes
					byte[] bytesToWrite = System.Text.Encoding.UTF8.GetBytes(headerOut);

					// Write the modified bytes at the beginning of the file
					fs.Seek(0, SeekOrigin.Begin);
					fs.Write(bytesToWrite, 0, bytesToWrite.Length);
				}

				Console.WriteLine("First line modified successfully.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
		}
	}
}