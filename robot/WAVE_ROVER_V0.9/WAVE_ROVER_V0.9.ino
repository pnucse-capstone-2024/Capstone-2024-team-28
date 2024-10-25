#include <ArduinoJson.h>
StaticJsonDocument<256> jsonCmdReceive;
StaticJsonDocument<256> jsonInfoSend;
StaticJsonDocument<512> jsonInfoHttp;

// TaskHandle_t Pid_ctrl;

#include <SCServo.h>
#include <nvs_flash.h>
#include <esp_system.h>
#include <LittleFS.h>
#include <WiFi.h>
#include <WebServer.h>
#include <esp_now.h>
#include <nvs_flash.h>
#include <Adafruit_SSD1306.h>
#include <INA219_WE.h>
#include <ESP32Encoder.h>
#include <PID_v2.h>
#include <SimpleKalmanFilter.h>
#include <math.h>
#include <Adafruit_ICM20X.h>
#include <Adafruit_ICM20948.h>
#include <Adafruit_Sensor.h>

#include <WebSocketsClient.h>
#include <Base64.h>
#include <ESP32_Servo.h>

// functions for barrery info.
#include "battery_ctrl.h"

// functions for oled.
#include "oled_ctrl.h"

// config for ugv.
#include "ugv_config.h"

// functions for the leds of UGV.
#include "ugv_led_ctrl.h"

// functions for RoArm-M2 ctrl.
#include "RoArm-M2_module.h"

// functions for gimbal ctrl.
#include "gimbal_module.h"

// define json cmd.
#include "json_cmd.h"

// functions for IMU ctrl.
#include "IMU_ctrl.h"

// functions for movtion ctrl. 
#include "movtion_module.h"

// functions for editing the files in flash.
#include "files_ctrl.h"

// advance functions for ugv ctrl.
#include "ugv_advance.h"

// functions for wifi ctrl.
#include "wifi_ctrl.h"

// functions for esp-now.
#include "esp_now_ctrl.h"

// functions for uart json ctrl.
#include "uart_ctrl.h"

// functions for http & web server.
#include "http_server.h"


void moduleType_RoArmM2() {
  unsigned long curr_time = millis();
  if (curr_time - prev_time >= 10){
    constantHandle();
    prev_time = curr_time;
  }

  RoArmM2_getPosByServoFeedback();
  
  // esp-now flow ctrl as a flow-leader.
  switch(espNowMode) {
  case 1: espNowGroupDevsFlowCtrl();break;
  case 2: espNowSingleDevFlowCtrl();break;
  }

  if (InfoPrint == 2) {
    RoArmM2_infoFeedback();
  }
}


void moduleType_Gimbal() {
  getGimbalFeedback();
  gimbalSteady(steadyGoalY);
}

// 서보 객체 생성
Servo servoLeft;
Servo servoRight;

// 서보 핀 정의
#define SERVO_LEFT_PIN 18
#define SERVO_RIGHT_PIN 19

void setupServos() {
  servoLeft.attach(SERVO_LEFT_PIN);
  servoRight.attach(SERVO_RIGHT_PIN);
  stopMotors();
}

// 서보 각도 범위 설정
#define SERVO_MIN_ANGLE 0
#define SERVO_MAX_ANGLE 180
#define SERVO_STOP_ANGLE 90

void moveForward(int speed) {
  int leftAngle = map(speed, 0, 255, SERVO_STOP_ANGLE, SERVO_MAX_ANGLE);
  int rightAngle = map(speed, 0, 255, SERVO_STOP_ANGLE, SERVO_MIN_ANGLE);
  servoLeft.write(leftAngle);
  servoRight.write(rightAngle);
}

void moveBackward(int speed) {
  int leftAngle = map(speed, 0, 255, SERVO_STOP_ANGLE, SERVO_MIN_ANGLE);
  int rightAngle = map(speed, 0, 255, SERVO_STOP_ANGLE, SERVO_MAX_ANGLE);
  servoLeft.write(leftAngle);
  servoRight.write(rightAngle);
}

void turnLeft(int speed) {
  int angle = map(speed, 0, 255, SERVO_STOP_ANGLE, SERVO_MAX_ANGLE);
  servoLeft.write(SERVO_STOP_ANGLE);
  servoRight.write(angle);
}

void turnRight(int speed) {
  int angle = map(speed, 0, 255, SERVO_STOP_ANGLE, SERVO_MIN_ANGLE);
  servoLeft.write(angle);
  servoRight.write(SERVO_STOP_ANGLE);
}

void stopMotors() {
  servoLeft.write(SERVO_STOP_ANGLE);
  servoRight.write(SERVO_STOP_ANGLE);
}

WebSocketsClient webSocket;

void webSocketEvent(WStype_t type, uint8_t * payload, size_t length) {
  switch(type) {
    case WStype_DISCONNECTED:
      Serial.println("WebSocket disconnected");
      break;
    case WStype_CONNECTED:
      Serial.println("WebSocket connected");
      break;
    case WStype_TEXT:
      // Text 타입의 메시지는 MIME 데이터로 간주하고 그대로 출력
      Serial.println("Received MIME data:");
      Serial.println((char*)payload);
      break;
    case WStype_BIN:
      // Base64로 디코딩
      int decodedLength = Base64.decodedLength((char*)payload, length);
      char decodedString[decodedLength];
      Base64.decode(decodedString, (char*)payload, length);

      // JSON 파싱
      StaticJsonDocument<200> doc;
      DeserializationError error = deserializeJson(doc, decodedString);

      if (error) {
        Serial.println("JSON 파싱 실패");
        return;
      }

      // 액션 실행
      const char* action = doc["action"];
      int speed = 200; // 기본 속도 설정 (0-255 사이의 값)

      if (strcmp(action, "forward") == 0) {
        moveForward(speed);
        Serial.println("Moving forward");
      } else if (strcmp(action, "backward") == 0) {
        moveBackward(speed);
        Serial.println("Moving backward");
      } else if (strcmp(action, "right") == 0) {
        turnRight(speed);
        Serial.println("Turning right");
      } else if (strcmp(action, "left") == 0) {
        turnLeft(speed);
        Serial.println("Turning left");
      } else if (strcmp(action, "stop") == 0) {
        stopMotors();
        Serial.println("Stopping");
      }
      break;
  }
}


void setup() {
  Serial.begin(115200);

  // 서보 모터 설정
  setupServos();

  Wire.begin(S_SDA, S_SCL);
  while(!Serial) {}

  ina219_init();
  inaDataUpdate();

  // set mainType & moduleType.
  // mainType: 1.WAVE ROVER, 2.UGV02, 3.UGV01
  // moduleType: 0.Null, 1.RoArm, 2.PT
  mm_settings(mainType, moduleType);

  init_oled();
  if (mainType == 1) {
    screenLine_0 = "WAVE ROVER";
  } else if (mainType == 2) {
    screenLine_0 = "UGV";
  } else if (mainType == 3) {
    screenLine_0 = "UGV";
  } 
  
  screenLine_1 = "version: 0.90";
  screenLine_2 = "starting...";
  screenLine_3 = "";
  oled_update();

  delay(1200);

  // functions for IMU.
  imu_init();

  // functions for the leds on ugv.
  led_pin_init();

  // init the littleFS funcs in files_ctrl.h
  screenLine_2 = screenLine_3;
  screenLine_3 = "Initialize LittleFS";
  oled_update();
  if(InfoPrint == 1){Serial.println("Initialize LittleFS for Flash files ctrl.");}
  initFS();

  // init the funcs in switch_module.h
  screenLine_2 = screenLine_3;
  screenLine_3 = "Initialize 12V-switch ctrl";
  oled_update();
  if(InfoPrint == 1){Serial.println("Initialize the pins used for 12V-switch ctrl.");}
  movtionPinInit();

  // servos power up
  screenLine_2 = screenLine_3;
  screenLine_3 = "Power up the servos";
  oled_update();
  if(InfoPrint == 1){Serial.println("Power up the servos.");}
  delay(500);
  
  // init servo ctrl functions.
  screenLine_2 = screenLine_3;
  screenLine_3 = "ServoCtrl init UART2TTL...";
  oled_update();
  if(InfoPrint == 1){Serial.println("ServoCtrl init UART2TTL...");}
  RoArmM2_servoInit();

  // check the status of the servos.
  screenLine_2 = screenLine_3;
  screenLine_3 = "Bus servos status check...";
  oled_update();
  if(InfoPrint == 1){Serial.println("Bus servos status check...");}
  RoArmM2_initCheck(false);

  if(InfoPrint == 1 && RoArmM2_initCheckSucceed){
    Serial.println("All bus servos status checked.");
  }
  if(RoArmM2_initCheckSucceed) {
    screenLine_2 = "Bus servos: succeed";
  } else {
    screenLine_2 = "Bus servos: " + 
    servoFeedback[BASE_SERVO_ID - 11].status +
    servoFeedback[SHOULDER_DRIVING_SERVO_ID - 11].status +
    servoFeedback[SHOULDER_DRIVEN_SERVO_ID - 11].status +
    servoFeedback[ELBOW_SERVO_ID - 11].status +
    servoFeedback[GRIPPER_SERVO_ID - 11].status;
  }
  screenLine_3 = ">>> Moving to init pos...";
  oled_update();
  RoArmM2_resetPID();
  RoArmM2_moveInit();

  screenLine_3 = "Reset joint torque to ST_TORQUE_MAX";
  oled_update();
  if(InfoPrint == 1){Serial.println("Reset joint torque to ST_TORQUE_MAX.");}
  RoArmM2_dynamicAdaptation(0, ST_TORQUE_MAX, ST_TORQUE_MAX, ST_TORQUE_MAX, ST_TORQUE_MAX);

  // WiFi 설정 코드
  screenLine_3 = "WiFi init";
  oled_update();
  if(InfoPrint == 1){Serial.println("WiFi init.");}
  initWifi();
  // 필요한 경우, 초기 WiFi 모드 설정
  // 예: wifiModeAP(ap_ssid, ap_password);
  // 또는: wifiModeSTA(sta_ssid, sta_password);
  // 또는: wifiModeAPSTA(ap_ssid, ap_password, sta_ssid, sta_password);

  screenLine_3 = "http & web init";
  oled_update();
  if(InfoPrint == 1){Serial.println("http & web init.");}
  initHttpWebServer();

  screenLine_3 = "ESP-NOW init";
  oled_update();
  if(InfoPrint == 1){Serial.println("ESP-NOW init.");}
  initEspNow();

  screenLine_3 = "UGV started";
  oled_update();
  if(InfoPrint == 1){Serial.println("UGV started.");}

  getThisDevMacAddress();

  updateOledWifiInfo();

  initEncoders();

  pidControllerInit();

  screenLine_2 = String("MAC:") + macToString(thisDevMac);
  oled_update();

  led_pwm_ctrl(0, 0);

  if(InfoPrint == 1){Serial.println("Application initialization settings.");}
  createMission("boot", "these cmds run automatically at boot.");
  missionPlay("boot", 1);


  // WebSocket 설정
  webSocket.begin("websocket-server.com", 80, "/your-websocket-path");
  webSocket.onEvent(webSocketEvent);
  webSocket.setReconnectInterval(5000);
}

unsigned long lastPing = 0;

void loop() {
  serialCtrl();
  server.handleClient();

  // read and compute the info of joints.
  switch (moduleType) {
  case 1: moduleType_RoArmM2();break;
  case 2: moduleType_Gimbal();break;
  }

  // recv esp-now json cmd.
  if(runNewJsonCmd) {/Users/rojenna/Downloads/WAVE_ROVER_V0.9 2/wifi_ctrl.h
    jsonCmdReceiveHandler();
    jsonCmdReceive.clear();
    runNewJsonCmd = false;
  }

  getLeftSpeed();

  LeftPidControllerCompute();
  
  getRightSpeed();
  
  RightPidControllerCompute();
  
  oledInfoUpdate();

  updateIMUData();

  if (baseFeedbackFlow) {
    baseInfoFeedback();
  }

  heartBeatCtrl();

  size_t freeHeap = esp_get_free_heap_size();

  // WiFi 연결 상태 확인 및 필요시 재연결 (선택적)
  if (WiFi.status() != WL_CONNECTED) {
    // STA 모드에서 연결이 끊어진 경우 재연결 시도
    wifiModeSTA(sta_ssid, sta_password);
  }

  // 주기적인 WiFi 상태 업데이트 (필요한 경우)
  static unsigned long lastWifiCheck = 0;
  if (millis() - lastWifiCheck > 30000) {  // 30초마다 확인
    wifiStatusFeedback();
    updateOledWifiInfo();
    lastWifiCheck = millis();
  }

  webSocket.loop();
  
  // 10초마다 ping 메시지 전송
  if (millis() - lastPing > 10000) {
    webSocket.sendTXT("ping");
    lastPing = millis();
  }
  
}