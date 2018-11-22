#include "Settings.h"

#ifndef MAXIMUM_MESSAGE_LENGTH
#define MAXIMUM_MESSAGE_LENGTH 47
#endif

#ifndef MAXIMUM_SENSOR_NAME_LENGTH
#define MAXIMUM_SENSOR_NAME_LENGTH 47
#define MAXIMUM_SENSOR_LOCATION_LENGTH 74
#endif

//const char STR_LEVEL_1[]     PROGMEM = "LEVEL 1";
//const char STR_LEVEL_2[]     PROGMEM = "LEVEL 2";
//const char STR_LEVEL_3[]     PROGMEM = "LEVEL 3";
//const char STR_LEVEL_4[]     PROGMEM = "LEVEL 4";
//const char STR_LEVEL_5[]     PROGMEM = "LEVEL 5";
//const char STR_SENSOR_NAME[] PROGMEM = "SENSOR";

//const char * const SETTINGS_DEFAULTS[] PROGMEM = {STR_LEVEL_1,STR_LEVEL_2,STR_LEVEL_3,STR_LEVEL_4,STR_LEVEL_5,STR_SENSOR_NAME};

void SettingsClass::LoadConfig()
{
	Current = Config();
	Current.checksum = 0;
	unsigned int sum = 0;
	unsigned char t;

	for (unsigned int i = 0; i < sizeof(Current); i++) {
		t = (unsigned char)EEPROM.read(i);
		*((char *)&Current + i) = t;
		if (i < sizeof(Current) - sizeof(Current.checksum)) {
			/* Don't checksum the checksum! */
			sum = sum + t;
		}
	}

	/* Now check the data we just read */
	if (Current.checksum != sum) {
		Current = Config();
        for (auto i=0;i<7;i++)
            Current.NotifyNumbers[i] = new char[15];

        //for (auto i=0;i<4;i++)
        //    Current.Monitor[i] = new char[15];

        for (auto i=0;i<7;i++)
            Current.Sensors[i] = new char[15];
        //for(auto i=0;i<5;i++)
        //{
        //    Current.LevelMessage[i] = new char[MAXIMUM_MESSAGE_LENGTH];
        //    strcpy_P(Current.LevelMessage[i],(char *)pgm_read_word(&(SETTINGS_DEFAULTS[i])));
        //}
        Current.SensorName = new char[MAXIMUM_SENSOR_NAME_LENGTH];
        Current.Location = new char[MAXIMUM_SENSOR_LOCATION_LENGTH];
        strcpy(Current.SensorName,"SENSOR1");
	} 
	
}

void SettingsClass::ResetConfig() {
	Current = Config();
	SaveConfig();
}

void SettingsClass::SaveConfig() {
	unsigned int sum = 0;
    
	unsigned int size = sizeof(Config);

	for (unsigned int i = 0; i < size; i++) {

		if (i == sizeof(Current) - sizeof(Current.checksum)) {
			Current.checksum = sum;
		}

	    const auto t = *((unsigned char*)&Current + i);
		if (i < sizeof(Current) - sizeof(Current.checksum)) {
			sum = sum + t;
		}

		EEPROM.write(i,t);
		
	}

}

SettingsClass Settings;