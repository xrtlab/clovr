using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;

//Utility script for importing and flipping pictures.
/// Problem: We need to slowly buffer, flip, mirror, and save the textures as we go through them. 
namespace XRT_OVR_Grabber
{
    public class ImageFlipper : MonoBehaviour
    {
        [SerializeField]
        public ComputeShader cs;

        [Tooltip("Toggle whether to flip the image across the x-axis")]
        public bool flipXAxis = true;

        [Tooltip("Toggle whether to flip the image across the y-axis")]
        public bool flipYAxis = true;

        [Tooltip("Toggle whether to flip the image across the diagonal axis")]
        public bool flipDiag = false;

        private RenderTexture image;

        public void FixAndProcessImages(string fileLocation, bool autoProcessBothEyes)
        {
            if (autoProcessBothEyes)
            {
                OpenImagesFromDirectory(Path.Combine(fileLocation , "leftEye"));
                OpenImagesFromDirectory(Path.Combine(fileLocation , "rightEye"));
            }
            else
            {
                OpenImagesFromDirectory(fileLocation);
            }
        }

        public void OpenImagesFromDirectory(string file_location)
        {
            if (!Directory.Exists(file_location))
            {
                Debug.LogError("Directory not valid");
                return;
            }
            var files = Directory.GetFiles(file_location);
            if (files.Length == 0)
            {
                Debug.LogError("Directory is empty.");
                return;
            }

            StartCoroutine(ProcessListOfFiles(files));
        }

        IEnumerator ProcessListOfFiles(string[] files)
        {
            foreach (string name in files)
            {

                Debug.Log(name);
                if (!name.Contains(".jpg"))
                    continue;

                Debug.Log(name);

                Texture2D input = new Texture2D(256, 256);
                byte[] data = File.ReadAllBytes(name);
                input.LoadImage(data);
                image = new RenderTexture(input.width, input.height, 24, RenderTextureFormat.ARGB32);

                //CSFlipAndMirror(input, "FlipXAxis");
                //CSFlipAndMirror(outputTexture, "FlipYAxis");
                //Texture2D t = CSFlipAndMirror(input, "FlipXAxis");
                //t = CSFlipAndMirror(t, "FlipYAxis");


                Texture2D t = FlipAndMirrorPictures(input);
                data = t.EncodeToJPG(95);
                Destroy(input);
                Destroy(t);

                //System.IO.File.Delete(name);
                System.IO.File.WriteAllBytes(name, data);

                yield return null;
            }
        }


        Texture2D outputTexture;


        /// <summary>
        /// Using this codebase for some specific flipping and mirroring. 
        /// https://christianjmills.com/posts/flip-image-compute-shader-tutorial/index.html
        /// 
        /// </summary>
        /// <param name="tIn"></param>
        /// <param name="varAction"></param>
        private void CSFlipAndMirror(Texture2D tIn, string varAction) 
        {
            // Specify the number of threads on the GPU
            int numthreads = 8;
            // Get the index for the PreprocessResNet function in the ComputeShader
            int kernelHandle = cs.FindKernel(varAction);

            /// Allocate a temporary RenderTexture
            RenderTexture result = RenderTexture.GetTemporary(tIn.width, tIn.height, 24);
            // Enable random write access
            result.enableRandomWrite = true;
            // Create the RenderTexture
            result.Create();

            Graphics.Blit(tIn, image); 

            // Set the value for the Result variable in the ComputeShader
            cs.SetTexture(kernelHandle, "Result", result);
            // Set the value for the InputImage variable in the ComputeShader
            cs.SetTexture(kernelHandle, "InputImage", image);
            // Set the value for the height variable in the ComputeShader
            cs.SetInt("height", image.height);
            // Set the value for the width variable in the ComputeShader
            cs.SetInt("width", image.width);

            // Execute the ComputeShader
            cs.Dispatch(kernelHandle, tIn.width / numthreads, tIn.height / numthreads, 1);

            var req = AsyncGPUReadback.Request(result, 0, 0, tIn.width, 0, tIn.height, 0, result.volumeDepth, ProcessFinalOutput);
            while (!req.done)
            {
                Debug.LogWarning("Lol");
                //Spin.
            }
            // Copy the flipped image to tempTex
            //Graphics.Blit(result, tempTex);

            // Release the temporary RenderTexture
            //RenderTexture.ReleaseTemporary(result);

            result.Release();
        }

        void ProcessFinalOutput(AsyncGPUReadbackRequest request)
        {
            if (!request.hasError)
            {
                if (outputTexture != null)
                    Destroy(outputTexture);

                outputTexture = new Texture2D(request.width, request.height, TextureFormat.RGBA32, false);  // Create CPU texture array

                // Copy the data
                for (var i = 0; i < request.layerCount; i++)
                    outputTexture.SetPixels32(request.GetData<Color32>(i).ToArray(), i);
                outputTexture.Apply(); 
            }
        }



        private Texture2D FlipAndMirrorPictures(Texture2D t)
        {
            Texture2D firstFlip = new Texture2D(t.width, t.height);
            Texture2D secondFlip = new Texture2D(t.width, t.height);
            int x = t.width;
            int y = t.height;

            //Flip horizontally
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    firstFlip.SetPixel(x - i - 1, j, t.GetPixel(i, j));
                }
            }
            firstFlip.Apply();

            //Flip vertically
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    secondFlip.SetPixel(i, y - j - 1, firstFlip.GetPixel(x - i - 1, j));
                }
            }
            secondFlip.Apply();
            Destroy(firstFlip);
            return secondFlip;
        }

            
    }
}