#define USE_LCD
//define USE_7_SEGMENTS

#define VERSION "1.0.0-a"
#define SIREN_POWER 7
#define SIGNAL_BARS 4
#define GSM_RX 11
#define GSM_TX 12

#include <Arduino.h>
#include "Settings.h"
#include "Sms.h"
#include  "WaterLevel.h"

#ifdef USE_LCD
#include "Display.h"
#endif

#ifdef USE_7_SEGMENTS
#include <TM1637Display.h>
#endif

WaterLevel river;
SmsClass Sms(GSM_RX,GSM_TX);

#ifdef USE_LCD
DisplayClass lcd(0x27);
#endif

uint8_t SirenPIN[3] = { 4,5,6 };

#ifdef USE_7_SEGMENTS
const uint8_t LED_R = SEG_E | SEG_G;
const uint8_t LED_A = LED_R | SEG_A | SEG_F | SEG_B | SEG_C;
const uint8_t LED_L = SEG_F | SEG_E | SEG_D;
const uint8_t LED_C = LED_L | SEG_A;
const uint8_t LED_o = SEG_C | SEG_D | SEG_E | SEG_G;
const uint8_t LED_d = LED_o | SEG_B;
const uint8_t LED_E = LED_C | SEG_G;
const uint8_t LED_N = LED_R | SEG_C;
const uint8_t LED_S = SEG_A | SEG_F | SEG_G | SEG_C | SEG_D;
const uint8_t LED_i = SEG_E;
const uint8_t LED_G = LED_C | SEG_C;

uint8_t LED_ERROR[] = {LED_E,LED_R,LED_R,0};
const uint8_t LED_GOOD[] = {LED_G,LED_o,LED_o,LED_d};
const uint8_t LED_LOAD[] = {LED_L,LED_o,LED_A,LED_d};
const uint8_t LED_CARD[] = {LED_C,LED_A,LED_R,LED_d};
const uint8_t LED_BLANK[] = {0,0,0,0};
const uint8_t LED_NO[] = {0,LED_N,LED_o,0};

TM1637Display display(8, 9);
#endif

void OnWaterLevelChanged(uint8_t level)
{
#ifdef USE_LCD
    lcd.setLevel(level);
#endif

    digitalWrite(SIREN_POWER, HIGH);
    if(level==0) return;

    delay(777);
    
    for (auto i = 0; i < 3; i++)
    {
        auto pin = SirenPIN[i];
        if (Settings.Current.SirenLevel[i] == level-1)
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

    if (Settings.Current.NotifyLevel[level-1])
    {
        char * msg = ".0";
        sprintf(msg, ".%d", level);
        for (auto number : Settings.Current.Monitor)
        {
            Sms.send(number,msg);
        }
        for (auto number : Settings.Current.NotifyNumbers)
            Sms.send(number,Settings.Current.LevelMessage[level-1]);
    }
}

void onReceive(char* number, char* message)
{
    switch (message[0]) {
    case '?':

        if(Sms.startSend(number)) return;
        Sms.write("!");
        Sms.write(message[1]);
        switch (message[1])
        {
        case 'v':
            Sms.write(VERSION);
            break;
        case 'n':
            Sms.write(Settings.Current.SensorName);
            break;
        case 'i':
            Sms.write(Sms.getIMEI());
            break;
        default:
            Sms.write(message[1]);
        }
        Sms.commitSend();
        break;
    case '=':
        switch (message[1])
        {
        case '@':
            for(auto i=2;i<strlen(message);i++)
            {
                if(i<17) Settings.Current.SensorName[i-2] = message[i];
                if(message[i]=='\0') break;
            }
            break;
        case '#':
            for(auto i=0;i<5;i++)
               Settings.Current.NotifyLevel[i] = message[i+2]=='1';
            for(auto i=0;i<3;i++)
                Settings.Current.SirenLevel[i] = message[i+7];
            break;
        case '$':
            break;
        default: ;
        }

        Settings.SaveConfig();
        break;
    default:;
    }
}

bool showSignal = true;
bool displayOff = true;
unsigned long lastDisplayUpdate = 0;
uint8_t displayCount = 0;

#ifdef USE_7_SEGMENTS
void updateDisplay()
{
    if(millis()-lastDisplayUpdate<1111) return;
    lastDisplayUpdate = millis();

    uint8_t data[4] = {0,0,0,0};
    if(displayOff)
    {
        if(showSignal)
        {
            data[0] = LED_L;
            data[3] = display.encodeDigit(river.getLevel());
        } else
        {
            data[0] = LED_S;
            data[3] = display.encodeDigit(Sms.getSignal());
        }
        displayCount++;
        if(displayCount==7)
        {
            showSignal = !showSignal;
            displayCount = 0;
        }
    }
    displayOff = !displayOff;
    display.setSegments(data);
}
#endif

void setup() {
    Serial.begin(9600);
    pinMode(SIREN_POWER, OUTPUT);
    digitalWrite(SIREN_POWER, HIGH);

    for (auto i : SirenPIN)
    {
        pinMode(i, OUTPUT);
        digitalWrite(i, HIGH);
    }

    Settings.LoadConfig();

#ifdef USE_LCD
    lcd.showWelcome(VERSION);
#endif

    Sms.onReceive(onReceive);

#ifdef USE_7_SEGMENTS
        display.setSegments(LED_LOAD);
        delay(1111);
#endif
#ifdef USE_LCD
        lcd.showLoading();
#endif

        Sms.init();
            #ifdef USE_7_SEGMENTS
            for(auto i=0;i<17;i++){

                LED_ERROR[3] = display.encodeDigit(Sms.getError()+1);
                display.setSegments(LED_ERROR);
                delay(1111);
                display.setSegments(LED_BLANK);
                delay(777);
                if(Sms.getError()==1)
                {
                    display.setSegments(LED_NO);
                    delay(1111);
                    display.setSegments(LED_BLANK);
                    delay(777);
                    display.setSegments(LED_CARD);
                    delay(1111);
                    display.setSegments(LED_BLANK);
                    delay(777);
                }
            }
            #endif
#ifndef USE_LCD || USE_7_SEGMENTS
            delay(47777);
#endif
#ifdef USE_LCD
            
            if(Sms.getError()==1)
            {
                lcd.init();
                lcd.setSignal(-1);
                lcd.setDescription("INSERT SIM");
                delay(1111);
            }
#endif
        
    
#ifdef USE_7_SEGMENTS
    display.setSegments(LED_GOOD);
#endif

#ifdef USE_LCD
    lcd.init();
#endif

    char number[20];
    Sms.getNumber(number);

    river.onLevelChange(OnWaterLevelChanged);
    river.init(A0, A1, A2, A3, 4);

    delay(1111);
}

bool numberSet = false;
// the loop function runs over and over again until power down or reset
void loop() {
    river.update();
    Sms.update();

    // Signal LED
    //for (auto i = 0; i < SIGNAL_BARS; i++)
    //    digitalWrite(SignalPIN[i], Sms.getSignal() > i);
#ifdef USE_7_SEGMENTS
    updateDisplay();
#endif

#ifdef USE_LCD
    if(Sms.isReady()){
        lcd.setSignal(Sms.getSignal());

        if(!numberSet){
            char number[20];
            Sms.getNumber(number);
            if(strlen(number)>0)
                numberSet = true;
            lcd.setDescription(number);
        }
    }
#endif
}
