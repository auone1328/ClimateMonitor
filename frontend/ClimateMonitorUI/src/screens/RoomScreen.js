import React, { useEffect, useState } from "react";
import { View, Text, Pressable, StyleSheet, TextInput, Alert, Switch } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

export default function RoomScreen({ api, room, role, onBack, onOpenHistory }) {
  const [measurement, setMeasurement] = useState(null);
  const [devices, setDevices] = useState([]);
  const [targetTemp, setTargetTemp] = useState(String(room.targetTemperature ?? ""));
  const [loading, setLoading] = useState(false);

  const canControl = role === "Admin" || role === "User";

  async function load() {
    setLoading(true);
    try {
      const [mRes, dRes] = await Promise.all([
        api.request(`/api/rooms/${room.id}/measurements/latest`),
        api.request(`/api/rooms/${room.id}/devices`),
      ]);

      if (mRes.ok) {
        const data = await mRes.json();
        setMeasurement(data || null);
      }

      if (dRes.ok) {
        const data = await dRes.json();
        setDevices(Array.isArray(data) ? data : []);
      } else {
        const text = await api.readError(dRes);
        Alert.alert("Не удалось загрузить устройства", text);
      }
    } finally {
      setLoading(false);
    }
  }

  function getCo2Value(source) {
    if (!source) return null;
    if (Number.isFinite(source.co2)) return source.co2;
    if (Number.isFinite(source.CO2)) return source.CO2;
    if (Number.isFinite(source.cO2)) return source.cO2;
    return null;
  }

  useEffect(() => {
    load();
    const id = setInterval(load, 5000);
    return () => clearInterval(id);
  }, [room.id]);

  async function setTarget() {
    if (!canControl) return;
    const value = Number(targetTemp);
    if (Number.isNaN(value)) {
      Alert.alert("Неверная температура", "Введите числовое значение.");
      return;
    }

    const res = await api.request(`/api/rooms/${room.id}/target-temperature`, {
      method: "PATCH",
      body: JSON.stringify({ targetTemperature: value }),
    });

    if (!res.ok) {
      const text = await api.readError(res);
      Alert.alert("Не удалось обновить", text);
      return;
    }

    Alert.alert("Обновлено", "Целевая температура сохранена.");
  }

  async function toggleDevice(deviceId, field, value) {
    if (!canControl) return;

    const path = field === "cooler" ? `/api/devices/${deviceId}/cooler` : `/api/devices/${deviceId}/relay`;
    const body = field === "cooler" ? { coolerState: value } : { relayState: value };

    setDevices((prev) =>
      prev.map((d) => {
        if (d.id !== deviceId) return d;
        if (field === "cooler") return { ...d, coolerState: value };
        return { ...d, relayState: value, heaterState: value };
      })
    );

    const res = await api.request(path, {
      method: "POST",
      body: JSON.stringify(body),
    });

    if (!res.ok) {
      const text = await api.readError(res);
      Alert.alert("Не удалось обновить", text);
      await load();
      return;
    }

    await load();
  }

  return (
    <SafeAreaView style={styles.container}>
      <Pressable onPress={onBack}>
        <Text style={styles.link}>Назад</Text>
      </Pressable>
      <Text style={styles.title}>{room.name}</Text>
      <Text style={styles.sub}>Роль: {role || "Неизвестно"}</Text>

      <View style={styles.actions}>
        <Pressable style={styles.actionBtn} onPress={onOpenHistory}>
          <Text style={styles.actionText}>История</Text>
        </Pressable>
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Последние измерения</Text>
        {measurement ? (
          <View>
            <Text style={styles.metric}>Температура: {measurement.temperature}°C</Text>
            <Text style={styles.metric}>Влажность: {measurement.humidity}%</Text>
            <Text style={styles.metric}>
              CO2: {getCo2Value(measurement) ?? "нет данных"}
            </Text>
            <Text style={styles.metric}>Время: {new Date(measurement.timestamp).toLocaleString()}</Text>
          </View>
        ) : (
          <Text style={styles.metric}>Данных нет.</Text>
        )}
        <Pressable style={styles.secondaryBtn} onPress={load}>
          <Text style={styles.secondaryBtnText}>{loading ? "Обновление..." : "Обновить"}</Text>
        </Pressable>
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Целевая температура</Text>
        <TextInput
          style={[styles.input, !canControl && styles.inputDisabled]}
          editable={canControl}
          value={targetTemp}
          onChangeText={setTargetTemp}
          keyboardType="numeric"
        />
        <Pressable style={[styles.primaryBtn, !canControl && styles.btnDisabled]} onPress={setTarget}>
          <Text style={styles.primaryBtnText}>Сохранить</Text>
        </Pressable>
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Устройства</Text>
        {devices.map((d) => (
          <View key={d.id} style={styles.deviceCard}>
            <Text style={styles.deviceTitle}>Устройство {d.macAddress}</Text>
            <View style={styles.toggleRow}>
              <Text style={styles.toggleLabel}>Охладитель</Text>
              <Switch
                value={!!d.coolerState}
                onValueChange={(v) => toggleDevice(d.id, "cooler", v)}
                disabled={!canControl}
              />
            </View>
            <View style={styles.toggleRow}>
              <Text style={styles.toggleLabel}>Обогреватель</Text>
              <Switch
                value={!!d.relayState}
                onValueChange={(v) => toggleDevice(d.id, "relay", v)}
                disabled={!canControl}
              />
            </View>
          </View>
        ))}
        {devices.length === 0 && <Text style={styles.metric}>Устройств нет.</Text>}
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fb", padding: 16 },
  title: { fontSize: 24, color: "#172134", marginTop: 8, fontFamily: "serif" },
  sub: { color: "#667792", marginBottom: 8 },
  link: { color: "#1b4dff" },
  actions: { flexDirection: "row", marginBottom: 8 },
  actionBtn: {
    paddingVertical: 6,
    paddingHorizontal: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#1b4dff",
    marginRight: 8,
  },
  actionText: { color: "#1b4dff" },
  section: {
    backgroundColor: "#fff",
    padding: 14,
    borderRadius: 12,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: "#d6deee",
  },
  sectionTitle: { fontSize: 16, color: "#172134", marginBottom: 6 },
  metric: { color: "#41536b", marginBottom: 4 },
  input: {
    backgroundColor: "#ffffff",
    borderRadius: 10,
    padding: 10,
    borderWidth: 1,
    borderColor: "#d6deee",
    marginBottom: 8,
  },
  inputDisabled: { opacity: 0.6 },
  primaryBtn: {
    backgroundColor: "#1b4dff",
    padding: 10,
    borderRadius: 10,
    alignItems: "center",
  },
  primaryBtnText: { color: "#fff" },
  secondaryBtn: {
    marginTop: 8,
    alignSelf: "flex-start",
    paddingVertical: 6,
    paddingHorizontal: 10,
    backgroundColor: "#e6ecff",
    borderRadius: 8,
  },
  secondaryBtnText: { color: "#1b4dff" },
  btnDisabled: { opacity: 0.6 },
  deviceCard: {
    borderTopWidth: 1,
    borderColor: "#eef2f8",
    paddingTop: 8,
    marginTop: 8,
  },
  deviceTitle: { color: "#172134", marginBottom: 4 },
  toggleRow: { flexDirection: "row", justifyContent: "space-between", alignItems: "center", marginBottom: 6 },
  toggleLabel: { color: "#41536b" },
});
