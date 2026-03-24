import React, { useState } from "react";
import { View, Text, TextInput, Pressable, StyleSheet, Alert } from "react-native";

export default function LoginScreen({ api, onLoginSuccess, onBack }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  async function submit() {
    if (!email || !password) {
      Alert.alert("Не заполнены поля", "Email и пароль обязательны.");
      return;
    }
    setLoading(true);
    try {
      const res = await api.request("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      if (!res.ok) {
        const text = await res.text();
        Alert.alert("Вход не выполнен", text || res.statusText);
        return;
      }

      const data = await res.json();
      if (!data?.accessToken || !data?.refreshToken) {
        Alert.alert("Вход не выполнен", "Нет токенов в ответе.");
        return;
      }

      await onLoginSuccess({ accessToken: data.accessToken, refreshToken: data.refreshToken });
    } catch (e) {
      Alert.alert("Вход не выполнен", String(e));
    } finally {
      setLoading(false);
    }
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Вход</Text>
      <TextInput
        style={styles.input}
        autoCapitalize="none"
        keyboardType="email-address"
        placeholder="Email"
        value={email}
        onChangeText={setEmail}
      />
      <TextInput
        style={styles.input}
        secureTextEntry
        placeholder="Пароль"
        value={password}
        onChangeText={setPassword}
      />
      <Pressable style={styles.primaryBtn} onPress={submit} disabled={loading}>
        <Text style={styles.primaryBtnText}>{loading ? "Вход..." : "Войти"}</Text>
      </Pressable>
      <Pressable style={styles.linkBtn} onPress={onBack}>
        <Text style={styles.linkText}>Назад</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 24,
    justifyContent: "center",
    backgroundColor: "#f5f7fb",
  },
  title: {
    fontSize: 28,
    marginBottom: 16,
    color: "#172134",
    fontFamily: "serif",
  },
  input: {
    backgroundColor: "#ffffff",
    borderRadius: 10,
    padding: 12,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: "#d6deee",
  },
  primaryBtn: {
    backgroundColor: "#1b4dff",
    padding: 12,
    borderRadius: 10,
    alignItems: "center",
    marginTop: 4,
  },
  primaryBtnText: { color: "#fff", fontSize: 16 },
  linkBtn: { padding: 12, alignItems: "center" },
  linkText: { color: "#1b4dff" },
});
