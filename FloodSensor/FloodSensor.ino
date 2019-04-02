//#define USE_LCD

#define VERSION "1.0.0"
#define SIREN_POWER A0
#define LIGHT_POWER A1

#define LEVEL1 3
#define LEVEL2 4
#define LEVEL3 5
#define LEVEL4 6
#define LEVEL5 7

//#define DEBUG_SERIAL
//#define DISABLE_SIREN

#include "Settings.h"
#include "Sms.h"
#include "WaterLevel.h"

#ifdef USE_LCD
#include "Display.h"
#endif

WaterLevel river;
SmsClass Sms;

#ifdef USE_LCD
DisplayClass display;
#endif


uint8_t SirenPIN[3] = { 10,11,12 };
bool numberSet = false;
bool notReadySet = false;
uint8_t _lastSimStatus = 7;

int _displayIndex = -1;

void SirenOn()
{
    digitalWrite(SIREN_POWER,LOW);
    digitalWrite(12,LOW);
}

void SirenOff()
{
    digitalWrite(SIREN_POWER,HIGH);
    digitalWrite(12,HIGH);
}

void Buzz()
{
    SirenOn();
    delay(111);
    SirenOff();
    delay(111);
    SirenOn();
    delay(444);
    SirenOff();
}


void OnWaterLevelChanged(uint8_t level)
{
#ifdef USE_LCD
    display.setLevel(level);
#endif

    digitalWrite(SIREN_POWER, HIGH);
    for (uint8_t pin : SirenPIN)
        digitalWrite(pin,HIGH);

    delay(777);

    if(level>0)
    {
        bool useLevel3 = true;
        for (auto i = 0; i < 3; i++)
        {
            auto pin = SirenPIN[i];
            if (Settings.Current.SirenLevel[i] == level)
            {
                useLevel3 = false;
#ifndef DISABLE_SIREN
                digitalWrite(SIREN_POWER, LOW);
                delay(777);
                digitalWrite(pin, LOW);
#endif
#ifdef DEBUG_SERIAL
                Serial.print(F("SIREN "));
                Serial.print(i+1);
                Serial.println(F(" ACTIVATED!"));
#endif
                break;
            }
        }
        if(useLevel3)
        {
#ifndef DISABLE_SIREN
            digitalWrite(SIREN_POWER, LOW);
            delay(777);
            digitalWrite(Settings.Current.SirenLevel[2], LOW);
#endif
#ifdef DEBUG_SERIAL
            Serial.println(F("Water level is higher than max warning level."));
            Serial.println(F("SIREN 3 ACTIVATED!"));
#endif
        }
    }
   

    char * msg = ".0";
    sprintf(msg, ".%d", level);

    Sms.send(Settings.Current.Monitor, msg);

    if (Settings.Current.WarningLevel>0 && Settings.Current.WarningLevel<=level)
        for(auto sensor:Settings.Current.Sensors)
            Sms.send(sensor,"*");

}

void SignalChanged(int signal)
{
    #ifdef USE_LCD
    display.setSignal(signal);
#endif
#ifdef DEBUG_SERIAL
            Serial.print(F("Signal Changed: "));
    Serial.println(signal);
#endif
}

void SimNumberChanged()
{
    char number[15];
    Sms.getNumber(number);
    #ifdef USE_LCD
    display.setDescription(number);
#endif
    _displayIndex = 2;
#ifdef DEBUG_SERIAL
            Serial.println(number);
#endif
}

void SettingsReceived()
{
    #ifdef USE_LCD
    display.showSettingsChanged();
#endif
#ifdef DEBUG_SERIAL
            Serial.println(F("Settings Received"));
#endif
}

void OnAlarm()
{
    digitalWrite(SIREN_POWER, HIGH);
    for (uint8_t pin : SirenPIN)
        digitalWrite(pin,HIGH);

#ifndef DISABLE_SIREN
    delay(777);
    digitalWrite(SIREN_POWER, LOW);
    delay(777);
    digitalWrite(SirenPIN[2], LOW);
#endif
#ifdef DEBUG_SERIAL
            Serial.println(F("Alarm Received, SIREN 3 activated!"));
#endif
}

bool showSignal = true;
bool displayOff = true;
unsigned long lastDisplayUpdate = 0;
uint8_t displayCount = 0;

void setup() {
    pinMode(SIREN_POWER, OUTPUT);
    digitalWrite(SIREN_POWER, HIGH);
       
    for (auto i : SirenPIN)
    {
        pinMode(i, OUTPUT);
        digitalWrite(i, HIGH);
    }

    //Buzz();

#ifdef DEBUG_SERIAL
    Serial.begin(9600);
    Serial.println("LOADING...");
#endif

    Settings.LoadConfig();
#ifdef DEBUG_SERIAL
    Serial.println(F("Settings Loaded"));
#endif
    
#ifdef USE_LCD
    display.begin();
    display.showWelcome();
    display.draw();
    display.setSignal(-1);
    _displayIndex = 0;
    display.setDescription("LOADING...");
    display.update();
#endif
    delay(2222);
    Sms.init();
    
    Sms.onSignalChanged(SignalChanged);
    Sms.onNumberChanged(SimNumberChanged);
    Sms.onSettingsReceived(SettingsReceived);
    Sms.onAlarm(OnAlarm);
    delay(1111);
    Sms.update();

    river.onLevelChange(OnWaterLevelChanged);
    river.init(LEVEL1,LEVEL2,LEVEL3, LEVEL4, LEVEL5, 0);

#ifdef DEBUG_SERIAL
    Serial.println(F("Loading Complete"));
#endif
}
//unsigned long _lastBuzz = 0;
void loop() {
  /*  if(millis()-_lastBuzz>7777)
    {
        _lastBuzz = millis();
        Buzz();
    }*/
    
    river.update();
    if(Sms.modemDetected()){
        Sms.update();

        auto simStatus = Sms.getSimStatus();
        
        if (simStatus == 0)
        {
            if(_displayIndex==0) return;
#ifdef USE_LCD
            display.setDescription("LOADING...");
#endif
#ifdef DEBUG_SERIAL
            Serial.println(F("GSM Loading..."));
#endif
            _displayIndex = 0;
        }
        else if(simStatus==-1)
        {
            if(_displayIndex==1) return;
            #ifdef USE_LCD
            display.setDescription("INSERT SIM");
#endif
#ifdef DEBUG_SERIAL
            Serial.println(F("INSERT SIM"));
#endif
            _displayIndex = 1;
        }
    } else
    {
        if(_displayIndex==2) return;
        #ifdef USE_LCD
        display.setDescription("MODEM ERROR");
#endif
#ifdef DEBUG_SERIAL
            Serial.println(F("MODEM ERROR"));
#endif
        _displayIndex = 2;
    }
    #ifdef USE_LCD
    display.update();
#endif
}