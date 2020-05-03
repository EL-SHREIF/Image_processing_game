//shiko level(2)
using Barracuda;
using TFClassify;
using System.Linq;
using System.Threading.Tasks;
//============================================

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

    //Level 2 Variables======================================================
    private float cameraScale = 1f;
    private float shiftX = 0f;
    private float shiftY = 0f;
    private float scaleFactor = 1;

    private static Texture2D boxOutlineTexture;
    private static GUIStyle labelStyle;
   
    public Classifier classifier;
    private bool isWorking = false;
    public Text uiText;
    int level;
    int obj_num = 1;
    string[] array_of_objects= new string[3];

    public GameObject AC;
    public GameObject WA;
    public GameObject TLE;

    int acc_num = 0;
    const int num_of_options = 5;
    string curr_obj = "";
    string[] array_of_avilable_options= new string[num_of_options] { "baseball","mouse, computer mouse","remote control, remote","electric fan, blower","analog clock"};
    int random2 = 1;
    //=======================================================================

    //GUI data to view and hide==============================================
    public GameObject level_num;
    public GameObject time_left;
    public GameObject player_name;
    public GameObject player_score;
    public GameObject un_imp;
    public GameObject un_imp2; 
    public AspectRatioFitter fitter;

    public GameObject task_panel;
    public GameObject task_msg;
    public GameObject ready_buttom;
    public GameObject Take_picture_buttom;
    //=======================================================================

    public bool takeway = true;
    public int timer = 30;
    string name = "";

    int random = 1;
    Mat draw = new Mat();
    Point last = new Point();

    private void Start()
    {
        name = PlayerPrefs.GetString("player_name");
        d_backGround = background.texture;

        random = Random.Range(0, 20000) % 3;
        random2 = Random.Range(0, 20000) % num_of_options;
        backCam = new WebCamTexture(WebCamTexture.devices[camidx].name, Screen.width, Screen.height);
        background.texture = backCam;
        backCam.Play();
        
        camAvl = true;

        boxOutlineTexture = new Texture2D(1, 1);
        boxOutlineTexture.SetPixel(0, 0, Color.red);
        boxOutlineTexture.Apply();

        labelStyle = new GUIStyle();
        labelStyle.fontSize = 50;
        labelStyle.normal.textColor = Color.red;

        level_num.SetActive(false);
        player_name.SetActive(false);
        time_left.SetActive(false);
        un_imp.SetActive(false);
        un_imp2.SetActive(false);
        AC.SetActive(false);
        WA.SetActive(false);
        TLE.SetActive(false);
        player_score.SetActive(false);
        //task_panel.SetActive(false);
        Take_picture_buttom.SetActive(false);
        task_msg.GetComponent<Text>().text = "First level is searching for Colors Are you Ready??";
        task_msg.GetComponent<Text>().fontSize = 70;
        draw = new OpenCvSharp.Mat(backCam.height, backCam.width, MatType.CV_8UC3, new Scalar(0, 0, 0));

        level = 1;
        CalculateShift(Classifier.IMAGE_SIZE);
    }

    private void Update()
    {

        if (go_back_check)
        {
            return;
        }

        if (!camAvl)
            return;

        float ratio = (float)backCam.width / (float)backCam.height;
        fitter.aspectRatio = ratio;

        float scaleX = cameraScale;
        float scaleY = backCam.videoVerticallyMirrored ? -cameraScale : cameraScale;
        background.rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

        if (orient != 0)
        {
            this.cameraScale = (float)Screen.width / Screen.height;
        }

        if (takeway == false && timer >= 1)
        {
            time_left.GetComponent<Text>().text = timer.ToString();
            StartCoroutine(Countdown());
        }

        //remove this when you finish development 
        if (level == 2) {
            TFClassify();
        }
        else if(level == 3)
        {
            var pixels = backCam.GetPixels32();
            var updatedTexture = new Texture2D(backCam.height, backCam.width);
            updatedTexture = OpenCvSharp.Unity.MatToTexture(draw);
            updatedTexture.Apply();
            background.texture = updatedTexture;
            DrawScreen();
        }
        //========================================
    }


    public void go_back()
    {
        go_back_check = true;
        backCam.Stop();
        backCam = null;
        background.texture = null;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void swap_camera_clicked()
    {
        if (WebCamTexture.devices.Length > 0)
        {
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


    }

    private IEnumerator Countdown()
    {
        takeway = true;
        yield return new WaitForSeconds(1);
        if (timer > 0)
            timer--;
        time_left.GetComponent<Text>().text = timer.ToString();
        takeway = false;
    }

    private IEnumerator showAns(bool ans)
    {
        if (ans)
        {
            acc_num++;
            AC.SetActive(true);
            yield return new WaitForSeconds(2);
            AC.SetActive(false);
        }
        else
        {
            WA.SetActive(true);
            yield return new WaitForSeconds(2);
            WA.SetActive(false);
        }
    }

    private IEnumerator showTLE()
    {
        TLE.SetActive(true);
        yield return new WaitForSeconds(2);
        TLE.SetActive(false);
    }

    public void Ready_to_go()
    {
        string s = level_num.GetComponent<Text>().text;
        if (s == "1")
        {
            timer = 30;
            task_msg.GetComponent<Text>().text = get_task_of_first_level();
            task_msg.GetComponent<Text>().fontSize = 100;
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
        else if (s == "2") {
            timer = 100;
            task_msg.GetComponent<Text>().text = get_task_of_second_level();
            task_msg.GetComponent<Text>().fontSize = 80;
            ready_buttom.SetActive(false);
            task_panel.SetActive(true);
            level_num.SetActive(true);
            player_name.SetActive(true);
            time_left.SetActive(true);
            un_imp.SetActive(true);
            un_imp2.SetActive(true);
            player_score.SetActive(true);
            Take_picture_buttom.SetActive(true);
            takeway = false;
        }

    }

    public void Capture_image()
    {
        string s = level_num.GetComponent<Text>().text;
        if (s == "1")
        {
            float n = evaluate_level_one();
            curr_score = n;
            if (timer > 0)
            {
                player_score.GetComponent<Text>().text = n.ToString();
            }
            else {
                StartCoroutine(showTLE());
            }
            Take_picture_buttom.SetActive(false);
            ready_buttom.SetActive(true);
            level_num.GetComponent<Text>().text = "2";
            timer = 0;
            takeway = true;
            time_left.GetComponent<Text>().text = "00";
            level = 2;
            task_msg.GetComponent<Text>().text = "Second level is Finding 5 Objects Are you still Ready :''D?? take care it become harder as long as you go";
            task_msg.GetComponent<Text>().fontSize = 60;
        }
        else if (s == "2") {
            if (obj_num <= 5)
            {
                obj_num++;
                if (timer > 0)
                {
                    bool ans = evaluate_level_two();
                    StartCoroutine(showAns(ans));
                }
                else {
                    StartCoroutine(showTLE());
                }
                timer = 0;
                takeway = true;
                time_left.GetComponent<Text>().text = "00";
                task_msg.GetComponent<Text>().text = get_task_of_second_level();
                timer = 100;
                takeway = false;
            }
            else {
                float tmp = acc_num / 5 * 20;
                curr_score = curr_score+tmp;
                player_score.GetComponent<Text>().text = curr_score.ToString();
                Take_picture_buttom.SetActive(false);
                ready_buttom.SetActive(true);
                level_num.GetComponent<Text>().text = "3";
                timer = 0;
                takeway = true;
                time_left.GetComponent<Text>().text = "00";
                level = 3;
                task_msg.GetComponent<Text>().text = "Third level fe albatee5 a8reflak :''D ??";
            }
        }

    }

    public string get_task_of_first_level()
    {
        List<string> names = new List<string>();
        names.Add("Red"); names.Add("Green"); names.Add("White");
        return "Search for " + names[random] + " color please!!";
    }
    public string get_task_of_second_level()
    {
        random2++;
        random2 = random2 % num_of_options;
        curr_obj = array_of_avilable_options[random2];
        return "Search for " + array_of_avilable_options[random2] + " please :) ";
       
    }
    public float evaluate_level_one()
    {
        var rawImage = backCam.GetPixels32();
        float n = 0;
        if (random == 0) n = DetectRed();
        else if (random == 1) n = DetectGreen();
        else n = DetectWhite();
        Debug.Log(n);
        return n;
    }

    public bool evaluate_level_two() {
        int num_of_frames = 4;
        for (int k = 0; k < num_of_frames; k++) {
            TFClassify();
            for (int i = 0; i < 3; i++)
            {
                if (curr_obj == array_of_objects[i])
                    return true;
            }
        }
        return false;
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

    // the current and best approach for drawing on the screen. The approach detect the red color on the screen and make 
    // a contour around the red object then draw the min enclosing circle to this contour and draw with this center of
    // the enclosing circle. The drawing approach is get the current center of the contour and the previous center
    // and draw a line between them.
    public void DrawScreen()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);
        Cv2.Flip(img, img, FlipMode.Y);
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2BGR);
        Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.BGR2HSV);
        int elementSize = 5;
        Mat element = new OpenCvSharp.Mat(elementSize, elementSize, MatType.CV_8UC1, 1);
        Mat mask = new Mat();
        Cv2.InRange(hsv2, new Scalar(100, 120, 70), new Scalar(150, 255, 255), mask);
        Cv2.Erode(mask, mask, element, null, 2);
        Cv2.MorphologyEx(mask, mask, MorphTypes.Open, element);
        Cv2.Dilate(mask, mask, element, null, 1);
        Mat res = new Mat();
        Cv2.BitwiseAnd(img, img, res, mask);
        Point[][] contors = new Point[1][];
        HierarchyIndex[] heirarchy = new HierarchyIndex[1];
        Cv2.FindContours(mask, out contors, out heirarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        //Cv2.ImShow("contor", mask);

        Point[] mx = new Point[1];
        double cntArea = 0;
        if (contors.Length > 0)
        {
            for (int i = 0; i < contors.Length; i++)
            {
                if (cntArea <= Cv2.ContourArea(contors[i]))
                {
                    mx = contors[i];
                    cntArea = Cv2.ContourArea(contors[i]);
                }
            }
            Point2f center = new Point2f();
            float radius = 0;
            Cv2.MinEnclosingCircle(mx, out center, out radius);
            if (last.X != 0 && last.Y != 0) Cv2.Line(draw, center, last, new Scalar(255, 0, 0), 4, LineTypes.Filled);
            last = center;
        }
    }

    // Old approach function for drawing on the screen but it was not so smooth in drawing. The approach was detecting 
    // a red color object on the screen and make a circle around it and draw with the center of this circle but 
    // the sircle wasn't stable aroun the object so was drawing in zigzag a little.
    public bool DrawScreenOld1()
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
        Cv2.Erode(mask1, mask1, element, null, 4);

        //Cv2.ImShow("Black mask", mask1);


        Mat gray = new OpenCvSharp.Mat();
        Cv2.CvtColor(img, gray, OpenCvSharp.ColorConversionCodes.RGBA2GRAY);
        //Cv2.ImShow("gray mask", gray);
        CircleSegment[] circles = null;
        circles = Cv2.HoughCircles(mask1, OpenCvSharp.HoughMethods.Gradient, 10,
            100,  // change this value to detect circles with different distances to each other
            100, 30, 1, 40 // change the last two parameters
                           // (min_radius & max_radius) to detect larger circles
        );

        for (int i = 0; i < circles.Length; i++)
        {
            CircleSegment c = circles[i];
            Point center = c.Center;
            if (last.X != 0 && last.Y != 0) Cv2.Line(draw, center, last, new Scalar(255, 0, 0), 4, LineTypes.Filled);
            last = center;

            // circle center
            Cv2.Circle(img, center.X, center.Y, 1, new Scalar(0, 100, 100), 3, OpenCvSharp.LineTypes.AntiAlias);
            // circle outline
            float radius = c.Radius;
            Cv2.Circle(img, center.X, center.Y, (int)radius, new Scalar(255, 0, 255), 3, OpenCvSharp.LineTypes.AntiAlias);
        }
        //Cv2.ImShow("detected circles", img);
        return true;
    }
    // Old approach function for drawing on the screen but it was not so smooth in drawing. The approach was to detect
    // a certain color in a certain point we tell the user about on the screen the color of the object the user have put
    // in this part of the picture then be tracked and draw with it, by getting the new position and the last position
    // and draw a line between them.
    public void DrawScreenOld2()
    {
        int downCnt = 30;
        Vec3b centerPoint = new Vec3b();
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);
        Cv2.Flip(img, img, FlipMode.Y);
        Cv2.MedianBlur(img, img, 21);
        Mat hsv1 = new Mat(), mask1 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2RGB);
        //Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.RGB2HSV);
        if (downCnt > 0)
        {
            Cv2.Circle(img, 50, 50, 5, new Scalar(0, 100, 100), 3, OpenCvSharp.LineTypes.AntiAlias);
            Cv2.ImShow("blank2", img);
            centerPoint = hsv1.Get<Vec3b>(50, 50);
            //Debug.Log(centerPoint);
            return;
        }

        //Debug.Log(centerPoint.ToString());

        Cv2.InRange(hsv1, new Scalar(Mathf.Max(0, centerPoint[0] - 5), Mathf.Max(centerPoint[1] - 5, 0), Mathf.Max(centerPoint[2] - 5, 0)), new Scalar(Mathf.Min(254, centerPoint[0] + 5), Mathf.Min(centerPoint[1] + 5, 254), Mathf.Min(centerPoint[2] + 5, 254)), mask1);

        Point center = new Point();
        int elementSize = 2;
        Mat element = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new Size(elementSize+1, elementSize+1), new Point(elementSize, elementSize));
        Cv2.Erode(mask1, mask1, element, null, 1);
        Mat res = mask1.FindNonZero();
        if (res.Rows == 0 || res.Cols == 0) return;
        center.X = res.At<Point>(0).X;
        center.Y = res.At<Point>(0).Y;
        if (last.X != 0 && last.Y != 0) Cv2.Line(draw, center, last, new Scalar(255, 0, 0), 10, LineTypes.Filled);
        last = center;
        return;
        
    }

    public int DetectCircle()
    {
        CircleSegment[] circles = null;
        int score = 0;
        Mat mask = new Mat();
        Cv2.InRange(draw, new Scalar(10, 0, 0), new Scalar(255, 255, 255), mask);
        //Cv2.ImShow("mask", mask);
        for (int i = 1; i <= 10; i++)
        {
            circles = Cv2.HoughCircles(mask, OpenCvSharp.HoughMethods.Gradient, 1,
            10,  // change this value to detect circles with different distances to each other
            100, 30, 1, 0 // change the last two parameters
                          // (min_radius & max_radius) to detect larger circles
            );
            if (circles.Length > 0) return 10 - i + 1;

        }
        return score;
    }


    //========================================================================================================
    //========================================================================================================
    //========================================================================================================
    //Level 3 Functions  by Shiko

    private void CalculateShift(int inputSize)
    {
        int smallest;

        if (Screen.width < Screen.height)
        {
            smallest = Screen.width;
            this.shiftY = (Screen.height - smallest) / 2f;
        }
        else
        {
            smallest = Screen.height;
            this.shiftX = (Screen.width - smallest) / 2f;
        }

        this.scaleFactor = smallest / (float)inputSize;
    }


    private void TFClassify()
    {
        if (this.isWorking)
        {
            return;
        }

        this.isWorking = true;
        StartCoroutine(ProcessImage(Classifier.IMAGE_SIZE, result =>
        {
            StartCoroutine(this.classifier.Classify(result, probabilities =>
            {
                this.uiText.text = string.Empty;
               

                if (probabilities.Any())
                {
                    for (int i = 0; i < 3; i++)
                    {
                        this.uiText.text += probabilities[i].Key + ": " + string.Format("{0:0.000}%", probabilities[i].Value) + "\n";
                        array_of_objects[i] = probabilities[i].Key;
                    }
                }

                Resources.UnloadUnusedAssets();
                this.isWorking = false;
            }));
        }));
    }

    private IEnumerator ProcessImage(int inputSize, System.Action<Color32[]> callback)
    {
        yield return StartCoroutine(TextureTools.CropSquare(backCam,
            TextureTools.RectOptions.Center, snap =>
            {
                var scaled = Scale(snap, inputSize);
                var rotated = Rotate(scaled.GetPixels32(), scaled.width, scaled.height);
                callback(rotated);
            }));
    }
    private Texture2D Scale(Texture2D texture, int imageSize)
    {
        var scaled = TextureTools.scaled(texture, imageSize, imageSize, FilterMode.Bilinear);

        return scaled;
    }


    private Color32[] Rotate(Color32[] pixels, int width, int height)
    {
        return TextureTools.RotateImageMatrix(
                pixels, width, height, -90);
    }
    //========================================================================================================
    //========================================================================================================
    //========================================================================================================
}


