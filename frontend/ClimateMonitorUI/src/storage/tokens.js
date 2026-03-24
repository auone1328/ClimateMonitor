import AsyncStorage from "@react-native-async-storage/async-storage";

const TOKEN_KEY = "cm_tokens_v1";

export async function loadTokens() {
  const raw = await AsyncStorage.getItem(TOKEN_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

export async function saveTokens(tokens) {
  await AsyncStorage.setItem(TOKEN_KEY, JSON.stringify(tokens));
}

export async function clearTokens() {
  await AsyncStorage.removeItem(TOKEN_KEY);
}
