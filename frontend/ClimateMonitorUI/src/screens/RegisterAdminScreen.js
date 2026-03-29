import React, { useEffect, useState } from "react";
import { View, Text, TextInput, Pressable, StyleSheet, Alert } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { CameraView, useCameraPermissions } from "expo-camera";

export default function RegisterAdminScreen({ api, onLoginSuccess, onBack }) {
  const [permission, requestPermission] = useCameraPermissions();
  const [scanned, setScanned] = useState(false);
  const [qrData, setQrData] = useState(null);
  const [email, setEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!permission) requestPermission();
  }, [permission, requestPermission]);

  function handleScan({ data }) {
    try {
      const parsed = JSON.parse(data);
      if (!parsed.mac || !parsed.building || !parsed.room || !parsed.secret) {
        throw new Error("Invalid QR payload");
      }
      setQrData(parsed);
      setScanned(true);
    } catch (e) {
      Alert.alert("Неверный QR", "Отсканируйте QR устройства.");
    }
  }

  async function submit() {
    if (!qrData) return;
    if (!email || !userName || !password) {
      Alert.alert("Не заполнены поля", "Email, имя пользователя и пароль обязательны.");
      return;
    }

    setLoading(true);
    try {
      const res = await api.request("/api/registration/device-qr", {
        method: "POST",
        body: JSON.stringify({
          macAddress: qrData.mac,
          buildingName: qrData.building,
          roomName: qrData.room,
          secret: qrData.secret,
          email,
          userName,
          password,
        }),
      });

      if (!res.ok) {
        const text = await api.readError(res);
        Alert.alert("Регистрация не удалась", text);
        return;
      }

      const data = await res.json();
      if (!data?.accessToken || !data?.refreshToken) {
        Alert.alert("Регистрация не удалась", "Нет токенов в ответе.");
        return;
      }

      await onLoginSuccess({ accessToken: data.accessToken, refreshToken: data.refreshToken });
    } catch (e) {
      Alert.alert("Регистрация не удалась", String(e));
    } finally {
      setLoading(false);
    }
  }

  if (!permission) {
    return (
      <SafeAreaView style={styles.container}>
        <Text style={styles.title}>Запрос разрешения на камеру...</Text>
      </SafeAreaView>
    );
  }

  if (!permission.granted) {
    return (
      <SafeAreaView style={styles.container}>
        <Text style={styles.title}>Требуется доступ к камере.</Text>
        <Pressable style={styles.primaryBtn} onPress={requestPermission}>
          <Text style={styles.primaryBtnText}>Разрешить</Text>
        </Pressable>
        <Pressable style={styles.linkBtn} onPress={onBack}>
          <Text style={styles.linkText}>Назад</Text>
        </Pressable>
      </SafeAreaView>
    );
  }

  if (!scanned) {
    return (
      <SafeAreaView style={styles.container}>
        <Text style={styles.title}>Сканируйте QR устройства</Text>
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
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.title}>Создать администратора</Text>
      <Text style={styles.sub}>Здание: {qrData.building}</Text>
      <Text style={styles.sub}>Комната: {qrData.room}</Text>
      <TextInput style={styles.input} placeholder="Email" autoCapitalize="none" value={email} onChangeText={setEmail} />
      <TextInput style={styles.input} placeholder="Имя пользователя" value={userName} onChangeText={setUserName} />
      <TextInput style={styles.input} placeholder="Пароль" secureTextEntry value={password} onChangeText={setPassword} />
      <Pressable style={styles.primaryBtn} onPress={submit} disabled={loading}>
        <Text style={styles.primaryBtnText}>{loading ? "Регистрация..." : "Зарегистрировать администратора"}</Text>
      </Pressable>
      <Pressable style={styles.linkBtn} onPress={() => { setScanned(false); setQrData(null); }}>
        <Text style={styles.linkText}>Сканировать снова</Text>
      </Pressable>
      <Pressable style={styles.linkBtn} onPress={onBack}>
        <Text style={styles.linkText}>Назад</Text>
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
