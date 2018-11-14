#include "Sms.h"

const char AT[]     PROGMEM = "AT\r";
const char CPIN[]   PROGMEM = "AT+CPIN?\r";
const char CREG1[]  PROGMEM = "AT+CREG=1\r";            // Network Registration
const char CSMS[]   PROGMEM = "AT+CSMS=1\r";            // Select Message Service
const char CNMI[]   PROGMEM = "AT+CNMI=2,2,0,0,0\r";    // New Message Indication
const char CMGF[]   PROGMEM = "AT+CMGF=1\r";            // Select Message Format (0:PDU; 1:TEXT)
const char CSCS[]   PROGMEM = "AT+CSCS=\"GSM\"\r";      // Select Character Set
const char OK[]     PROGMEM = "OK";
const char ERROR[]  PROGMEM = "ERROR";

const char* const COMMANDS[] PROGMEM = { AT,CPIN, CREG1,CSMS,CNMI,CMGF,CSCS };

SmsClass::SmsClass(uint8_t rx, uint8_t tx)
{
    sms = new SoftwareSerial(rx, tx);
}

bool SmsClass::init()
{
    if (_isReady) return;


    char cmd[47];

    sms->begin(9600);

    while (!sms) {}

    for (auto i = 0; i < 7; i++)
    {
        strcpy_P(cmd, (char*)pgm_read_word(&(COMMANDS[i])));
        uint8_t tries = 0;
        bool ok = false;
        while (!ok)
        {
            delay(777);
            sms->write(cmd);
            ok = waitOk();
            tries++;
            if (!ok && (tries >= 4 || i == 1))
            {
                errorCode = i;
                return false;
            }
        }

    }

    _isReady = true;

    delay(777);
    sms->println(F("AT+CMGL=\"REC UNREAD\""));

    return true;
}

void SmsClass::readLine(char data[])
{
    unsigned int count = 0;
    unsigned int timeout = 0;

    while (sms->available() == 0 && timeout < 1111)
    {
        delay(1);
        timeout++;
    }

    while (sms->available() > 0)
    {
        byte b = sms->read();
        Serial.write(b);
          if (b == '\n' || b=='\r') {
            data[count] = '\0';
            return;
          }
          
            data[count] = b;
            count++;

        /*if (b == 13)
        {
            data[count] = 0;
            return data;
        }
        if (b != 10)
        {
            data[count] = b;
            count++;
        }*/
        delay(7);
    }
    data[count] = 0;
}

/// Returns the signal strength (0-4)
uint8_t SmsClass::getSignal()
{
    if (csq < 7) return 0;
    if (csq < 10) return 1;
    if (csq < 15) return 2;
    if (csq < 19) return 3;
    if (csq == 99) return 0;
    return 4;
}

void SmsClass::update()
{
    if (sms->available())
        parseData(readLine());
}

void SmsClass::send(char* number, char* text)
{
    startSend(number);
    write(text);
    commitSend();
}

void SmsClass::onReceive(void(*callback)(char* number, char* message))
{
    onReceiveCallback = callback;
}

char* SmsClass::getIMEI()
{
    sms->println(F("AT+GSN"));
    return readLine();
}

void SmsClass::startSend(char* number)
{
    sms->print(F("AT+CMGS=\""));
    for(auto i=0;i<sizeof(number);i++)
    {
        if(number[i]=='\0') break;
        sms->write(number[i]);
    }
    sms->print(F("\"\r"));
    readLine();
}

void SmsClass::write(char* message)
{
    sms->write(message);
}

void SmsClass::write(char text)
{
    sms->write(text);
}

void SmsClass::commitSend()
{
    sms->write(0x26);
    waitOk();
}

bool SmsClass::waitOk()
{
    int start = millis();
    int now = start;
    while (now - start < 10000)
    {
        char* response = readLine();

        if (strcasecmp_P(response, OK) == 0)
            return true;

        if (strcasecmp_P(response, ERROR) == 0)
            return false;

        now = millis();
    }
    return false;
}

void SmsClass::processCSQ(char command[])
{
    char c[sizeof(command)];
    for (auto x = 5; x < sizeof(command); x++)
    {
        if (command[x] == ',')
        {
            csq = atoi(c);
            return;
        }
        c[x - 5] = command[x];
    }
}

bool SmsClass::startsWith(const char* pre, const char* str)
{
    //if (sizeof(str) < sizeof(pre) || sizeof(str) == 0 || sizeof(pre) == 0) return false;
    return strncmp(pre, str, strlen(pre)) == 0;
}

char* SmsClass::getNumber(const char* str)
{
    char _number[12];

    int len = strlen(str);

    int index = 2;
    bool start = false;
    bool record = false;

    for (int i = 0; i < len; i++)
    {
        if (str[i] == '"')
        {
            if (start)
            {
                _number[index] = '\0';
                return _number;
            }
            start = true;
        }

        if (record)
        {
            _number[index] = str[i];
            index++;
        }

        if (str[i] == '9' && start && !record)
        {
            record = true;
        }
    }

    return _number;
}

#ifndef UNRESTRICTED
bool SmsClass::isAdmin(char* number)
{
    char * _number = "00000000000";

    if (number[0] == '0') strcpy(_number, number);
    else if (number[0] == '+')
        for (auto i = 3; i < strlen(number); i++)
            _number[i - 2] = number[i];
    else return false;

    for (auto i = 0; i < 4; i++)
        if (strcmp(_number, Settings.Current.Monitor[i]) == 0) return true;

    return false;
}
#endif


void SmsClass::parseSMS(char* command)
{
    char* number = getNumber(command);

#ifndef UNRESTRICTED
    if (!isAdmin(number)) return;
#endif

    char* msg = readLine();

    if (onReceiveCallback) onReceiveCallback(number, msg);
}

void SmsClass::parseData(char* command)
{
    if (strlen(command) == 0) return;

    if (startsWith("+CSQ:", command))
    {
        processCSQ(command);
        return;
    }

    if (startsWith("+CREG:", command))
    {
        if (strcmp(command, "+CREG: 1") == 0)
            sms->println(F("AT+CREG?"));
        else
            _isRegistered = strcmp(command, "+CREG: 0,1") == 0 || strcmp(command, "+CREG: 1,1") == 0;
    }

    if (startsWith("+CLIP:", command)) //Hangup call
        sms->println(F("ATH"));
    else if (startsWith("+CMT: ", command)) //New message
        parseSMS(command);
}