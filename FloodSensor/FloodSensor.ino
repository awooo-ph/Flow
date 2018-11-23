#define USE_LCD

#define VERSION "1.0.0-a"
#define SIREN_POWER 7
#define SIGNAL_BARS 4
#define GSM_RX 8
#define GSM_TX 9

#include "Settings.h"
#include "Sms.h"
#include "WaterLevel.h"

#ifdef USE_LCD
#include "Display.h"
#endif

WaterLevel river;
SmsClass Sms(GSM_RX, GSM_TX);

#ifdef USE_LCD
DisplayClass lcd(0x27);
#endif

uint8_t SirenPIN[3] = { 4,5,6 };

void OnWaterLevelChanged(uint8_t level)
{
    lcd.setLevel(level);

    digitalWrite(SIREN_POWER, HIGH);
    if (level == 0) return;

    delay(777);

    for (auto i = 0; i < 3; i++)
    {
        auto pin = SirenPIN[i];
        if (Settings.Current.SirenLevel[i] == level - 1)
        {
            digitalWrite(SIREN_POWER, LOW);
            delay(777);
            digitalWrite(pin, LOW);
        }
        else
        {
            digitalWrite(pin, HIGH);
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
    lcd.setSignal(signal);
}

void SimNumberChanged()
{
    char number[15];
    Sms.getNumber(number);
    lcd.setDescription(number);
}

bool showSignal = true;
bool displayOff = true;
unsigned long lastDisplayUpdate = 0;
uint8_t displayCount = 0;

void setup() {
   // #ifdef DEBUG
    Serial.begin(9600);
//#endif

    pinMode(SIREN_POWER, OUTPUT);
    digitalWrite(SIREN_POWER, HIGH);

    for (auto i : SirenPIN)
    {
        pinMode(i, OUTPUT);
        digitalWrite(i, HIGH);
    }

    Settings.LoadConfig();
    
    auto sms = Sms.init();

#ifdef USE_LCD
    lcd.showWelcome(VERSION);
    lcd.init();
    lcd.setSignal(-1);
#endif

    Sms.onSignalChanged(SignalChanged);
    Sms.onNumberChanged(SimNumberChanged);

    delay(1111);
    
    if(!sms) lcd.setDescription("MODEM ERROR");

    river.onLevelChange(OnWaterLevelChanged);
    river.init(A0, A1, A2, A3, 11, 10);

}

bool numberSet = false;
bool notReadySet = false;
uint8_t _lastSimStatus = 7;

// the loop function runs over and over again until power down or reset
void loop() {
    river.update();
    Sms.update();

    auto simStatus = Sms.getSimStatus();
    if (simStatus == 0)
        lcd.setDescription("LOADING...");
    else if(simStatus==-1)
        lcd.setDescription("INSERT SIM");


    lcd.update();
}
