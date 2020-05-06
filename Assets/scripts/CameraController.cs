using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// including libraries for openCV in C#
using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.ML;
using OpenCvSharp.Tracking;

// include the libraries that will be used in Object Detection Part in level 2
using Barracuda;
using TFClassify;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

public class CameraController : MonoBehaviour
{
    /*
        In this class we have everything that connect from the backend to the frontend    
    */
    
    //UI part variables==========================================================
    //each one of this variables connected to part in the frontend
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
    public GameObject AC;
    public GameObject WA;
    public GameObject TLE;
    //============================================================================


    //Camera Variables ===========================================================
    //This part to access mobile phone camera or web cam in laptop and open it
    private bool camAvl;
    int camidx = 0;                                     // as their is devices that has more than one camera
    public WebCamTexture backCam;
    private Texture d_backGround;
    public RawImage background;
    public AspectRatioFitter fit;
    //next variables to control sizes of the camera object
    private float cameraScale = 1f;
    private float shiftX = 0f;
    private float shiftY = 0f;
    private float scaleFactor = 1;
    //============================================================================


    //Game Logic Variables =======================================================
    //This part of variables is for the game backend logic that put rules
    float curr_score = 0;                                //The score of the player
    int level;                                           //The current level we are in 
    string nick_name = "";                               //The nick name of the player
    // The next 2 variables is to control the Timer
    public bool takeway = true;                          //To restart the timer you should
    public int timer = 30;                               //set these 2 variables
    // The next 2 variables to randomize the adventure of playing each time
    int random = 1;
    int random2 = 1;
    //the next variable to control moving between differnt Scense
    private bool go_back_check = false;
    //============================================================================


    //Level two special Variables=================================================
    private static Texture2D boxOutlineTexture;
    private static GUIStyle labelStyle;
    public Classifier classifier;                          // instance of the classifier that we use
    private bool isWorking = false;        
    int obj_num = 1;                                       // This to make level 2 consist of 5 parts
    int acc_num = 0;                                       // This to calculate number of times player get accepted image
    // The next array to save predictions from the Classifier
    string[] array_of_objects= new string[3];
    //The next 3 variables is for the object that we can detect with our model
    string curr_obj = "";                                  // which object we want to search for
    const int num_of_options = 18;
    string[] array_of_avilable_options= new string[num_of_options] { "monitor", "modem",
          "kimono", "cleaver, meat cleaver, chopper", "sunglasses, dark glasses,shades",
          "prayer rug, prayer mat" , "joystick" , "perfume, essence" , "cucumber, cuke",
          "iPod" , "orange" , "folding chair" , "computer keyboard, keypad" ,"baseball",
          "mouse, computer mouse","remote control, remote","electric fan, blower","analog clock"};
    // The next variable is just for debgging the results
    public Text uiText;
    //============================================================================

    // Draw is a mask for the drawn picture on the screen. last is a point to help draw on the screen,
    // used to capture the last point to draw a line between it and the current point.
    Mat draw = new Mat();
    Point last = new Point();
    //============================================================================

    private void Start()
    {
        // This function is the initial function that called when you make new inistance
        // It's like a constructor in c++ code

        nick_name = PlayerPrefs.GetString("player_name");  //get the name from last Scene
        level = 1;                                         //make player start from level 1

        // Select Random Variables for the player new Adventure
        random = Random.Range(0, 20000) % 3;
        random2 = Random.Range(0, 20000) % num_of_options;
        //===============================================================

        // Open the Camera in the player Devices
        d_backGround = background.texture;
        backCam = new WebCamTexture(WebCamTexture.devices[camidx].name, Screen.width, Screen.height);
        background.texture = backCam;
        backCam.Play();
        camAvl = true;
        //===============================================================

        // Initialize the Variables that level 2 will need when its start
        boxOutlineTexture = new Texture2D(1, 1);
        boxOutlineTexture.SetPixel(0, 0, Color.red);
        boxOutlineTexture.Apply();
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 50;
        labelStyle.normal.textColor = Color.red;
        CalculateShift(Classifier.IMAGE_SIZE);
        //===============================================================


        Enable_Ready_and_Task_only("First level is searching for Colors Are you Ready??", 70);

        // initialize the draw mask to be the same of the screen and to have black pixels.
        draw = new OpenCvSharp.Mat(backCam.height, backCam.width, MatType.CV_8UC3, new Scalar(0, 0, 0));        
    }

    private void Update()
    {
        // This function called each new Frame automatically 
        
        if (go_back_check)    // Control moving between Scenes
            return;

        if (!camAvl)
            return;

        // Control How the Camera view presented in the Device Screen====================
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
        //=================================================================================

        //This is to call the function of the Timer to make the CountDown==================
        if (takeway == false && timer >= 1)
        {
            time_left.GetComponent<Text>().text = timer.ToString();
            StartCoroutine(Countdown());
        }
        //=================================================================================

        //To help in high number of frames detection
        if (level == 2) TFClassify();

        //Debuging Part====================================================================
        //remove this when you finish development please

        else if(level == 3)// added after you press ready button
        {
            // Take our draw mask at the current moment and convert it to texture and apply it to the background. 
            var updatedTexture = new Texture2D(backCam.height, backCam.width);
            updatedTexture = OpenCvSharp.Unity.MatToTexture(draw);
            updatedTexture.Apply();
            background.texture = updatedTexture;
            DrawScreen();
        }
        //================================================================================
    }

    public void go_back()
    {
        // This Function is to Control Back button in the UI 
        go_back_check = true;
        backCam.Stop();
        backCam = null;
        background.texture = null;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void swap_camera_clicked()
    {
        // This to change from front Cam to back cam
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
        // This function used for the timer
        takeway = true;
        yield return new WaitForSeconds(1);
        if (timer > 0)
            timer--;
        time_left.GetComponent<Text>().text = timer.ToString();
        takeway = false;
    }

    private IEnumerator showAns(bool ans)
    {
        //  This function to show the answer to the user is his image accepted or not
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
        // This to tell the user that you submit image after the valid time that you have
        TLE.SetActive(true);
        yield return new WaitForSeconds(2);
        TLE.SetActive(false);
    }

    private void CalculateShift(int inputSize)
    {
        // This function help in how we view the Camera input in the Device Screen
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

    public void Ready_to_go()
    {
        // The function that called automatic when you press on Ready button
        string s = level_num.GetComponent<Text>().text;
        if (s == "1")
        {
            player_name.GetComponent<Text>().text = nick_name;
            timer = 30;
            takeway = false;
            string str = get_task_of_first_level();
            Press_Ready_button_helper(str, 100);
        }
        else if (s == "2")
        {
            timer = 100;
            takeway = false;
            string str = get_task_of_second_level();
            Press_Ready_button_helper(str, 70);
        }
        else if (s == "3") { 
            
        }
    }

    private void Press_Ready_button_helper(string s, int font_sz) {
        // helper function to control the UI
        task_msg.GetComponent<Text>().text = s;
        task_msg.GetComponent<Text>().fontSize = font_sz;
        ready_buttom.SetActive(false);
        task_panel.SetActive(true);
        level_num.SetActive(true);
        player_name.SetActive(true);
        time_left.SetActive(true);
        un_imp.SetActive(true);
        un_imp2.SetActive(true);
        player_score.SetActive(true);
        Take_picture_buttom.SetActive(true);
    }
    public void Capture_image()
    {
        // This function is called when you capture new image
        string s = level_num.GetComponent<Text>().text;
        // to know which level
        if (s == "1")
        {
            //get the score
            float n = evaluate_level_one();
            curr_score = n;
            if (timer > 0)
                player_score.GetComponent<Text>().text = n.ToString();
            else 
                StartCoroutine(showTLE());
            // Update the UI for level 2
            level = 2;
            Take_picture_buttom.SetActive(false);
            ready_buttom.SetActive(true);
            level_num.GetComponent<Text>().text = "2";
            task_msg.GetComponent<Text>().text = "Second level is Finding 5 Objects Are you still Ready :''D?? take care it become harder as long as you go";
            task_msg.GetComponent<Text>().fontSize = 50;
            timer = 0;
            takeway = true;
            time_left.GetComponent<Text>().text = "00";
            //====================================================================
            
        }
        else if (s == "2") {
            // if level 2 you will need to loop 5 times
            if (obj_num < 5)
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
                if (timer > 0){
                    bool ans = evaluate_level_two();
                    StartCoroutine(showAns(ans));
                }
                else
                    StartCoroutine(showTLE());
                //calculate the score and set the UI and make it ready for level 3
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
                task_msg.GetComponent<Text>().text = "Bring Red object as in level 3 you will need to draw with it" +
                    "Take Care it's challenging part";
                task_msg.GetComponent<Text>().fontSize = 50;
                //======================================================================
            }
        }

    }
    
    public string get_task_of_first_level()
    {
        // get the random task we choose for level one and output it to the user.
        List<string> names = new List<string>();
        names.Add("Red"); names.Add("Green"); names.Add("White");
        return "Search for " + names[random] + " color please!!";
    }
    public string get_task_of_second_level()
    {
        // get the random task we choose for level 2 and output it to the user.
        random2++;
        random2 = random2 % num_of_options;
        curr_obj = array_of_avilable_options[random2];
        string s_in = curr_obj;
        string s_out = Get_the_name_of_object(s_in);
        return "Search for " + s_out + " please :)";
    }
 
    public float evaluate_level_one()
    {
        // Helper function to evaluate the score for level one by quering the associated function for the task we have
        // chosen randomly.
        var rawImage = backCam.GetPixels32();
        float n = 0;
        if (random == 0) n = DetectRed();
        else if (random == 1) n = DetectGreen();
        else n = DetectWhite();
        Debug.Log(n);
        return n;
    }

    public bool evaluate_level_two() {
        Debug.Log(curr_obj);
        // Helper function to help in the calculation of the score of level 2 by calling the classifier part
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
    // Helper function to detect the amount of red pixels in the captured picture and return a score for the picture.
    // The score is computed by getting the amount of red pixels in the whole picture and transform this amount to
    // a percent from 0 to 10.
    public float DetectRed()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);     // capture the picture in the camera.
        // Transform our picture from RGBA to HSV to better performance in color detection.
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2BGR);
        Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.BGR2HSV);
        // Make mask for the red color.
        Mat mask1 = new Mat();
        Cv2.InRange(hsv2, new Scalar(0, 120, 70), new Scalar(180, 255, 255), mask1);
        // Loop on all the total mask and get the number of red pixels captured in the mask.
        int redPixels = 0;
        for (int i = 0; i < mask1.Rows; i++)
        {
            for (int j = 0; j < mask1.Cols; j++) if (mask1.At<Vec3b>(i, j)[0] != 0 && mask1.At<Vec3b>(i, j)[1] != 0 && mask1.At<Vec3b>(i, j)[2] != 0) redPixels++;
        }
        // Compute the score out of 10 and return it.
        float score = (float)redPixels / (backCam.width * backCam.height);
        score = score * 10;
        return score;
    }
    public float DetectWhite()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam); // capture the picture in the camera.
         // Transform our picture from RGBA to RGB.
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2RGB);
        // Make mask for the white color.
        Mat mask1 = new Mat();
        Cv2.InRange(hsv1, new Scalar(200, 200, 0), new Scalar(255, 255, 255), mask1);
        // Loop on all the total mask and get the number of white pixels captured in the mask.
        int whitePixels = 0;
        for (int i = 0; i < mask1.Rows; i++)
        {
            for (int j = 0; j < mask1.Cols; j++) if (mask1.At<Vec3b>(i, j)[0] != 0 || mask1.At<Vec3b>(i, j)[1] != 0 || mask1.At<Vec3b>(i, j)[2] != 0) whitePixels++;
        }
        // Compute the score out of 10 and return it.
        float score = (float)whitePixels / (backCam.width * backCam.height);
        score = score * 10;
        return score;
    }
    public float DetectGreen()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam);  // capture the picture in the camera.
        // Transform our picture from RGBA to HSV to better performance in color detection.
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2BGR);
        Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.BGR2HSV);
        // Make mask for the green color.
        Mat mask1 = new Mat();
        Cv2.InRange(hsv2, new Scalar(36, 0, 0), new Scalar(86, 255, 255), mask1);
        // Loop on all the total mask and get the number of green pixels captured in the mask.
        int greenPixels = 0;
        for (int i = 0; i < mask1.Rows; i++)
        {
            for (int j = 0; j < mask1.Cols; j++) if (mask1.At<Vec3b>(i, j)[0] != 0 && mask1.At<Vec3b>(i, j)[1] != 0 && mask1.At<Vec3b>(i, j)[2] != 0) greenPixels++;
        }
        // Compute the score out of 10 and return it.
        float score = (float)greenPixels / (backCam.width * backCam.height);
        score = score * 10;
        return score;
    }

    // The current and best approach for drawing on the screen. The approach detect the red color on the screen and make 
    // a contour around the red object then draw the min enclosing circle to this contour and draw with this center of
    // the enclosing circle. The drawing approach is get the current center of the contour and the previous center
    // and draw a line between them.
    public void DrawScreen()
    {
        Mat img = OpenCvSharp.Unity.TextureToMat(backCam); // Capture the image from the camera.
        Cv2.Flip(img, img, FlipMode.Y);     // Flip the captured image.
        // CHange the captured image from RGBA to HSV.
        Mat hsv1 = new Mat(), hsv2 = new Mat();
        Cv2.CvtColor(img, hsv1, OpenCvSharp.ColorConversionCodes.RGBA2BGR);
        Cv2.CvtColor(hsv1, hsv2, OpenCvSharp.ColorConversionCodes.BGR2HSV);
        // Make a square structuring element with size of 5.
        int elementSize = 5;
        Mat element = new OpenCvSharp.Mat(elementSize, elementSize, MatType.CV_8UC1, 1);
        // Make a mask for red color on the screen.
        Mat mask = new Mat();
        Cv2.InRange(hsv2, new Scalar(100, 120, 70), new Scalar(150, 255, 255), mask);
        // Erode the mask with the square structuring element then apply opening on it then dilate it. This is to
        // enhance the captured red color on the screen and eliminate the noise.
        Cv2.Erode(mask, mask, element, null, 2);
        Cv2.MorphologyEx(mask, mask, MorphTypes.Open, element);
        Cv2.Dilate(mask, mask, element, null, 1);
        // Add the captured image to the mask.
        Mat res = new Mat();
        Cv2.BitwiseAnd(img, img, res, mask);
        // Detect the contour of the red color object.
        Point[][] contors = new Point[1][];
        HierarchyIndex[] heirarchy = new HierarchyIndex[1];
        Cv2.FindContours(mask, out contors, out heirarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        //Cv2.ImShow("contor", mask);

        // We assume our biggest contour is the red object in the screen. so we get the greatest contour. Then 
        // Get the minimum enclosing circle for the contour then draw with a line between the previous point 
        // captured and the current point. we draw with the center of the enclosing circle to the contour.
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

    // Helper function to detect if there is a circle drawn on the screen.
    public int DetectCircle()
    {
        CircleSegment[] circles = null;
        int score = 0;
        Mat mask = new Mat();
        Cv2.InRange(draw, new Scalar(10, 0, 0), new Scalar(255, 255, 255), mask);
        //Cv2.ImShow("mask", mask);
        
        circles = Cv2.HoughCircles(mask, OpenCvSharp.HoughMethods.Gradient, 1,
        10,  // change this value to detect circles with different distances to each other
        100, 30, 1, 0 // change the last two parameters
                        // (min_radius & max_radius) to detect larger circles
        );
        if (circles.Length > 0) return 10;

        
        return score;
    }

    private void TFClassify()
    {
        // This is the function that will take the frame and make it as a input to the Classifier
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
                //after geting the probabilities we need to access them
                if (probabilities.Any())
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //this part is for debugging
                        //this.uiText.text += probabilities[i].Key + ": " + string.Format("{0:0.000}%", probabilities[i].Value) + "\n";
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
        //Process the frame that the classifier will take to detect the objects
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
        //Helper function to get the best size of the image in detection
        //We mainly use this helper function from TextureTools library for c#
        var scaled = TextureTools.scaled(texture, imageSize, imageSize, FilterMode.Bilinear);
        return scaled;
    }

    private Color32[] Rotate(Color32[] pixels, int width, int height)
    {
        //We mainly use this helper function from TextureTools library for c#
        return TextureTools.RotateImageMatrix(
                pixels, width, height, -90);
    }

    private void Enable_Ready_and_Task_only(string s, int font_sz)
    {
        // This function to control what to view in the UI
        level_num.SetActive(false);
        player_name.SetActive(false);
        time_left.SetActive(false);
        un_imp.SetActive(false);
        un_imp2.SetActive(false);
        AC.SetActive(false);
        WA.SetActive(false);
        TLE.SetActive(false);
        player_score.SetActive(false);
        Take_picture_buttom.SetActive(false);
        task_msg.GetComponent<Text>().text = s;
        task_msg.GetComponent<Text>().fontSize = font_sz;
    }

    string Get_the_name_of_object(string s_in)
    {
        //Helper function to get better messages for the UI
        switch (s_in)
        {
            case "modem":
                return "Router";

            case "kimono":
                return "'bel3araby Kanaba'";

            case "cleaver, meat cleaver, chopper":
                return "Knife";

            case "sunglasses, dark glasses,shades":
                return "sunglasses";

            case "prayer rug, prayer mat":
                return "'bel3araby Segadet sala aw mos7af'";

            case "perfume, essence":
                return "perfume";

            case "cucumber, cuke":
                return "cucumber/5yaar";

            case "iPod":
                return "Mobile/iPod";

            case "computer keyboard, keypad":
                return "keyboard";

            case "mouse, computer mouse":
                return "mouse";

            case "remote control, remote":
                return "remote control/Calculator";

            case "folding chair":
                return "chair";

            default:
                return s_in;
        }
    }
}


