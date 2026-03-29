import React, { useEffect, useState } from "react";
import { View, Text, FlatList, Pressable, StyleSheet, Alert } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

export default function NotificationsScreen({ api, onBack }) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);

  async function load() {
    setLoading(true);
    try {
      const res = await api.request("/api/notifications");
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
  }, []);

  async function markRead(id) {
    const res = await api.request(`/api/notifications/${id}/read`, { method: "PATCH" });
    if (res.ok) load();
  }

  return (
    <SafeAreaView style={styles.container}>
      <Pressable onPress={onBack}>
        <Text style={styles.link}>Назад</Text>
      </Pressable>
      <Text style={styles.title}>Уведомления</Text>
      <FlatList
        data={items}
        keyExtractor={(item) => item.id}
        refreshing={loading}
        onRefresh={load}
        renderItem={({ item }) => (
          <Pressable style={[styles.card, item.isRead && styles.readCard]} onPress={() => markRead(item.id)}>
            <Text style={styles.cardTitle}>{item.message}</Text>
            <Text style={styles.cardSub}>{item.type}</Text>
            <Text style={styles.cardSub}>{new Date(item.createdAt).toLocaleString()}</Text>
          </Pressable>
        )}
        ListEmptyComponent={<Text style={styles.empty}>Уведомлений нет.</Text>}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fb", padding: 16 },
  title: { fontSize: 24, color: "#172134", marginTop: 8, fontFamily: "serif" },
  link: { color: "#1b4dff" },
  card: {
    backgroundColor: "#fff",
    padding: 14,
    borderRadius: 12,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: "#d6deee",
  },
  readCard: { opacity: 0.6 },
  cardTitle: { fontSize: 16, color: "#172134" },
  cardSub: { color: "#667792", marginTop: 4 },
  empty: { color: "#667792", textAlign: "center", marginTop: 24 },
});
