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

char ownNumber[20];

void SmsClass::parseCNUM(char* data)
{
    bool comma=false;
    bool start = false;
    int index = 0;
    int len=strlen(data);
    Serial.print("CNUM: len=");
    Serial.println(len);
    for(auto i=7;i<len,i++;)
    {
        if(comma && start && data[i]!='"')
        {
            ownNumber[index]=data[i];
            index++;
        }
        else
        {
            if(data[i]==',') comma = true;
            if(data[i]=='"')
            {
                if(start){
                    ownNumber[index]=0;
                    return;
                }
                else if(comma) start = true;
                
            }
            
        }
    }
}

SmsClass::SmsClass(uint8_t rx, uint8_t tx)
{
    sms = new SoftwareSerial(rx, tx);
}

bool SmsClass::init()
{
    if (_isReady) return;

    Serial.println(F("\nInitializing GSM modem..."));

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
            //Serial.println();
            //Serial.print(cmd);
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

    Serial.println(F("\nModem initialization complete"));   
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
        if (b == '\n' || b == '\r') {
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
int SmsClass::getSignal()
{
    if(_isReady) return -1;
    if(millis()-_lastCSQ>7777)
    {
        _lastCSQ = millis();
        sms->println("AT+CSQ");    
    }
    
    if (csq == 99 || csq==0) return -1;
    if (csq < 7) return 0;
    if (csq < 10) return 1;
    if (csq < 15) return 2;
    if (csq < 20) return 3;
    
    return 4;
}

void SmsClass::update()
{
    if (sms->available())
    {
        char data[147];
        readLine(data);
        parseData(data);
    }
    while (Serial.available())
        sms->write(Serial.read());
}

void SmsClass::send(char* number, char* text)
{
    if (!number || strlen(number) == 0 || !text || strlen(text) == 0) return;
    if (!number || strlen(number) < 7) return;
    if(!(number[0]=='0' || number[0]=='+')) return;

    Serial.println(F("\nSending message to: "));
    Serial.println(number);
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
    char data[47];
    readLine(data);
    return data;
}

bool SmsClass::startSend(char* number)
{
    if(!_isReady) return false;
    if (!number || strlen(number) < 7) return false;
    if(!(number[0]=='0' || number[0]=='+')) return false;
    if (_smsSendStarted) return false;
    _smsSendStarted = true;
    sms->print(F("AT+CMGS=\""));
    for (auto i = 0; i < sizeof(number); i++)
    {
        if (number[i] == '\0') break;
        sms->write(number[i]);
    }
    sms->print(F("\"\r"));
    char data[47];
    readLine(data);
    return true;
}

bool SmsClass::write(char* message)
{
    if (!_smsSendStarted) return false;
    sms->write(message);
    return true;
}

bool SmsClass::write(char text)
{
    if (!_smsSendStarted) return false;
    sms->write(text);
    return true;
}

bool SmsClass::commitSend()
{
    if (!_smsSendStarted) return false;
    sms->write(0x26);
    if (waitOk())
        Serial.println(F("\nMessage Sent!"));
    else
        Serial.println(F("\nMessage sending failed!"));
    _smsSendStarted = false;
    return true;
}

void SmsClass::cancelSend()
{
    if(!_smsSendStarted) return;
    sms->println("777");
    sms->println("777");
    sms->write(0x03);
}

void SmsClass::restart()
{

}


void SmsClass::getNumber(char num[])
{
    if(!_isReady) return;
    if(strlen(ownNumber)==0 && millis()-_lastCNUM>7777)
    {
        _lastCNUM = millis();
        sms->println(F("AT+CNUM"));
    } else
    {
        strcpy(num,ownNumber);    
    }
}


void SmsClass::readUnread()
{
    sms->println(F("AT+CMGL=\"REC UNREAD\""));sms->println(F("AT+CMGL=\"REC UNREAD\""));
}

unsigned long waitStart = 0;
bool SmsClass::waitOk()
{
    waitStart = millis();
    while (millis() - waitStart < 4444)
    {
        char response[147];
        readLine(response);
        if (response && strlen(response) > 0) {
            if (strcasecmp_P(response, OK) == 0)
                return true;

            auto res = strstr_P(ERROR, response);
            if (res)
                return false;
        }
    }
    return false;
}

void SmsClass::processCSQ(char command[])
{
    char c[20];
    for (auto x = 5; x < strlen(command); x++)
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

char* SmsClass::parseNumber(const char* str)
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
    char * number=parseNumber(command);

#ifndef UNRESTRICTED
    if (!isAdmin(number)) return;
#endif

    char msg[147];
    readLine(msg);

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

    if(startsWith("+CNUM:",command))
    {
        parseCNUM(command);
        return;
    }

    if (startsWith("+CREG:", command))
    {
        if (strcmp(command, "+CREG: 1") == 0)
            sms->println(F("AT+CSQ"));
        else
            _isRegistered = strcmp(command, "+CREG: 0,1") == 0 || strcmp(command, "+CREG: 1,1") == 0;
    }

    if (startsWith("+CLIP:", command)) //Hangup call
        sms->println(F("ATH"));
    else if (startsWith("+CMT: ", command)) //New message
        parseSMS(command);
}