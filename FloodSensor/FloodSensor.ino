#define USE_LCD

#define VERSION "1.0.0-a"
#define SIREN_POWER 4
//#define GSM_POWER 12
#define GSM_RX 2
#define GSM_TX 3

#include "Settings.h"
#include "Sms.h"
#include "WaterLevel.h"

#ifdef USE_LCD
#include "Display.h"
#endif

WaterLevel river;
SmsClass Sms(GSM_RX, GSM_TX);

#ifdef USE_LCD
DisplayClass display(0x27);
#endif

uint8_t SirenPIN[3] = { 11,10,9 };
bool numberSet = false;
bool notReadySet = false;
uint8_t _lastSimStatus = 7;

int _displayIndex = -1;

void OnWaterLevelChanged(uint8_t level)
{
    display.setLevel(level);

    digitalWrite(SIREN_POWER, HIGH);
    for (uint8_t pin : SirenPIN)
        digitalWrite(pin,HIGH);

    delay(777);

    if(level>0)
    for (auto i = 0; i < 3; i++)
    {
        auto pin = SirenPIN[i];
        if (Settings.Current.SirenLevel[i] == level)
        {
            digitalWrite(SIREN_POWER, LOW);
            delay(777);
            digitalWrite(pin, LOW);
            Serial.print("LEVEL:");
            Serial.println(level);
        }
    }

    char * msg = ".0";
    sprintf(msg, ".%d", level);

    Sms.send(Settings.Current.Monitor, msg);

    if (Settings.Current.WarningLevel>0 && Settings.Current.WarningLevel<=level)
        for(auto sensor:Settings.Current.Sensors)
            Sms.send(sensor,msg);

}

void SignalChanged(int signal)
{
    display.setSignal(signal);
}

void SimNumberChanged()
{
    char number[15];
    Sms.getNumber(number);
    display.setDescription(number);
    _displayIndex = 2;
}

void SettingsReceived()
{
    display.showSettingsChanged();
}


bool showSignal = true;
bool displayOff = true;
unsigned long lastDisplayUpdate = 0;
uint8_t displayCount = 0;

void setup() {
    pinMode(SIREN_POWER, OUTPUT);
    digitalWrite(SIREN_POWER, HIGH);
    
    Serial.begin(9600);
    
    for (auto i : SirenPIN)
    {
        pinMode(i, OUTPUT);
        digitalWrite(i, HIGH);
    }

    Settings.LoadConfig();

    Sms.init();

#ifdef USE_LCD
    display.begin();
    display.showWelcome();
    display.draw();
    display.setSignal(-1);
#endif
    
    Sms.update();

    Sms.onSignalChanged(SignalChanged);
    Sms.onNumberChanged(SimNumberChanged);
    Sms.onSettingsReceived(SettingsReceived);
    delay(1111);
    
    river.onLevelChange(OnWaterLevelChanged);
    river.init(8,7,A0, A1, A2, A3);
   
}

void loop() {
    river.update();
    if(Sms.modemDetected()){
        Sms.update();

        auto simStatus = Sms.getSimStatus();
        
        if (simStatus == 0)
        {
            if(_displayIndex==0) return;
            display.setDescription("LOADING...");
            _displayIndex = 0;
        }
        else if(simStatus==-1)
        {
            if(_displayIndex==1) return;
            display.setDescription("INSERT SIM");
            _displayIndex = 1;
        }
    } else
    {
        if(_displayIndex==2) return;
        display.setDescription("MODEM ERROR");
        _displayIndex = 2;
    }

    display.update();
}