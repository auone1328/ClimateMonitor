import "react-native-gesture-handler";
import React, { useEffect, useMemo, useState } from "react";
import { NavigationContainer } from "@react-navigation/native";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { StatusBar } from "expo-status-bar";

import { createApi } from "./src/api/client";
import { loadTokens, saveTokens, clearTokens } from "./src/storage/tokens";

import WelcomeScreen from "./src/screens/WelcomeScreen";
import LoginScreen from "./src/screens/LoginScreen";
import RegisterAdminScreen from "./src/screens/RegisterAdminScreen";
import AcceptInviteScreen from "./src/screens/AcceptInviteScreen";
import BuildingsScreen from "./src/screens/BuildingsScreen";
import RoomsScreen from "./src/screens/RoomsScreen";
import RoomScreen from "./src/screens/RoomScreen";
import NotificationsScreen from "./src/screens/NotificationsScreen";
import MeasurementsScreen from "./src/screens/MeasurementsScreen";
import CreateInviteScreen from "./src/screens/CreateInviteScreen";
import AuditLogScreen from "./src/screens/AuditLogScreen";
import BuildingUsersScreen from "./src/screens/BuildingUsersScreen";

const Stack = createNativeStackNavigator();

export default function App() {
  const [accessToken, setAccessToken] = useState(null);
  const [refreshToken, setRefreshToken] = useState(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    loadTokens().then((tokens) => {
      if (tokens?.accessToken && tokens?.refreshToken) {
        setAccessToken(tokens.accessToken);
        setRefreshToken(tokens.refreshToken);
      }
      setReady(true);
    });
  }, []);

  const api = useMemo(() => createApi({
    getAccessToken: async () => accessToken,
    getRefreshToken: async () => refreshToken,
    setTokens: async (tokens) => {
      setAccessToken(tokens.accessToken);
      setRefreshToken(tokens.refreshToken);
      await saveTokens(tokens);
    },
  }), [accessToken, refreshToken]);

  async function handleLogin(tokens) {
    setAccessToken(tokens.accessToken);
    setRefreshToken(tokens.refreshToken);
    await saveTokens(tokens);
  }

  async function handleLogout() {
    setAccessToken(null);
    setRefreshToken(null);
    await clearTokens();
  }

  if (!ready) return null;

  return (
    <NavigationContainer>
      <StatusBar style="dark" />
      {accessToken ? (
        <Stack.Navigator>
          <Stack.Screen name="Buildings" options={{ headerShown: false }}>
            {(props) => (
              <BuildingsScreen
                {...props}
                api={api}
                onOpenNotifications={() => props.navigation.navigate("Notifications")}
                onLogout={handleLogout}
                onOpenAcceptInvite={() => props.navigation.navigate("AcceptInvite")}
                onOpenBuilding={(b) => props.navigation.navigate("Rooms", { building: b })}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="Rooms" options={{ headerShown: false }}>
            {(props) => (
              <RoomsScreen
                {...props}
                api={api}
                building={props.route.params.building}
                onBack={() => props.navigation.goBack()}
                onOpenInvite={() => props.navigation.navigate("CreateInvite", { building: props.route.params.building })}
                onOpenAudit={() => props.navigation.navigate("AuditLog", { building: props.route.params.building })}
                onOpenUsers={() => props.navigation.navigate("BuildingUsers", { building: props.route.params.building })}
                onOpenRoom={(room, role) => props.navigation.navigate("Room", { room, role })}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="Room" options={{ headerShown: false }}>
            {(props) => (
              <RoomScreen
                {...props}
                api={api}
                room={props.route.params.room}
                role={props.route.params.role}
                onBack={() => props.navigation.goBack()}
                onOpenHistory={() => props.navigation.navigate("Measurements", { room: props.route.params.room })}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="Measurements" options={{ headerShown: false }}>
            {(props) => (
              <MeasurementsScreen
                {...props}
                api={api}
                room={props.route.params.room}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="CreateInvite" options={{ headerShown: false }}>
            {(props) => (
              <CreateInviteScreen
                {...props}
                api={api}
                building={props.route.params.building}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="AuditLog" options={{ headerShown: false }}>
            {(props) => (
              <AuditLogScreen
                {...props}
                api={api}
                building={props.route.params.building}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="BuildingUsers" options={{ headerShown: false }}>
            {(props) => (
              <BuildingUsersScreen
                {...props}
                api={api}
                building={props.route.params.building}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="Notifications" options={{ headerShown: false }}>
            {(props) => (
              <NotificationsScreen
                {...props}
                api={api}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="AcceptInvite" options={{ headerShown: false }}>
            {(props) => (
              <AcceptInviteScreen
                {...props}
                api={api}
                isAuthenticated
                onInviteAccepted={() => props.navigation.goBack()}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
        </Stack.Navigator>
      ) : (
        <Stack.Navigator>
          <Stack.Screen name="Welcome" options={{ headerShown: false }}>
            {(props) => (
              <WelcomeScreen
                {...props}
                onLogin={() => props.navigation.navigate("Login")}
                onRegisterAdmin={() => props.navigation.navigate("RegisterAdmin")}
                onAcceptInvite={() => props.navigation.navigate("AcceptInvite")}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="Login" options={{ headerShown: false }}>
            {(props) => (
              <LoginScreen
                {...props}
                api={api}
                onLoginSuccess={handleLogin}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="RegisterAdmin" options={{ headerShown: false }}>
            {(props) => (
              <RegisterAdminScreen
                {...props}
                api={api}
                onLoginSuccess={handleLogin}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="AcceptInvite" options={{ headerShown: false }}>
            {(props) => (
              <AcceptInviteScreen
                {...props}
                api={api}
                onLoginSuccess={handleLogin}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>
        </Stack.Navigator>
      )}
    </NavigationContainer>
  );
}
