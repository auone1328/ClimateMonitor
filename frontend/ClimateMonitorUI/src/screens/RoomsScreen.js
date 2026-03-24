import React, { useEffect, useState } from "react";
import { View, Text, FlatList, Pressable, StyleSheet, Alert } from "react-native";

export default function RoomsScreen({ api, building, onOpenRoom, onOpenInvite, onOpenAudit, onOpenUsers, onBack }) {
  const [rooms, setRooms] = useState([]);
  const [role, setRole] = useState(null);
  const [loading, setLoading] = useState(false);

  async function load() {
    setLoading(true);
    try {
      const [roomsRes, roleRes] = await Promise.all([
        api.request(`/api/buildings/${building.id}/rooms`),
        api.request(`/api/buildings/${building.id}/role`),
      ]);

      if (roomsRes.ok) {
        const roomsData = await roomsRes.json();
        setRooms(Array.isArray(roomsData) ? roomsData : []);
      }

      if (roleRes.ok) {
        const roleData = await roleRes.json();
        setRole(roleData?.role || null);
      }
    } catch (e) {
      Alert.alert("Загрузка не удалась", String(e));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, [building.id]);

  const isAdmin = role === "Admin";

  return (
    <View style={styles.container}>
      <Pressable onPress={onBack}>
        <Text style={styles.link}>Назад</Text>
      </Pressable>
      <Text style={styles.title}>{building.name}</Text>
      <Text style={styles.sub}>Роль: {role || "Неизвестно"}</Text>

      <View style={styles.actions}>
        {isAdmin && (
          <Pressable style={styles.actionBtn} onPress={() => onOpenInvite(role)}>
            <Text style={styles.actionText}>Создать приглашение</Text>
          </Pressable>
        )}
        {isAdmin && (
          <Pressable style={styles.actionBtn} onPress={onOpenUsers}>
            <Text style={styles.actionText}>Пользователи</Text>
          </Pressable>
        )}
        {isAdmin && (
          <Pressable style={styles.actionBtn} onPress={onOpenAudit}>
            <Text style={styles.actionText}>Журнал действий</Text>
          </Pressable>
        )}
      </View>

      <FlatList
        data={rooms}
        keyExtractor={(item) => item.id}
        refreshing={loading}
        onRefresh={load}
        renderItem={({ item }) => (
          <Pressable style={styles.card} onPress={() => onOpenRoom(item, role)}>
            <Text style={styles.cardTitle}>{item.name}</Text>
            <Text style={styles.cardSub}>Цель: {item.targetTemperature}°C</Text>
          </Pressable>
        )}
        ListEmptyComponent={<Text style={styles.empty}>Комнат пока нет.</Text>}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fb", padding: 16 },
  title: { fontSize: 24, color: "#172134", marginTop: 8, fontFamily: "serif" },
  sub: { color: "#667792", marginBottom: 12 },
  link: { color: "#1b4dff" },
  actions: { flexDirection: "row", flexWrap: "wrap", marginBottom: 8 },
  actionBtn: {
    paddingVertical: 6,
    paddingHorizontal: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#1b4dff",
    marginRight: 8,
    marginBottom: 8,
  },
  actionText: { color: "#1b4dff" },
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
