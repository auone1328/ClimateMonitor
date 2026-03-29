import React from "react";
import { View, Text, Pressable, StyleSheet } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

export default function WelcomeScreen({ onLogin, onRegisterAdmin, onAcceptInvite }) {
  return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.title}>Умный климат</Text>
      <Text style={styles.sub}>Централизованное управление микроклиматом</Text>
      <Pressable style={styles.primaryBtn} onPress={onLogin}>
        <Text style={styles.primaryBtnText}>Войти</Text>
      </Pressable>
      <Pressable style={styles.secondaryBtn} onPress={onRegisterAdmin}>
        <Text style={styles.secondaryBtnText}>Регистрация по QR устройства</Text>
      </Pressable>
      <Pressable style={styles.secondaryBtn} onPress={onAcceptInvite}>
        <Text style={styles.secondaryBtnText}>Принять приглашение</Text>
      </Pressable>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 24,
    justifyContent: "center",
    backgroundColor: "#f5f7fb",
  },
  title: { fontSize: 32, color: "#172134", fontFamily: "serif" },
  sub: { color: "#667792", marginBottom: 24 },
  primaryBtn: {
    backgroundColor: "#1b4dff",
    padding: 12,
    borderRadius: 10,
    alignItems: "center",
    marginBottom: 10,
  },
  primaryBtnText: { color: "#fff", fontSize: 16 },
  secondaryBtn: {
    padding: 12,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: "#1b4dff",
    alignItems: "center",
    marginBottom: 10,
  },
  secondaryBtnText: { color: "#1b4dff" },
});
