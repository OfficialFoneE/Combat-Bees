using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

// Just draws the timing for everything.
public class BeeGizmos : MonoBehaviour
{
    private const int textFontSize = 12;

    private const float textPaddingTop = 3;
    private const float textPaddingLeft = 3;

    private const float textHeight = 15;
    private const float textSpacing = 3;

    private GUIStyle style = new GUIStyle();

    [HideInInspector] public int beeCount;
    [HideInInspector] public int deadBeeCount;

    private IntValue fps;
    public DoubleValue simulationTime = new DoubleValue(0);
    public DoubleValue renderTime = new DoubleValue(0);
    private DoubleValue totalRenderTime = new DoubleValue(0);

    private void Awake()
    {
        style.fontSize = textFontSize;
        style.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        fps.UpdateValue((int)math.ceil(1f / Time.unscaledDeltaTime));
        totalRenderTime.UpdateValue(simulationTime.value + renderTime.value);

        GUI.color = Color.white;
        int textWidth = Screen.width;
        Rect rect = new Rect(textPaddingLeft, textPaddingTop, textWidth, textHeight);

        PrintLabel($"FPS: {fps.Average} [{fps.min}, {fps.max}]");
        PrintLabel($"BeeCount (Alive): {beeCount}");
        PrintLabel($"BeeCount (Dead): {deadBeeCount}");

        PrintLabel($"SimulationTime (CPU): {simulationTime.Average.ToString("00.000")} ms [{simulationTime.min.ToString("00.000")}, {simulationTime.max.ToString("00.000")}]");
        PrintLabel($"RenderTime (CPU): {renderTime.Average.ToString("00.000")} ms [{renderTime.min.ToString("00.000")}, {renderTime.max.ToString("00.000")}]");
        PrintLabel($"TotalTime (CPU): {totalRenderTime.Average.ToString("00.000")} ms [{totalRenderTime.min.ToString("00.000")}, {totalRenderTime.max.ToString("00.000")}]");

        void PrintLabel(string text)
        {
            GUI.Label(rect, text, style);
            rect.y += textHeight + textSpacing;
        }
    }

    public struct DoubleValue
    {
        FixedList64Bytes<double> buffer;

        public double min;
        public double max;

        public double value;

        public DoubleValue(double initialValue)
        {
            value = initialValue;
            buffer = new FixedList32Bytes<double>();
            min = double.MaxValue;
            max = double.MinValue;
        }

        public unsafe double Average {

            get
            {
                double totalValue = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    totalValue += buffer[i];
                }
                return totalValue / buffer.Length;
            }
        }

        public void UpdateValue(double value)
        {
            this.value = value;
            min = math.min(min, value);
            max = math.max(max, value);

            for (int i = buffer.Length - 1; i >= 1; i--)
            {
                buffer[i] = buffer[i - 1];
            }
            if (buffer.Capacity == buffer.Length)
            {
                buffer[0] = value;
            }
            else
            {
                buffer.Add(value);
            }
        }
    }

    public struct IntValue
    {
        FixedList64Bytes<int> buffer;

        public int min;
        public int max;

        public int value;

        public IntValue(int initialValue)
        {
            value = initialValue;
            buffer = new FixedList32Bytes<int>();
            min = int.MaxValue;
            max = int.MinValue;
        }

        public unsafe int Average
        {

            get
            {
                int totalValue = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    totalValue += buffer[i];
                }
                return totalValue / buffer.Length;
            }
        }

        public void UpdateValue(int value)
        {
            this.value = value;
            min = math.min(min, value);
            max = math.max(max, value);

            for (int i = buffer.Length - 1; i >= 1; i--)
            {
                buffer[i] = buffer[i - 1];
            }
            if (buffer.Capacity == buffer.Length)
            {
                buffer[0] = value;
            }
            else
            {
                buffer.Add(value);
            }
        }
    }
}
