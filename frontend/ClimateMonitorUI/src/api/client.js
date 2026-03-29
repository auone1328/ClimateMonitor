import { API_BASE_URL } from "../config";

export function createApi({ getAccessToken, getRefreshToken, setTokens }) {
  async function refreshAccessToken() {
    const refreshToken = await getRefreshToken();
    const accessToken = await getAccessToken();
    if (!refreshToken || !accessToken) return null;

    const res = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!res.ok) return null;
    const data = await res.json();
    if (data?.accessToken && data?.refreshToken) {
      await setTokens({ accessToken: data.accessToken, refreshToken: data.refreshToken });
      return data.accessToken;
    }
    return null;
  }

  async function request(path, options = {}, retry = true) {
    const headers = { ...(options.headers || {}) };
    const accessToken = await getAccessToken();
    if (accessToken) headers.Authorization = `Bearer ${accessToken}`;
    if (options.body && !headers["Content-Type"]) headers["Content-Type"] = "application/json";

    const res = await fetch(`${API_BASE_URL}${path}`, { ...options, headers });

    if (res.status === 401 && retry) {
      const newToken = await refreshAccessToken();
      if (newToken) return request(path, options, false);
    }

    return res;
  }

  async function readError(res) {
    let text = "";
    try {
      text = await res.text();
    } catch {
      return res.statusText || "Ошибка запроса.";
    }

    if (!text) return res.statusText || "Ошибка запроса.";

    try {
      const data = JSON.parse(text);
      if (data?.detail) return String(data.detail);
      if (data?.title) return String(data.title);
    } catch {
      // not json
    }

    return text;
  }

  return { request, readError };
}
