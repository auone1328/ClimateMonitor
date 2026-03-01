#include <Arduino.h>
#include <WiFi.h>
#include <WiFiClient.h>
#include <qrcode.h>

String getSetupPageHtml() {
    return R"rawliteral(
      <!DOCTYPE html>
      <html>
        <head>
          <meta charset="utf-8"><title>Smart Climate Setup</title>
          <style>
            body {  
              font-family: sans-serif;
              max-width: 500px;
              margin: 40px auto;
              padding: 20px;
            }
            input, button {
              display: block;
              width: 100%;
              margin: 10px 0;
              padding: 10px;
            } 
          </style>
        </head>
        <body>
          <h1>Настройка устройства</h1>
          <form action="/save" method="POST">
            <label>Wi-Fi SSID:</label><input name="ssid" required>
            <label>Wi-Fi пароль:</label><input type="password" name="pass">
            <label>Название здания:</label><input name="building" required placeholder="например: ул. Мира, 29">
            <label>Название комнаты:</label><input name="room" required placeholder="например: Room 101">
            <button type="submit">Сохранить и перезагрузить</button>
          </form>
        </body>
      </html>
      )rawliteral";
}

String getQrPageHtml(const String& qrData) {
    return R"rawliteral(
      <!DOCTYPE html>
      <html lang="ru">
      <head>
          <meta charset="utf-8">
          <title>Настройка завершена</title>
          <style>
              body { font-family: Arial, sans-serif; text-align: center; padding: 40px; background: #f8f9fa; }
              h1 { color: #007bff; }
              svg { margin: 30px auto; display: block; border: 10px solid white; box-shadow: 0 0 20px rgba(0,0,0,0.2); }
              p { font-size: 1.2em; }
          </style>
      </head>
      <body>
          <h1>Устройство настроено!</h1>
          <p>Отсканируйте QR-код в мобильном приложении:</p>
      )rawliteral" + qrData + R"rawliteral(
          <p><small>MAC: )rawliteral" + WiFi.macAddress() + R"rawliteral(</small></p>
      </body>
      </html>
      )rawliteral";
}

String getQrSvg(const String& text) {
    QRCode qrcode;
    uint8_t qrcodeData[qrcode_getBufferSize(3)];
    uint8_t err = qrcode_initText(&qrcode, qrcodeData, 3, 0, text.c_str());

    if (err != 0) {
        return "<p>Ошибка генерации QR: " + String(err) + "</p>";
    }

    String svg = "<svg xmlns='http://www.w3.org/2000/svg' version='1.1' width='300' height='300' viewBox='0 0 " 
                 + String(qrcode.size) + " " + String(qrcode.size) + "'>";
    svg += "<rect width='100%' height='100%' fill='white'/>";

    for (uint8_t y = 0; y < qrcode.size; y++) {
        for (uint8_t x = 0; x < qrcode.size; x++) {
            if (qrcode_getModule(&qrcode, x, y)) {
                svg += "<rect x='" + String(x) + "' y='" + String(y) + "' width='1' height='1' fill='black'/>";
            }
        }
    }
    svg += "</svg>";

    return svg;
}