import React, { useState } from "react";
import { View, Text, TextInput, Pressable, StyleSheet, Alert } from "react-native";
import QRCode from "react-native-qrcode-svg";

const roles = [
  { key: "Admin", label: "Администратор", value: 0 },
  { key: "User", label: "Пользователь", value: 1 },
  { key: "Observer", label: "Наблюдатель", value: 2 },
];

export default function CreateInviteScreen({ api, building, onBack }) {
  const [roleValue, setRoleValue] = useState(1);
  const [expiresInDays, setExpiresInDays] = useState("7");
  const [token, setToken] = useState(null);
  const [loading, setLoading] = useState(false);

  async function submit() {
    const days = Number(expiresInDays);
    if (Number.isNaN(days) || days <= 0) {
      Alert.alert("Некорректно", "Срок должен быть положительным числом.");
      return;
    }

    setLoading(true);
    try {
      const res = await api.request(`/api/buildings/${building.id}/invites`, {
        method: "POST",
        body: JSON.stringify({ role: roleValue, expiresInDays: days }),
      });
      if (!res.ok) {
        const text = await res.text();
        Alert.alert("Создать не удалось", text || res.statusText);
        return;
      }
      const data = await res.json();
      setToken(data?.token || null);
    } finally {
      setLoading(false);
    }
  }

  return (
    <View style={styles.container}>
      <Pressable onPress={onBack}>
        <Text style={styles.link}>Назад</Text>
      </Pressable>
      <Text style={styles.title}>Создать приглашение</Text>
      <Text style={styles.sub}>{building.name}</Text>

      <Text style={styles.label}>Роль</Text>
      <View style={styles.roleRow}>
        {roles.map((r) => (
          <Pressable
            key={r.key}
            style={[styles.roleBtn, roleValue === r.value && styles.roleBtnActive]}
            onPress={() => setRoleValue(r.value)}
          >
            <Text style={[styles.roleText, roleValue === r.value && styles.roleTextActive]}>{r.label}</Text>
          </Pressable>
        ))}
      </View>

      <Text style={styles.label}>Срок (дни)</Text>
      <TextInput
        style={styles.input}
        keyboardType="numeric"
        value={expiresInDays}
        onChangeText={setExpiresInDays}
      />

      <Pressable style={styles.primaryBtn} onPress={submit} disabled={loading}>
        <Text style={styles.primaryBtnText}>{loading ? "Создание..." : "Создать приглашение"}</Text>
      </Pressable>

      {token && (
        <View style={styles.qrCard}>
          <Text style={styles.label}>Токен приглашения</Text>
          <Text selectable style={styles.tokenText}>{token}</Text>
          <QRCode value={token} size={200} />
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fb", padding: 16 },
  title: { fontSize: 22, color: "#172134", marginTop: 8, fontFamily: "serif" },
  sub: { color: "#667792", marginBottom: 12 },
  link: { color: "#1b4dff" },
  label: { color: "#41536b", marginTop: 12, marginBottom: 6 },
  roleRow: { flexDirection: "row", marginBottom: 8 },
  roleBtn: {
    paddingVertical: 6,
    paddingHorizontal: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#1b4dff",
    marginRight: 8,
  },
  roleBtnActive: { backgroundColor: "#1b4dff" },
  roleText: { color: "#1b4dff" },
  roleTextActive: { color: "#fff" },
  input: {
    backgroundColor: "#ffffff",
    borderRadius: 10,
    padding: 10,
    borderWidth: 1,
    borderColor: "#d6deee",
  },
  primaryBtn: {
    backgroundColor: "#1b4dff",
    padding: 10,
    borderRadius: 10,
    alignItems: "center",
    marginTop: 12,
  },
  primaryBtnText: { color: "#fff" },
  qrCard: {
    marginTop: 16,
    alignItems: "center",
    backgroundColor: "#fff",
    padding: 12,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: "#d6deee",
  },
  tokenText: { color: "#172134", marginBottom: 12 },
});
