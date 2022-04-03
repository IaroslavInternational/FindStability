#pragma once

#include "Headers/commands.h"

/* Библиотека для работы с АЦП HX711 */

#include <HX711.h>

/*************************************/

/* Настройки */

/* Общие настройки */


#define T_DELAY	   10 // Кол-во итерация считывания данных с HX711 для усреднения
#define DELAY_TIME 1  // Время задержки выполнения программы в мс	

/*******************/

/* Настройки подключения */

#define S_BAUD_RATE	9600 // Скорость обмена данными c SerialPort
#define S_TIMEOUT	10	 // Время ожидания приёма данных от SerialPort

/*************************/

/* Цифровые пины */


#define HX711_1_CLK	 3 // Пин clock  для АЦП HX711 1	
#define HX711_1_DOUT 2 // Пин данных для АЦП HX711 1
#define HX711_2_CLK	 5 // Пин clock  для АЦП HX711 2	
#define HX711_2_DOUT 4 // Пин данных для АЦП HX711 2
#define HX711_3_CLK	 7 // Пин clock  для АЦП HX711 3	
#define HX711_3_DOUT 6 // Пин данных для АЦП HX711 3
#define HX711_4_CLK	 9 // Пин clock  для АЦП HX711 4	
#define HX711_4_DOUT 8 // Пин данных для АЦП HX711 4

/*****************/

/* Настройки модулей */

#define HX711_1_CAL_FACTOR 1.0f		 // Калибровочный коэффициент для АЦП HX711 1
#define HX711_2_CAL_FACTOR 1.0f		 // Калибровочный коэффициент для АЦП HX711 2
#define HX711_3_CAL_FACTOR 1.0f		 // Калибровочный коэффициент для АЦП HX711 3
#define HX711_4_CAL_FACTOR 1.0f		 // Калибровочный коэффициент для АЦП HX711 4
#define HX711_SCALE		   0.035274f // Коэффициент для перевода е. и. в граммы для АЦП HX711

/*********************/

/*************/

// Управление АЦП HX711
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

// Система управления
class System
{
public:
	System();
public:	
	// Инициализатор модулей
	void InitiliazeModules();												
public:	
	// Предпроцессор
	void PreProcess(size_t b_rate = S_BAUD_RATE, size_t t_out = S_TIMEOUT); 

	// Обработчик входных команд
	void ProcessCommands();													
public:
	// Событие итерации
	void Tick();															
private:	
	hx711_adc tenzo1; // Тензодатчик 1
	hx711_adc tenzo2; // Тензодатчик 2
	hx711_adc tenzo3; // Тензодатчик 3
	hx711_adc tenzo4; // Тензодатчик 4
private:
	size_t current_t_time = 0;  // Текущий отсчёт времени перед усреднением данных тяги
	String sub_command    = ""; // Подкоманда 												
};