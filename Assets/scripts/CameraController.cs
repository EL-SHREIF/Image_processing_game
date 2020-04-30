using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using System.Data;
using System.Text.RegularExpressions;
using OpenCvSharp;
using Unity;
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
        //DetectCircle();
        
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
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2RGB);
        //Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.BGR2HSV);
        Mat mask1 = new Mat();
        Cv2.InRange(hsv1, new Scalar(220, 220, 0), new Scalar(255, 255, 255), mask1);
        Cv2.ImShow("Black mask", mask1);

        Mat gray = new OpenCvSharp.Mat();
        Cv2.CvtColor(img, gray, OpenCvSharp.ColorConversionCodes.RGBA2GRAY);
        //Cv2.ImShow("gray mask", gray);
        CircleSegment[] circles = null;
        circles = Cv2.HoughCircles(gray,OpenCvSharp.HoughMethods.Gradient, 1,
            img.Rows/16,  // change this value to detect circles with different distances to each other
            100, 30, 1, 40 // change the last two parameters
                           // (min_radius & max_radius) to detect larger circles
        );
       
        for (int i = 0; i < circles.Length; i++)
        {
            CircleSegment c = circles[i];
            Point center = c.Center;
            // circle center
            Cv2.Circle(img, center.X,center.Y, 1,new Scalar(0, 100, 100), 3, OpenCvSharp.LineTypes.AntiAlias);
            // circle outline
            float radius = c.Radius;
            Cv2.Circle(img, center.X,center.Y, (int)radius,new Scalar(255, 0, 255), 3, OpenCvSharp.LineTypes.AntiAlias);
        }
        //Cv2.ImShow("detected circles", img);
        return true;
    }
    public void DetectCircle2()
    {
        int minH = 130, maxH = 160, minS = 10, maxS = 40, minV = 75, maxV = 130;

        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2BGR);
        Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.BGR2HSV);
        Cv2.InRange(hsv2, new Scalar(minH, minS, minV), new Scalar(maxH, maxS, maxV), hsv2);
        // Pre processing
        int blurSize = 5;
        int elementSize = 5;
        Cv2.MedianBlur(hsv2, hsv2, blurSize);
        Mat element = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse,new Size(2 * elementSize + 1, 2 * elementSize + 1), new Point(elementSize, elementSize));
        Cv2.Dilate(hsv2, hsv2, element);
        // Contour detection
             
        Point[][] contours = new Point[100][];
        HierarchyIndex[] hierarchy = new HierarchyIndex[100];
        Cv2.FindContours(hsv2,out contours,out hierarchy,OpenCvSharp.RetrievalModes.External,OpenCvSharp.ContourApproximationModes.ApproxSimple,new Point(0, 0));
        int largestContour = 0;
        for (int i = 1; i < contours.Length; i++)
        {
            if (Cv2.ContourArea(contours[i]) > Cv2.ContourArea(contours[largestContour]))
                largestContour = i;
        }
        Cv2.DrawContours(img, contours, largestContour, new Scalar(0, 0, 255), 1);
        // Convex hull
        if (!(contours.Length==0))
        {
            Point[][] hull = new Point[1][];
            hull[0] = Cv2.ConvexHull(contours[largestContour]);
            Cv2.DrawContours(img, hull, 0, new Scalar(0, 255, 0), 3);
            if (hull[0].Length > 2)
            {
                Point[] hullIndexes = null;
                hullIndexes = Cv2.ConvexHull(contours[largestContour], true);
                Mat convexityDefectsV = new Mat();
                Mat inp = new OpenCvSharp.MatOfPoint(img.Rows,img.Cols,contours[largestContour]);
                Mat inp2 = new OpenCvSharp.MatOfPoint(img.Rows, img.Cols, hullIndexes);
                Cv2.ConvexityDefects(inp, inp2, convexityDefectsV);
                for (int i = 0; i < convexityDefectsV.Rows; i++)
                {
                    Point p1 = contours[largestContour][convexityDefectsV.At<Vec3b>(i)[0]];
                    Point p2 = contours[largestContour][convexityDefectsV.At<Vec3b>(i)[1]];
                    Point p3 = contours[largestContour][convexityDefectsV.At<Vec3b>(i)[2]];
                    Cv2.Line(img, p1, p3, new Scalar(255, 0, 0), 2);
                    Cv2.Line(img, p3, p2, new Scalar(255, 0, 0), 2);
                }
            }
        }
        Cv2.ImShow("output", img);
        
    }
}

