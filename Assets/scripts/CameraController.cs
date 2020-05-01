using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.ML;
using OpenCvSharp.Tracking;

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
    Mat draw = new Mat();
    Point last = new Point();

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
        draw = new OpenCvSharp.Mat(backCam.height, backCam.width, MatType.CV_8UC3,new Scalar(0,0,0));
    }

    private void Update() {

        if (go_back_check) {
            return;
        }

        if (!camAvl)
            return;

        float ratio =  (float)backCam.height / (float)backCam.width;
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
        DetectCircle();
        
        Cv2.ImShow("blank", draw);
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
        names.Add("Red"); names.Add("Green"); names.Add("White");
        return "Search for "+ names[random] +" color please yazmili";    
    }
    public float evaluate_level_one()
    {
       
        var rawImage = backCam.GetPixels32();
        float n= 0;
        if(random==0)n = DetectRed();
        else if(random==1) n = DetectGreen();
        else n = DetectWhite();
        Debug.Log(n);
           
        return n;
        
    }
    public float DetectRed()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2BGR);
        Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.BGR2HSV);
        Mat mask1 = new Mat(), mask2 = new Mat();
        Cv2.InRange(hsv2, new Scalar(0, 120, 70), new Scalar(180, 255, 255), mask1);
        Cv2.InRange(hsv2, new Scalar(160, 120, 70), new Scalar(180, 255, 255), mask2);

        int redPixels = 0;
        for (int i = 0; i < mask1.Rows; i++)
        {
            for (int j = 0; j < mask1.Cols; j++) if (mask1.At<Vec3b>(i, j)[0] != 0 && mask1.At<Vec3b>(i, j)[1] != 0 && mask1.At<Vec3b>(i, j)[2] != 0) redPixels++;
        }
        float score = (float)redPixels / (backCam.width * backCam.height);
        score = score * 10;
        return score;
    }
    public float DetectWhite()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2RGB);
        Mat mask1 = new Mat();
        Cv2.InRange(hsv1, new Scalar(200, 200, 0), new Scalar(255, 255, 255), mask1);
        int redPixels = 0;
        for (int i = 0; i < mask1.Rows; i++)
        {
            for (int j = 0; j < mask1.Cols; j++) if (mask1.At<Vec3b>(i, j)[0] != 0 || mask1.At<Vec3b>(i, j)[1] != 0 || mask1.At<Vec3b>(i, j)[2] != 0) redPixels++;
        }
        float score = (float)redPixels / (backCam.width * backCam.height);
        score = score * 10;
        return score;
    }
    public float DetectGreen()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2BGR);
        Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.BGR2HSV);
        Mat mask1 = new Mat();
        Cv2.InRange(hsv2, new Scalar(36, 0, 0), new Scalar(86, 255, 255), mask1);
        int redPixels = 0;
        for (int i = 0; i < mask1.Rows; i++)
        {
            for (int j = 0; j < mask1.Cols; j++) if (mask1.At<Vec3b>(i, j)[0] != 0 && mask1.At<Vec3b>(i, j)[1] != 0 && mask1.At<Vec3b>(i, j)[2] != 0) redPixels++;
        }
        float score = (float)redPixels / (backCam.width * backCam.height);
        score = score * 10;
        return score;
    }
    public bool DetectCircle()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);
        Cv2.Flip(img, img, FlipMode.Y);
        Cv2.MedianBlur(img, img, 51);
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2RGB);
        Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.RGB2HSV);
        Mat mask1 = new Mat();
        Cv2.InRange(hsv2, new Scalar(100, 120, 70), new Scalar(150, 255, 255), mask1);
        
        int elementSize = 5;
        Mat element = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new Size(2 * elementSize + 1, 2 * elementSize + 1), new Point(elementSize, elementSize));
        Cv2.Erode(mask1, mask1, element,null, 4);

        Cv2.ImShow("Black mask", mask1);

        
        Mat gray = new OpenCvSharp.Mat();
        Cv2.CvtColor(img, gray, OpenCvSharp.ColorConversionCodes.RGBA2GRAY);
        //Cv2.ImShow("gray mask", gray);
        CircleSegment[] circles = null;
        circles = Cv2.HoughCircles(mask1,OpenCvSharp.HoughMethods.Gradient, 10,
            100,  // change this value to detect circles with different distances to each other
            100, 30, 1, 40 // change the last two parameters
                           // (min_radius & max_radius) to detect larger circles
        );
        
        for (int i = 0; i < circles.Length; i++)
        {
            CircleSegment c = circles[i];
            Point center = c.Center;
            fill(center.X, center.Y);
            if(last.X!=0&&last.Y!=0)Cv2.Line(draw, center, last, new Scalar(255, 0, 0), 4, LineTypes.Filled);
            last = center;
            
            // circle center
            Cv2.Circle(img, center.X,center.Y, 1,new Scalar(0, 100, 100), 3, OpenCvSharp.LineTypes.AntiAlias);
            // circle outline
            float radius = c.Radius;
            Cv2.Circle(img, center.X,center.Y, (int)radius,new Scalar(255, 0, 255), 3, OpenCvSharp.LineTypes.AntiAlias);
        }
        Cv2.ImShow("detected circles", img);
        return true;
    }
    public void fill(int x,int y)
    {
        draw.Set<Scalar>(y, x, new Scalar(255, 0, 0));
        for(int i = 0; i < 4; i++)
        {
            draw.Set<Scalar>(y - i, x - i, new Scalar(255, 0, 0));
            draw.Set<Scalar>(y + i, x + i, new Scalar(255, 0, 0));
            draw.Set<Scalar>(y - i, x + i, new Scalar(255, 0, 0));
            draw.Set<Scalar>(y + i, x - i, new Scalar(255, 0, 0));
            draw.Set<Scalar>(y, x - i, new Scalar(255, 0, 0));
            draw.Set<Scalar>(y, x + i, new Scalar(255, 0, 0));
            draw.Set<Scalar>(y + i, x, new Scalar(255, 0, 0));
            draw.Set<Scalar>(y - i, x, new Scalar(255, 0, 0));
        }
        
    }

    

}


