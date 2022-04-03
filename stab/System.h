#pragma once

#include "Headers/commands.h"

/* ���������� ��� ������ � ��� HX711 */

#include <HX711.h>

/*************************************/

/* ��������� */

/* ����� ��������� */


#define T_DELAY	   10 // ���-�� �������� ���������� ������ � HX711 ��� ����������
#define DELAY_TIME 1  // ����� �������� ���������� ��������� � ��	

/*******************/

/* ��������� ����������� */

#define S_BAUD_RATE	9600 // �������� ������ ������� c SerialPort
#define S_TIMEOUT	10	 // ����� �������� ����� ������ �� SerialPort

/*************************/

/* �������� ���� */


#define HX711_1_CLK	 3 // ��� clock  ��� ��� HX711 1	
#define HX711_1_DOUT 2 // ��� ������ ��� ��� HX711 1
#define HX711_2_CLK	 5 // ��� clock  ��� ��� HX711 2	
#define HX711_2_DOUT 4 // ��� ������ ��� ��� HX711 2
#define HX711_3_CLK	 7 // ��� clock  ��� ��� HX711 3	
#define HX711_3_DOUT 6 // ��� ������ ��� ��� HX711 3
#define HX711_4_CLK	 9 // ��� clock  ��� ��� HX711 4	
#define HX711_4_DOUT 8 // ��� ������ ��� ��� HX711 4

/*****************/

/* ��������� ������� */

#define HX711_1_CAL_FACTOR 1.0f		 // ������������� ����������� ��� ��� HX711 1
#define HX711_2_CAL_FACTOR 1.0f		 // ������������� ����������� ��� ��� HX711 2
#define HX711_3_CAL_FACTOR 1.0f		 // ������������� ����������� ��� ��� HX711 3
#define HX711_4_CAL_FACTOR 1.0f		 // ������������� ����������� ��� ��� HX711 4
#define HX711_SCALE		   0.035274f // ����������� ��� �������� �. �. � ������ ��� ��� HX711

/*********************/

/*************/

// ���������� ��� HX711
class hx711_adc
{
public:
	hx711_adc(float cal_factor, float scale);
public:
	void Setup(size_t data_pin, size_t clock_pin);	
	void Process(String header, float* arr);			
	float GetUnits();
private:
	HX711 hx711;									
	float calibration_factor;						
	float scale;									
	float units    = 0.0f;								
	float kg_press = 0.0f;							
private:
	bool IsHX_Valid = false;						
};

// ������� ����������
class System
{
public:
	System();
public:	
	// ������������� �������
	void InitiliazeModules();												
public:	
	// �������������
	void PreProcess(size_t b_rate = S_BAUD_RATE, size_t t_out = S_TIMEOUT); 

	// ���������� ������� ������
	void ProcessCommands();													
public:
	// ������� ��������
	void Tick();															
private:	
	hx711_adc tenzo1; // ����������� 1
	hx711_adc tenzo2; // ����������� 2
	hx711_adc tenzo3; // ����������� 3
	hx711_adc tenzo4; // ����������� 4
private:
	size_t current_t_time = 0;  // ������� ������ ������� ����� ����������� ������ ����
	String sub_command    = ""; // ���������� 												
};