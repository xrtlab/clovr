//////////////////////////////////////
/// <summary>
///  Unity Process Passthrough
///  
///  Use this API to call a library to call an external process. This API does not 1:1 match Unity's System.Diagnostics
///  API however this strives to at least call a function externally. 
///  </summary>
using UnityEngine;
using System.Runtime.InteropServices;


namespace UnityProcessPassthrough
{ 
    public class UPP
    {
        /// <summary>
        /// These first three are used for testing so don't take them seriously. 
        /// </summary>
        [DllImport("StartProcessUnity")]
        private static extern void TestCall();

        [DllImport("StartProcessUnity")]
        private static extern void StoreTestValue(int value);

        [DllImport("StartProcessUnity")]
        private static extern int RetrieveValue();

        /// <summary>
        /// Test case to check if the library can store a value.
        /// </summary>
        /// <param name="value"></param>
        public static void StoreValue(int value)
        {
            StoreTestValue(value);
        }

        /// <summary>
        /// Test case to check if the library can recall a value. 
        /// </summary>
        /// <returns></returns>
        public static int RecallValue()
        {
            return RetrieveValue();
        }
        /// <summary>
        /// These next four will use the functions from the Unity Process Passthrough (UPP). 
        /// </summary>
        [DllImport("StartProcessUnity")]
        private static extern int StartProcess(System.IntPtr executablePath, System.IntPtr argument, System.IntPtr workingDirectory); // Creates a new CMD process.
        [DllImport("StartProcessUnity")]
        private static extern int WriteToActiveProcess(System.IntPtr inputString);              // Talks to an active CMD process.
        [DllImport("StartProcessUnity")]
        private static extern System.IntPtr ReadFromActiveProcess();                             // Retrieves CMD process responses.
        [DllImport("StartProcessUnity")]
        private static extern void KillAllProcessHandles();								// Kills active process and pipes. 


        /// <summary>
        /// Creates a new process, sort of similar to StartProcess() provided by Unity. 
        /// </summary>
        /// <param name="executableLocation"></param>
        /// <param name="arguments"></param>

        public static void CreateProcess(string executableLocation, string arguments, string workingDir)
        {
            int status = 0; 
            status = StartProcess(
                Marshal.StringToHGlobalAnsi(executableLocation), 
                Marshal.StringToHGlobalAnsi(arguments),
                Marshal.StringToHGlobalAnsi(workingDir));


            if(status == 0)
            {
                Debug.Log("No errors");
            }
            else
            {
                Debug.Log("Something wrong happened");
                Debug.Log(status);
            }
        }


        /// <summary>
        /// Stops the process in UPP library.
        /// </summary>
        public static void StopProcess()
        {
            KillAllProcessHandles();
        }

        /// <summary>
        /// Sends a command to the console window where the process is running.
        /// </summary>
        /// <param name="input"></param>
        public static void SendCmdToProcess(string input)
        {
            var value = WriteToActiveProcess(Marshal.StringToHGlobalAnsi(input));
            Debug.Log(value);
        }

        /// <summary>
        /// Reads whatever was posted on the command window of the currently running process. 
        /// </summary>
        /// <returns></returns>
        public static string ReadCmdFromProcess()
        {
            string output = null;
            System.IntPtr processOutput = ReadFromActiveProcess();
            output = Marshal.PtrToStringAnsi(processOutput);
            return output; 
        }
    }
}



