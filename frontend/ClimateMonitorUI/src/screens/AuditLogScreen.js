import React, { useEffect, useState } from "react";
import { View, Text, FlatList, Pressable, StyleSheet, Alert } from "react-native";

const actionMap = {
  SetTemperature: "Установка температуры",
  ToggleRelay: "Переключение реле",
  ToggleCooler: "Переключение охладителя",
  RegisterDevice: "Регистрация устройства",
  UserLogin: "Вход пользователя",
  UpdateSchedule: "Обновление расписания",
  Other: "Другое",
};

export default function AuditLogScreen({ api, building, onBack }) {
  const [items, setItems] = useState([]);
  const [window, setWindow] = useState("7d");
  const [loading, setLoading] = useState(false);

  function buildRange() {
    const now = new Date();
    let from;
    if (window === "24h") from = new Date(now.getTime() - 24 * 60 * 60 * 1000);
    else from = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    return { fromUtc: from.toISOString(), toUtc: now.toISOString() };
  }

  async function load() {
    setLoading(true);
    try {
      const { fromUtc, toUtc } = buildRange();
      const res = await api.request(`/api/buildings/${building.id}/audit?fromUtc=${encodeURIComponent(fromUtc)}&toUtc=${encodeURIComponent(toUtc)}`);
      if (!res.ok) {
        const text = await res.text();
        Alert.alert("Загрузка не удалась", text || res.statusText);
        return;
      }
      const data = await res.json();
      setItems(Array.isArray(data) ? data : []);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, [building.id, window]);

  return (
    <View style={styles.container}>
      <Pressable onPress={onBack}>
        <Text style={styles.link}>Назад</Text>
      </Pressable>
      <Text style={styles.title}>Журнал действий</Text>

      <View style={styles.windowRow}>
        {[
          { key: "24h", label: "24ч" },
          { key: "7d", label: "7д" },
        ].map((w) => (
          <Pressable
            key={w.key}
            style={[styles.windowBtn, window === w.key && styles.windowBtnActive]}
            onPress={() => setWindow(w.key)}
          >
            <Text style={[styles.windowText, window === w.key && styles.windowTextActive]}>{w.label}</Text>
          </Pressable>
        ))}
      </View>

      <FlatList
        data={items}
        keyExtractor={(item) => item.id}
        refreshing={loading}
        onRefresh={load}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <Text style={styles.metric}>
              Пользователь: {item.userName || item.userEmail || item.userId}
            </Text>
            <Text style={styles.metric}>{actionMap[item.actionType] || item.actionType}</Text>
            <Text style={styles.metric}>{item.details}</Text>
            {item.roomName && <Text style={styles.metric}>Комната: {item.roomName}</Text>}
            {!item.roomName && item.roomId && <Text style={styles.metric}>Комната: {item.roomId}</Text>}
            {item.deviceMac && <Text style={styles.metric}>Устройство: {item.deviceMac}</Text>}
            {!item.deviceMac && item.deviceId && <Text style={styles.metric}>Устройство: {item.deviceId}</Text>}
            <Text style={styles.metric}>{new Date(item.timestamp).toLocaleString()}</Text>
          </View>
        )}
        ListEmptyComponent={<Text style={styles.empty}>Журнал пуст.</Text>}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fb", padding: 16 },
  title: { fontSize: 22, color: "#172134", marginTop: 8, fontFamily: "serif" },
  link: { color: "#1b4dff" },
  windowRow: { flexDirection: "row", marginVertical: 12 },
  windowBtn: {
    paddingVertical: 6,
    paddingHorizontal: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#1b4dff",
    marginRight: 8,
  },
  windowBtnActive: { backgroundColor: "#1b4dff" },
  windowText: { color: "#1b4dff" },
  windowTextActive: { color: "#fff" },
  card: {
    backgroundColor: "#fff",
    padding: 12,
    borderRadius: 12,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: "#d6deee",
  },
  metric: { color: "#41536b" },
  empty: { color: "#667792", textAlign: "center", marginTop: 24 },
});
