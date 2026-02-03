#include "Pages.h"

void sendWelcomePageHtml() {
    String response = R"(
    <!DOCTYPE html><html>
      <head>
        <title>Welcome</title>
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <style>
        </style>
      </head>
            
      <body>
        <h1>Welcome</h1>
      </body>
    </html>
  )";
  server.send(200, "text/html", response);
}