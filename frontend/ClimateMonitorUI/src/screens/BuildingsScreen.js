import React, { useEffect, useState } from "react";
import { View, Text, FlatList, Pressable, StyleSheet, Alert } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

export default function BuildingsScreen({ api, onOpenBuilding, onOpenNotifications, onOpenAcceptInvite, onLogout }) {
  const [buildings, setBuildings] = useState([]);
  const [loading, setLoading] = useState(false);

  async function load() {
    setLoading(true);
    try {
      const res = await api.request("/api/buildings");
      if (!res.ok) {
        const text = await api.readError(res);
        Alert.alert("Загрузка не удалась", text);
        return;
      }
      const data = await res.json();
      setBuildings(Array.isArray(data) ? data : []);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Здания</Text>
        <View style={styles.headerActions}>
          <Pressable style={styles.headerBtn} onPress={onOpenNotifications}>
            <Text style={styles.headerBtnText}>Уведомления</Text>
          </Pressable>
          <Pressable style={styles.headerBtn} onPress={onOpenAcceptInvite}>
            <Text style={styles.headerBtnText}>Принять приглашение</Text>
          </Pressable>
          <Pressable style={[styles.headerBtn, styles.headerBtnDanger]} onPress={onLogout}>
            <Text style={[styles.headerBtnText, styles.headerBtnDangerText]}>Выйти</Text>
          </Pressable>
        </View>
      </View>
      <FlatList
        data={buildings}
        keyExtractor={(item) => item.id}
        refreshing={loading}
        onRefresh={load}
        renderItem={({ item }) => (
          <Pressable style={styles.card} onPress={() => onOpenBuilding(item)}>
            <Text style={styles.cardTitle}>{item.name}</Text>
          </Pressable>
        )}
        ListEmptyComponent={<Text style={styles.empty}>Зданий пока нет.</Text>}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fb", padding: 16 },
  header: { marginBottom: 12 },
  title: { fontSize: 26, color: "#172134", fontFamily: "serif" },
  headerActions: { flexDirection: "row", flexWrap: "wrap", marginTop: 8 },
  headerBtn: {
    paddingVertical: 6,
    paddingHorizontal: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#1b4dff",
    backgroundColor: "#ffffff",
    marginRight: 8,
    marginBottom: 8,
  },
  headerBtnText: { color: "#1b4dff" },
  headerBtnDanger: { borderColor: "#d43b3b" },
  headerBtnDangerText: { color: "#d43b3b" },
  card: {
    backgroundColor: "#fff",
    padding: 14,
    borderRadius: 12,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: "#d6deee",
  },
  cardTitle: { fontSize: 18, color: "#172134" },
  cardSub: { color: "#667792", marginTop: 4 },
  empty: { color: "#667792", textAlign: "center", marginTop: 24 },
});
