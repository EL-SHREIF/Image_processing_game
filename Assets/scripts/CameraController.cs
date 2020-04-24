using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using System.Data;

public class CameraController : MonoBehaviour
{
    private bool camAvl;
    public WebCamTexture backCam;
    private Texture d_backGround;



    public RawImage background;
    public AspectRatioFitter fit;

    private bool go_back_check = false;
    int camidx = 0;
    string nick_name = "";
    private void Start() {

        nick_name = PlayerPrefs.GetString("player_name");
        Debug.Log(nick_name);


        d_backGround = background.texture;

        if (WebCamTexture.devices.Length == 0) {
            Debug.Log("No devices");
            camAvl = false;
            return;
        }

        backCam = new WebCamTexture(WebCamTexture.devices[camidx].name, Screen.width, Screen.height);

        if (backCam == null) {
            Debug.Log("unable bardo");
            return;
        }

        backCam.Play();
        background.texture = backCam;

        camAvl = true;
    }

    private void Update() {

        if (go_back_check) {
            return;
        }

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
            var rawImage = backCam.GetPixels32();
            float n = OpenCVInterop.DetectRed(ref rawImage, backCam.width, backCam.height);
            Debug.Log(n);
        }
    }


    public void go_back()
    {
        go_back_check = true;
        backCam.Stop();
        backCam = null;
        background.texture = null;
        SceneManager.LoadScene(0,LoadSceneMode.Single);
    }

    public void swap_camera_clicked() {
        if (WebCamTexture.devices.Length > 0) {
            int old_idx = camidx;
            camidx++;
            camidx = camidx % WebCamTexture.devices.Length;
            if (old_idx != camidx)
            {
                d_backGround = background.texture;
                backCam = new WebCamTexture(WebCamTexture.devices[camidx].name, Screen.width, Screen.height);
                backCam.Play();
                background.texture = backCam;
                camAvl = true;
            }
        }
        if (backCam == null)
        {
            Debug.Log("unable bardo");
            return;
        }

        
    }

}
// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
    [DllImport("faceDetection")]

    internal unsafe static extern float DetectRed(ref Color32[] rawImage, int width,int height);
}
