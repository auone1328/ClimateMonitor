import React, { useEffect, useMemo, useState } from "react";
import { View, Text, FlatList, Pressable, StyleSheet, Alert } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import SimpleLineChart from "./SimpleLineChart";

export default function MeasurementsScreen({ api, room, onBack }) {
  const [items, setItems] = useState([]);
  const [window, setWindow] = useState("24h");
  const [loading, setLoading] = useState(false);

  function buildRange() {
    const now = new Date();
    let from;
    if (window === "1h") from = new Date(now.getTime() - 60 * 60 * 1000);
    else if (window === "7d") from = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    else from = new Date(now.getTime() - 24 * 60 * 60 * 1000);
    return { fromUtc: from.toISOString(), toUtc: now.toISOString() };
  }

  async function load() {
    setLoading(true);
    try {
      const { fromUtc, toUtc } = buildRange();
      const res = await api.request(`/api/rooms/${room.id}/measurements?fromUtc=${encodeURIComponent(fromUtc)}&toUtc=${encodeURIComponent(toUtc)}`);
      if (!res.ok) {
        const text = await api.readError(res);
        Alert.alert("Загрузка не удалась", text);
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
  }, [room.id, window]);

  const tempSeries = useMemo(() => {
    const list = [...items].reverse();
    return list.map((m) => ({ value: m.temperature, ts: m.timestamp }));
  }, [items]);

  function getCo2Value(source) {
    if (!source) return null;
    if (Number.isFinite(source.co2)) return source.co2;
    if (Number.isFinite(source.CO2)) return source.CO2;
    if (Number.isFinite(source.cO2)) return source.cO2;
    return null;
  }

  const humiditySeries = useMemo(() => {
    const list = [...items].reverse();
    return list.map((m) => ({ value: m.humidity, ts: m.timestamp }));
  }, [items]);

  const co2Series = useMemo(() => {
    const list = [...items].reverse();
    return list.map((m) => ({ value: getCo2Value(m), ts: m.timestamp }));
  }, [items]);

  return (
    <SafeAreaView style={styles.container}>
      <Pressable onPress={onBack}>
        <Text style={styles.link}>Назад</Text>
      </Pressable>
      <Text style={styles.title}>История: {room.name}</Text>

      <View style={styles.windowRow}>
        {[
          { key: "1h", label: "1ч" },
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

      <View style={styles.chartCard}>
        <Text style={styles.sectionTitle}>График температуры</Text>
        <SimpleLineChart data={tempSeries} />
      </View>

      <View style={styles.chartCard}>
        <Text style={styles.sectionTitle}>График влажности</Text>
        <SimpleLineChart data={humiditySeries} />
      </View>

      <View style={styles.chartCard}>
        <Text style={styles.sectionTitle}>График CO2</Text>
        <SimpleLineChart data={co2Series} />
      </View>

      <FlatList
        data={items}
        keyExtractor={(item) => item.id}
        refreshing={loading}
        onRefresh={load}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <Text style={styles.metric}>Температура: {item.temperature}°C</Text>
            <Text style={styles.metric}>Влажность: {item.humidity}%</Text>
            <Text style={styles.metric}>
              CO2: {getCo2Value(item) ?? "нет данных"}
            </Text>
            <Text style={styles.metric}>{new Date(item.timestamp).toLocaleString()}</Text>
          </View>
        )}
        ListEmptyComponent={<Text style={styles.empty}>Нет измерений.</Text>}
      />
    </SafeAreaView>
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
  chartCard: {
    backgroundColor: "#fff",
    padding: 12,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: "#d6deee",
    marginBottom: 12,
  },
  sectionTitle: { color: "#172134", marginBottom: 6 },
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
