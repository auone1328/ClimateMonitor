import paho.mqtt.client as mqtt

TOPIC = "cs2026_climate_7A3B9F2C/#"  

def on_message(client, userdata, msg):
    print(f"✓ Получено: {msg.topic} -> {msg.payload.decode()}")

client = mqtt.Client()
client.connect("test.mosquitto.org", 1883)
client.subscribe(TOPIC)
client.on_message = on_message

print(f"Слушаю топик: {TOPIC}")
print("Ожидаю данные от ESP32...")
client.loop_forever()