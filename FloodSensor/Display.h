// Display.h

#ifndef _DISPLAY_h
#define _DISPLAY_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#include <Wire.h>
#include <LiquidCrystal_I2C.h>

class DisplayClass
{
private:
    uint8_t _address;
    bool _lcdFound = false;
    bool _initialized = false;
    unsigned int _lastUpdate=0;
    char description[15]{};
    bool _messageSet = false;
    unsigned int _messageStarted=0;
    int _signal=7;
    int _displayIndex=0;
    uint8_t _waterLevel=7;
    LiquidCrystal_I2C * lcd;
    byte water_level[8] = { B11111,  B10001,  B10001,  B11101,  B10001,  B10001,  B11101,  B10001 };
    byte water_level_2[8] = { B10001,  B11101,  B10001,  B10001,  B11101,  B10001,  B10001,  B11111 };
    byte antenna[8] = { B11111,  B10101,  B01110,  B00100,  B00100,  B00100,  B00100,  B00100 };
    byte bars_1[8] = { B00000,  B00000,  B00000,  B00000,  B00000,  B00000,  B11000,  B11000 };
    byte bars_2[8] = { B00000,  B00000,  B00000,  B00000,  B00011,  B00011,  B11011,  B11011 };
    byte bars_3[8] = { B00000,  B00000,  B11000,  B11000,  B11000,  B11000,  B11000,  B11000 };
    byte bars_4[8] = { B00011,  B00011,  B11011,  B11011,  B11011,  B11011,  B11011,  B11011 };
    byte level0[8] = { B00000,  B00000,  B00000,  B00000,  B00000,  B00000,  B00010,  B00011 };
    byte level1[8] = {  B00000,  B00000,  B00000,  B00010,  B00011,  B00010,  B00000,  B00000 };
    byte level2[8] = {  B00010,  B00011,  B00010,  B00000,  B00000,  B00000,  B00000,  B00000 };
    byte level3_L[8] = { 0,  0,  0,  0,  0,  0,  0,  0 };
    byte level3_U[8] = { B00000,  B00000,  B00000,  B00000,  B00000,  B00010,  B00011,  B00010 };
    byte level4[8] = { B00000,  B00000,  B00010,  B00011,  B00010,  B00000,  B00000,  B00000 };
    byte level5[8] = {  B00011,  B00010,  B00000,  B00000,  B00000,  B00000,  B00000,  B00000 };
    byte blank[8] = {0,0,0,0,0,0,0,0};
    byte no_signal[8] = {
  0x00,
  0x00,
  0x00,
  0x00,
  0x00,
  0x00,
  0x00,
  0x1B
};

 public:
    DisplayClass(uint8_t);
	void showWelcome();
    void clear();
    void draw();
    void begin();
    void setSignal(int signal);
    void setLevel(uint8_t level);
    void setDescription(char * desc);
    void showSettingsChanged();
    void update();
};

extern DisplayClass Display;

#endif

