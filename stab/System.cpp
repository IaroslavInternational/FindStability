#include "System.h"
#include "Headers\SpecialFuntions.hpp"

// Массив с данными тяги двигателя за 
// T_DELAY итераций
float t1Data[T_DELAY];
float t2Data[T_DELAY];
float t3Data[T_DELAY];
float t4Data[T_DELAY];

/*   System stuff   */

System::System()
	:
	tenzo1(HX711_1_CAL_FACTOR, HX711_SCALE),
	tenzo2(HX711_2_CAL_FACTOR, HX711_SCALE),
	tenzo3(HX711_3_CAL_FACTOR, HX711_SCALE),
	tenzo4(HX711_4_CAL_FACTOR, HX711_SCALE)
{
}

void System::InitiliazeModules()
{
	tenzo1.Setup(HX711_1_DOUT, HX711_1_CLK);
	tenzo2.Setup(HX711_2_DOUT, HX711_2_CLK);
	tenzo3.Setup(HX711_3_DOUT, HX711_3_CLK);
	tenzo4.Setup(HX711_4_DOUT, HX711_4_CLK);
}

void System::PreProcess(size_t b_rate, size_t t_out)
{
	// Установка скорости обмена данными
	Serial.begin(b_rate);
	Serial.setTimeout(t_out);

	// Цикл для тестового подключения
	while (true)
	{
		if (Serial.available() > 0)
		{
			String data = Serial.readString();
			Serial.println(data);

			// Тестовое подключение
			if (data == TEST_QUERY)
			{
				Serial.println(TEST_QUERY_SUCCESS);
			}

			// Рабочий режим
			if (data == WORK_MODE_ACTIVE)
			{
				Serial.println(WORK_MODE_ACTIVATING);
				break;
			}
		}
	}
}

void System::ProcessCommands()
{
}

void System::Tick()
{
	t1Data[current_t_time] = tenzo1.GetUnits();
	t2Data[current_t_time] = tenzo2.GetUnits();
	t3Data[current_t_time] = tenzo3.GetUnits();
	t4Data[current_t_time] = tenzo4.GetUnits();
	current_t_time = current_t_time + DELAY_TIME;

	if (current_t_time == T_DELAY)
	{
		tenzo1.Process(Tenzo1, t1Data);
		tenzo2.Process(Tenzo2, t2Data);
		tenzo3.Process(Tenzo3, t3Data);
		tenzo4.Process(Tenzo4, t4Data);
		Serial.println("___________________________");

		current_t_time = 0;
	}
}

/* end System stuff */


/*   HX 711 stuff   */

hx711_adc::hx711_adc(float cal_factor, float scale)
	:
	calibration_factor(cal_factor),
	scale(scale)
{
}

void hx711_adc::Setup(size_t data_pin, size_t clock_pin)
{
	hx711.begin(data_pin, clock_pin);
	hx711.set_scale(calibration_factor);
	hx711.tare();
}

void hx711_adc::Process(String header, float* arr)
{
	SpecialFunctions::SendData(header, String(SpecialFunctions::mean<float>(arr, T_DELAY)));
}

float hx711_adc::GetUnits()
{
	units = hx711.get_units(), 10;

	if (units < 0)
	{
		units = 0.00;
	}

	kg_press = units* scale;

	return kg_press;
}

/* end HX 711 stuff */
