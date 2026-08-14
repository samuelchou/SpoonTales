const int sensorPin = A0;
const bool isDebug = false;
const int interval = 500;

// 定義查表點位
// 數據點越多越精準，此處用 4 個點
const int NUM_POINTS = 5;
// ADC數值由大到小排列 (對應 2cm -> 15cm)
const int adcTable[NUM_POINTS]   = {430, 215, 164, 123, 82};
// 對應距離 (單位：mm，使用整數避免浮點數運算)
const int distTable[NUM_POINTS]  = {20,  50, 70, 100, 150};

// 分段線性內插函式
int getDistanceMm(int rawAdc) {
  // 範圍檢查：超過邊界直接回傳極限值或錯誤碼
  if (rawAdc >= adcTable[0]) return distTable[0];             // < 2cm (盲區)
  if (rawAdc <= adcTable[NUM_POINTS - 1]) return 999;          // > 15cm (超出範圍)

  // 尋找 rawAdc 落在哪個區間
  for (int i = 0; i < NUM_POINTS - 1; i++) {
    if (rawAdc <= adcTable[i] && rawAdc > adcTable[i+1]) {
      // 在區間內使用 Arduino 內建 map 進行線性內插
      // map(value, fromLow, fromHigh, toLow, toHigh)
      return map(rawAdc, adcTable[i], adcTable[i+1], distTable[i], distTable[i+1]);
    }
  }
  return 999;
}

void setup() {
  Serial.begin(9600);
  Serial.println("Setup");
}

void loop() {
  // 1. 移動平均濾波（連續讀取 10 次取平均，避免訊號突波）
  int totalAdc = 0;
  for (int i = 0; i < 10; i++) {
    totalAdc += analogRead(sensorPin);
    delay(2);
  }
  int avgAdc = int(totalAdc / 10.0);

  // 3. 將電壓換算為距離 (cm)
  // 注意：僅在 2 cm ~ 15 cm 的有效範圍內準確
  float distanceMm = getDistanceMm(avgAdc);
  if (isDebug) {
    Serial.print("ADC value: ");
    Serial.println(avgAdc);
    Serial.print("distance: ");
    Serial.print(distanceMm);
    Serial.println(" mm");
  }
  else {
    Serial.println(distanceMm);
  }
  delay(interval);
}
