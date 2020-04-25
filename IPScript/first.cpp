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
extern "C" float __declspec(dllexport) __stdcall DetectRed(Color32** img,int width,int height)
{	
	Mat Img1(height, width, CV_8UC4, *img);
	flip(Img1, Img1, -1);
	Mat hsv1,hsv2;
	cvtColor(Img1, hsv1, COLOR_RGBA2BGR);
	cvtColor(hsv1, hsv2, COLOR_BGR2HSV);
	Mat mask1, mask2;
	inRange(hsv2, Scalar(0, 120, 70), Scalar(10, 255, 255), mask1);
	inRange(hsv2, Scalar(170, 120, 70), Scalar(180, 255, 255), mask2);
	mask1 = mask1 + mask2;
	int redPixels = 0;
	for (int i = 0; i < mask1.rows; i++) {
		for (int j = 0; j < mask1.cols; j++)if (mask1.at<cv::Vec3b>(i, j)[0] != 0&& mask1.at<cv::Vec3b>(i, j)[1] != 0&& mask1.at<cv::Vec3b>(i, j)[2] != 0)redPixels++;
	}
	float score = (float)redPixels / (width*height);
	score = score * 10;
	return score;
	
}

extern "C" float __declspec(dllexport) __stdcall DetectBlue(Color32** img, int width, int height)
{
	Mat Img1(height, width, CV_8UC4, *img);
	flip(Img1, Img1, -1);
	Mat hsv1, hsv2;
	cvtColor(Img1, hsv1, COLOR_RGBA2BGR);
	cvtColor(hsv1, hsv2, COLOR_BGR2HSV);
	Mat mask1;
	inRange(hsv2, Scalar(100, 150, 0), Scalar(140, 255, 255), mask1);
	int BluePixels = 0;
	for (int i = 0; i < mask1.rows; i++) {
		for (int j = 0; j < mask1.cols; j++)if (mask1.at<cv::Vec3b>(i, j)[0] != 0 && mask1.at<cv::Vec3b>(i, j)[1] != 0 && mask1.at<cv::Vec3b>(i, j)[2] != 0)BluePixels++;
	}
	float score = (float)BluePixels / (width*height);
	score = score * 10;
	return score;

}

extern "C" float __declspec(dllexport) __stdcall DetectGreen(Color32** img, int width, int height)
{
	Mat Img1(height, width, CV_8UC4, *img);
	flip(Img1, Img1, -1);
	Mat hsv1, hsv2;
	cvtColor(Img1, hsv1, COLOR_RGBA2BGR);
	cvtColor(hsv1, hsv2, COLOR_BGR2HSV);
	Mat mask1;
	inRange(hsv2, Scalar(36, 0, 0), Scalar(86, 255, 255), mask1);
	mask1 = mask1;
	int GreenPixels = 0;
	for (int i = 0; i < mask1.rows; i++) {
		for (int j = 0; j < mask1.cols; j++)if (mask1.at<cv::Vec3b>(i, j)[0] != 0 && mask1.at<cv::Vec3b>(i, j)[1] != 0 && mask1.at<cv::Vec3b>(i, j)[2] != 0)GreenPixels++;
	}
	float score = (float)GreenPixels / (width*height);
	score = score * 10;
	return score;

}

