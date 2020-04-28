using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using System.Data;
using System.Text.RegularExpressions;

public class CameraController : MonoBehaviour
{
    private bool camAvl;
    public WebCamTexture backCam;
    private Texture d_backGround;

    public RawImage background;
    public AspectRatioFitter fit;

    float curr_score = 0;

    private bool go_back_check = false;
    int camidx = 0;
    string nick_name = "";

    public GameObject level_num;
    public GameObject time_left;
    public GameObject player_name;
    public GameObject player_score;
    public GameObject un_imp;
    public GameObject un_imp2;

    public GameObject task_panel;
    public GameObject task_msg;
    public GameObject ready_buttom;
    public GameObject Take_picture_buttom;

    public bool takeway = true;
    public int timer = 30;
    string name = "";

    int random=1;
    private void Start() {

        name=PlayerPrefs.GetString("player_name");
        d_backGround = background.texture;

        random = Random.Range(0,20000)%3;

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

        level_num.SetActive(false);
        player_name.SetActive(false);
        time_left.SetActive(false);
        un_imp.SetActive(false);
        un_imp2.SetActive(false);
        player_score.SetActive(false);
        task_panel.SetActive(false);
        Take_picture_buttom.SetActive(false);
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

        if (takeway == false && timer > 1)
        {
            time_left.GetComponent<Text>().text = timer.ToString();
            StartCoroutine(Countdown());
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


    private IEnumerator Countdown()
    {
        takeway = true;
        yield return new WaitForSeconds(1);
        if(timer>0)
            timer--;
        time_left.GetComponent<Text>().text = timer.ToString();
        takeway = false;
    }


    public void Ready_to_go()
    {
        string s = level_num.GetComponent<Text>().text;
        if (s == "1") {
            timer = 30;
            task_msg.GetComponent<Text>().text = get_task_of_first_level();
            ready_buttom.SetActive(false);
            task_panel.SetActive(true);
            level_num.SetActive(true);
            player_name.SetActive(true);
            time_left.SetActive(true);
            un_imp.SetActive(true);
            un_imp2.SetActive(true);
            player_score.SetActive(true);
            Take_picture_buttom.SetActive(true);
            player_name.GetComponent<Text>().text = name;
            takeway = false;
        }

    }

    public void Capture_image() {
        string s = level_num.GetComponent<Text>().text;
        if (s == "1")
        {
            float n=evaluate_level_one();
            task_panel.SetActive(false);
            curr_score = n;
            if (timer > 0)
            {
                player_score.GetComponent<Text>().text = n.ToString();
            }  
            Take_picture_buttom.SetActive(false);
            ready_buttom.SetActive(true);
            level_num.GetComponent<Text>().text = "2";
            timer = 0;
            takeway = true;
            time_left.GetComponent<Text>().text = "0";

        }
        

    }

    public string get_task_of_first_level() {
        List<string> names = new List<string>();
        names.Add("Red"); names.Add("Green"); names.Add("Blue");
        return "Search for "+ names[random] +" color please yazmili";    
    }
    public float evaluate_level_one()
    {
        unsafe
        {
            var rawImage = backCam.GetPixels32();
            float n= 0;
            if(random==0)n = OpenCVInterop.DetectRed(ref rawImage, backCam.width, backCam.height);
            else if(random==1) n = OpenCVInterop.DetectGreen(ref rawImage, backCam.width, backCam.height);
            else n = OpenCVInterop.DetectBlue(ref rawImage, backCam.width, backCam.height);
            Debug.Log(n);
           
            return n;
        }
    }
}
// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
    [DllImport("faceDetection")]
    internal unsafe static extern float DetectRed(ref Color32[] rawImage, int width,int height);
    [DllImport("faceDetection")]
    internal unsafe static extern float DetectBlue(ref Color32[] rawImage, int width, int height);
    [DllImport("faceDetection")]
    internal unsafe static extern float DetectGreen(ref Color32[] rawImage, int width, int height);
    [DllImport("faceDetection")]
    internal unsafe static extern bool DetectCircle(ref Color32[] rawImage, int width, int height);
}
