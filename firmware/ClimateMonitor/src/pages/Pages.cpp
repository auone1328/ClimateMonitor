#include <Arduino.h>

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