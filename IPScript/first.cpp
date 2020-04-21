#include "opencv2/objdetect.hpp"
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"
#include <iostream>
#include <stdio.h>

using namespace std;
using namespace cv;

// Declare structure to be used to pass data from C++ to Mono.
struct Circle
{
	Circle(int x, int y, int radius) : X(x), Y(y), Radius(radius) {}
	int X, Y, Radius;
};

CascadeClassifier _faceCascade;
String _windowName = "Unity OpenCV Interop Sample";
VideoCapture _capture;
int _scale = 1;

extern "C" int __declspec(dllexport) __stdcall  Init(int& outCameraWidth, int& outCameraHeight)
{
	// Load LBP face cascade.
	if (!_faceCascade.load("lbpcascade_frontalface.xml"))
		return -1;

	// Open the stream.
	_capture.open(0);
	if (!_capture.isOpened())
		return -2;

	outCameraWidth = _capture.get(CAP_PROP_FRAME_WIDTH);
	outCameraHeight = _capture.get(CAP_PROP_FRAME_HEIGHT);

	return 0;
}

extern "C" void __declspec(dllexport) __stdcall  Close()
{
	_capture.release();
}

extern "C" void __declspec(dllexport) __stdcall SetScale(int scale)
{
	_scale = scale;
}
struct Color32
{
	uchar r;
	uchar g;
	uchar b;
	uchar a;
};
extern "C" int __declspec(dllexport) __stdcall Detect(Color32* img,int width,int height)
{
	Mat Img1(height, width, CV_8UC4, img);
	Mat mask1;
	// Creating masks to detect the upper and lower red color.
	inRange(Img1, Scalar(0, 0, 100), Scalar(50, 50, 255), mask1);
	
	// Generating the final mask
	mask1 = mask1;
	int redPixels = 0;
	for (int i = 0; i < mask1.rows; i++)for (int j = 0; j < mask1.cols; j++)if (mask1.at<cv::Vec3b>(i, j)[0] != 0)redPixels = redPixels + 1;
	
	return redPixels;
	
}