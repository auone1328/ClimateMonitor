import React, { useMemo } from "react";
import { View } from "react-native";
import Svg, { Polyline, Line, Text as SvgText } from "react-native-svg";

export default function SimpleLineChart({ data, height = 140 }) {
  const width = 320;
  const padding = 16;
  const labelPadBottom = 18;

  const clean = useMemo(() => {
    if (!data || data.length === 0) return [];
    return data.filter((d) => Number.isFinite(d?.value));
  }, [data]);

  const points = useMemo(() => {
    if (!clean || clean.length === 0) return "";
    const values = clean.map((d) => d.value);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const range = max - min || 1;

    return clean
      .map((d, i) => {
        const x = padding + (i / Math.max(1, clean.length - 1)) * (width - padding * 2);
        const y = padding + (1 - (d.value - min) / range) * (height - padding - labelPadBottom);
        return `${x},${y}`;
      })
      .join(" ");
  }, [clean, height]);

  const minMax = useMemo(() => {
    if (!clean || clean.length === 0) return null;
    const values = clean.map((d) => d.value);
    return { min: Math.min(...values), max: Math.max(...values) };
  }, [clean]);

  const timeLabels = useMemo(() => {
    if (!clean || clean.length === 0) return null;

    const firstTs = new Date(clean[0]?.ts);
    const lastTs = new Date(clean[clean.length - 1]?.ts);
    const spanMs = lastTs.getTime() - firstTs.getTime();
    const showDate = Number.isFinite(spanMs) && spanMs >= 36 * 60 * 60 * 1000;

    const getLabel = (idx) => {
      const raw = clean[idx]?.ts;
      if (!raw) return "";
      const date = new Date(raw);
      if (Number.isNaN(date.getTime())) return "";
      if (showDate) {
        return date.toLocaleDateString([], { day: "2-digit", month: "2-digit" });
      }
      return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
    };

    const last = clean.length - 1;
    return {
      left: getLabel(0),
      mid: getLabel(Math.floor(last / 2)),
      right: getLabel(last),
    };
  }, [clean]);

  if (!clean || clean.length === 0) return <View style={{ height }} />;

  return (
    <Svg width={width} height={height}>
      <Line x1={padding} y1={height - labelPadBottom} x2={width - padding} y2={height - labelPadBottom} stroke="#d6deee" />
      <Polyline points={points} fill="none" stroke="#1b4dff" strokeWidth="2" />
      {minMax && (
        <>
          <SvgText x={padding} y={padding + 2} fill="#667792" fontSize="10">{minMax.max.toFixed(1)}</SvgText>
          <SvgText x={padding} y={height - labelPadBottom - 4} fill="#667792" fontSize="10">{minMax.min.toFixed(1)}</SvgText>
        </>
      )}
      {timeLabels && (
        <>
          <SvgText x={padding} y={height - 2} fill="#667792" fontSize="10">{timeLabels.left}</SvgText>
          <SvgText x={width / 2} y={height - 2} fill="#667792" fontSize="10" textAnchor="middle">{timeLabels.mid}</SvgText>
          <SvgText x={width - padding} y={height - 2} fill="#667792" fontSize="10" textAnchor="end">{timeLabels.right}</SvgText>
        </>
      )}
    </Svg>
  );
}
