import React, { useEffect, useState } from "react";
import { View, Text, TextInput, Pressable, StyleSheet, Alert } from "react-native";
import { CameraView, useCameraPermissions } from "expo-camera";

export default function AcceptInviteScreen({ api, onLoginSuccess, onBack, isAuthenticated, onInviteAccepted }) {
  const [permission, requestPermission] = useCameraPermissions();
  const [scanned, setScanned] = useState(false);
  const [token, setToken] = useState("");
  const [email, setEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!permission) requestPermission();
  }, [permission, requestPermission]);

  function handleScan({ data }) {
    setToken(String(data).trim());
    setScanned(true);
  }

  async function submit() {
    if (!token) {
      Alert.alert("Не заполнены поля", "Токен приглашения обязателен.");
      return;
    }
    if (!isAuthenticated && (!email || !userName || !password)) {
      Alert.alert("Не заполнены поля", "Email, имя пользователя и пароль обязательны.");
      return;
    }

    setLoading(true);
    try {
      const res = await api.request(
        isAuthenticated ? "/api/registration/invite/authenticated" : "/api/registration/invite",
        {
          method: "POST",
          body: JSON.stringify(
            isAuthenticated
              ? { token }
              : { token, email, userName, password }
          ),
        }
      );

      if (!res.ok) {
        const text = await res.text();
        Alert.alert("Приглашение не принято", text || res.statusText);
        return;
      }

      if (!isAuthenticated) {
        const data = await res.json();
        if (!data?.accessToken || !data?.refreshToken) {
          Alert.alert("Приглашение не принято", "Нет токенов в ответе.");
          return;
        }
        await onLoginSuccess({ accessToken: data.accessToken, refreshToken: data.refreshToken });
      } else if (onInviteAccepted) {
        onInviteAccepted();
      }
    } catch (e) {
      Alert.alert("Приглашение не принято", String(e));
    } finally {
      setLoading(false);
    }
  }

  if (!permission) {
    return (
      <View style={styles.container}>
        <Text style={styles.title}>Запрос разрешения на камеру...</Text>
      </View>
    );
  }

  if (!permission.granted) {
    return (
      <View style={styles.container}>
        <Text style={styles.title}>Требуется доступ к камере.</Text>
        <Pressable style={styles.primaryBtn} onPress={requestPermission}>
          <Text style={styles.primaryBtnText}>Разрешить</Text>
        </Pressable>
        <Pressable style={styles.linkBtn} onPress={onBack}>
          <Text style={styles.linkText}>Назад</Text>
        </Pressable>
      </View>
    );
  }

  if (!scanned) {
    return (
      <View style={styles.container}>
        <Text style={styles.title}>Сканируйте QR приглашения</Text>
        <View style={styles.scannerBox}>
          <CameraView
            style={StyleSheet.absoluteFillObject}
            onBarcodeScanned={scanned ? undefined : handleScan}
            barcodeScannerSettings={{ barcodeTypes: ["qr"] }}
          />
        </View>
        <Pressable style={styles.linkBtn} onPress={onBack}>
          <Text style={styles.linkText}>Назад</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Принять приглашение</Text>
      <Text style={styles.sub}>Токен приглашения найден</Text>
      {!isAuthenticated && (
        <>
          <TextInput style={styles.input} placeholder="Email" autoCapitalize="none" value={email} onChangeText={setEmail} />
          <TextInput style={styles.input} placeholder="Имя пользователя" value={userName} onChangeText={setUserName} />
          <TextInput style={styles.input} placeholder="Пароль" secureTextEntry value={password} onChangeText={setPassword} />
        </>
      )}
      <Pressable style={styles.primaryBtn} onPress={submit} disabled={loading}>
        <Text style={styles.primaryBtnText}>{loading ? "Отправка..." : "Принять приглашение"}</Text>
      </Pressable>
      <Pressable style={styles.linkBtn} onPress={() => { setScanned(false); setToken(""); }}>
        <Text style={styles.linkText}>Сканировать снова</Text>
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
    fontSize: 26,
    marginBottom: 12,
    color: "#172134",
    fontFamily: "serif",
  },
  sub: { color: "#41536b", marginBottom: 8 },
  scannerBox: {
    height: 320,
    borderRadius: 16,
    overflow: "hidden",
    borderWidth: 1,
    borderColor: "#d6deee",
    marginBottom: 16,
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
  linkBtn: { padding: 8, alignItems: "center" },
  linkText: { color: "#1b4dff" },
});
