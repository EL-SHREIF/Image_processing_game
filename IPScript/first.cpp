#include "opencv2/objdetect.hpp"
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"
#include <iostream>
#include <stdio.h>

using namespace std;
using namespace cv;

struct Color32
{
	uchar r;
	uchar g;
	uchar b;
	uchar a;
};
extern "C" int __declspec(dllexport) __stdcall Detect(Color32** img,int width,int height)
{	
	Mat Img1(height, width, CV_8UC4, *img);
	flip(Img1, Img1, -1);
	imshow("Original Image", Img1);
	
	Mat hsv1,hsv2;
	cvtColor(Img1, hsv1, COLOR_RGBA2BGR);
	cvtColor(hsv1, hsv2, COLOR_BGR2HSV);
	Mat mask1, mask2;
	inRange(hsv2, Scalar(0, 120, 70), Scalar(10, 255, 255), mask1);
	inRange(hsv2, Scalar(170, 120, 70), Scalar(180, 255, 255), mask2);
	mask1 = mask1 + mask2;
	imshow("Mask", mask1);


	return 0;
	
}