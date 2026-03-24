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
            <label>Wi-Fi password:</label><input type="password" name="pass">
            <label>Название здания:</label><input name="building" required placeholder="Например: ул.Мира, 29">
            <label>Название комнаты:</label><input name="room" required placeholder="Например: Room 101">
            <button type="submit">Сохранить и показать QR</button>
          </form>
        </body>
      </html>
      )rawliteral";
}

String getQrPageHtml(const String& qrPayload) {
    return R"rawliteral(
      <!DOCTYPE html>
      <html lang="en">
      <head>
          <meta charset="utf-8">
          <title>Setup Complete</title>
          <style>
              body { font-family: Arial, sans-serif; text-align: center; padding: 40px; background: #f8f9fa; }
              h1 { color: #007bff; }
              img { margin: 30px auto; display: block; border: 10px solid white; box-shadow: 0 0 20px rgba(0,0,0,0.2); }
              p { font-size: 1.1em; }
              textarea { width: 100%; height: 120px; }
              .box { max-width: 320px; margin: 0 auto; text-align: left; }
          </style>
      </head>
      <body>
          <h1>Настройка завершена!</h1>
          <p>Отсканируйте QR код в мобильном приложении:</p>
          <img src="/qr.svg" width="300" height="300" alt="QR" />
          <p><small>MAC: )rawliteral" + WiFi.macAddress() + R"rawliteral(</small></p>
          <form action="/finish" method="POST">
            <button type="submit" style="margin-top:12px;padding:10px 16px;">Завершить и перезапустить</button>
          </form>
      </body>
      </html>
      )rawliteral";
}

String getQrSvg(const String& text) {
    QRCode qrcode;
    const uint8_t version = 7; // higher capacity for UTF-8 payloads
    const uint8_t ecc = 1;     // medium error correction
    uint8_t qrcodeData[qrcode_getBufferSize(version)];
    uint8_t err = qrcode_initText(&qrcode, qrcodeData, version, ecc, text.c_str());

    if (err != 0) {
        return "<p>QR generation error</p>";
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
