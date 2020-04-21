using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Numerics;
using System.Runtime.InteropServices;

public class CameraController : MonoBehaviour
{
    private bool camAvl;
    public WebCamTexture backCam;
    private Texture d_backGround;



    public RawImage background;
    public AspectRatioFitter fit;


    private void Start() {
        d_backGround = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0) {
            Debug.Log("No devices");
            camAvl = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++) {
            if (devices[i].isFrontFacing) {
                backCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if (backCam == null) {
            Debug.Log("unable bardo");
            return;
        }

        backCam.Play();
        background.texture = backCam;

        camAvl = true;
    }

    private void Update() {

        if (!camAvl)
            return;

        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;


        float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new UnityEngine.Vector3(1f, scaleY, 1f);

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new UnityEngine.Vector3(0, 0, orient);

        unsafe
        {
            int n = OpenCVInterop.Detect(backCam.GetPixels32(), backCam.width, backCam.height);
            Debug.Log(n);
        }
    }
    
}
// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
    [DllImport("faceDetection")]

    internal unsafe static extern int Detect(Color32[] img,int width,int height);
}
