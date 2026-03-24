import React, { useEffect, useState } from "react";
import { View, Text, FlatList, Pressable, StyleSheet, Alert } from "react-native";

export default function BuildingUsersScreen({ api, building, onBack }) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);

  async function load() {
    setLoading(true);
    try {
      const res = await api.request(`/api/buildings/${building.id}/users`);
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
  }, [building.id]);

  async function setRole(userId, role) {
    const res = await api.request(`/api/buildings/${building.id}/users/${userId}/role`, {
      method: "PATCH",
      body: JSON.stringify({ role }),
    });

    if (!res.ok) {
      const text = await res.text();
      Alert.alert("Не удалось обновить роль", text || res.statusText);
      return;
    }

    await load();
  }

  async function removeUser(userId) {
    const res = await api.request(`/api/buildings/${building.id}/users/${userId}`, {
      method: "DELETE",
    });

    if (!res.ok) {
      const text = await res.text();
      Alert.alert("Не удалось удалить", text || res.statusText);
      return;
    }

    await load();
  }

  function roleLabel(role) {
    if (role === "Admin") return "Администратор";
    if (role === "Observer") return "Наблюдатель";
    return "Пользователь";
  }

  return (
    <View style={styles.container}>
      <Pressable onPress={onBack}>
        <Text style={styles.link}>Назад</Text>
      </Pressable>
      <Text style={styles.title}>Пользователи: {building.name}</Text>

      <FlatList
        data={items}
        keyExtractor={(item) => item.userId}
        refreshing={loading}
        onRefresh={load}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <Text style={styles.cardTitle}>{item.userName || item.email}</Text>
            <Text style={styles.cardSub}>{item.email}</Text>
            <Text style={styles.cardSub}>Роль: {roleLabel(item.role)}</Text>

            <View style={styles.actions}>
              <Pressable
                style={styles.actionBtn}
                onPress={() => setRole(item.userId, "User")}
                disabled={item.role === "Admin"}
              >
                <Text style={styles.actionText}>Сделать пользователем</Text>
              </Pressable>
              <Pressable
                style={styles.actionBtn}
                onPress={() => setRole(item.userId, "Observer")}
                disabled={item.role === "Admin"}
              >
                <Text style={styles.actionText}>Сделать наблюдателем</Text>
              </Pressable>
              <Pressable
                style={[styles.actionBtn, styles.dangerBtn]}
                onPress={() => removeUser(item.userId)}
                disabled={item.role === "Admin"}
              >
                <Text style={[styles.actionText, styles.dangerText]}>Удалить</Text>
              </Pressable>
            </View>
          </View>
        )}
        ListEmptyComponent={<Text style={styles.empty}>Пользователей пока нет.</Text>}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fb", padding: 16 },
  title: { fontSize: 22, color: "#172134", marginTop: 8, fontFamily: "serif" },
  link: { color: "#1b4dff" },
  card: {
    backgroundColor: "#fff",
    padding: 12,
    borderRadius: 12,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: "#d6deee",
  },
  cardTitle: { fontSize: 16, color: "#172134" },
  cardSub: { color: "#667792", marginTop: 4 },
  actions: { marginTop: 8 },
  actionBtn: {
    paddingVertical: 6,
    paddingHorizontal: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#1b4dff",
    marginBottom: 6,
  },
  actionText: { color: "#1b4dff" },
  dangerBtn: { borderColor: "#d43b3b" },
  dangerText: { color: "#d43b3b" },
  empty: { color: "#667792", textAlign: "center", marginTop: 24 },
});
