import React, { useMemo } from "react";
import { View } from "react-native";
import Svg, { Polyline, Line, Text as SvgText } from "react-native-svg";

export default function SimpleLineChart({ data, height = 140 }) {
  const width = 320;
  const padding = 16;

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
        const y = padding + (1 - (d.value - min) / range) * (height - padding * 2);
        return `${x},${y}`;
      })
      .join(" ");
  }, [clean, height]);

  const minMax = useMemo(() => {
    if (!clean || clean.length === 0) return null;
    const values = clean.map((d) => d.value);
    return { min: Math.min(...values), max: Math.max(...values) };
  }, [clean]);

  if (!clean || clean.length === 0) return <View style={{ height }} />;

  return (
    <Svg width={width} height={height}>
      <Line x1={padding} y1={height - padding} x2={width - padding} y2={height - padding} stroke="#d6deee" />
      <Polyline points={points} fill="none" stroke="#1b4dff" strokeWidth="2" />
      {minMax && (
        <>
          <SvgText x={padding} y={padding} fill="#667792" fontSize="10">{minMax.max.toFixed(1)}</SvgText>
          <SvgText x={padding} y={height - 4} fill="#667792" fontSize="10">{minMax.min.toFixed(1)}</SvgText>
        </>
      )}
    </Svg>
  );
}
