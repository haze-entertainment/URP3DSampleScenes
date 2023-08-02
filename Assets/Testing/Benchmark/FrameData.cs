using System.Collections.Generic;
using UnityEngine;

namespace Benchmarking
{

    public struct FrameData
    {
        public float frameTime;
        private float _fpsOverride;
        public float fps => (_fpsOverride > 0f) ? _fpsOverride : 1000f / frameTime;

        public bool advancedFrameTiming;
        public double cpuTime;
        public double cpuRenderTime;
        public double gpuTime;

        public double timeLineTime;

        public FrameData(float timeMS, float timelineTime = 0, bool captureAdvancedTimings = true)
        {
            frameTime = timeMS;
            this.timeLineTime = timelineTime;
            _fpsOverride = -1f;

            advancedFrameTiming = FrameTimingManager.IsFeatureEnabled() && captureAdvancedTimings;

            cpuTime = cpuRenderTime = gpuTime = timeMS;

            if (advancedFrameTiming)
            {
                FrameTimingManager.CaptureFrameTimings();
                FrameTiming[] timings = new FrameTiming[1];
                uint count = FrameTimingManager.GetLatestTimings(1, timings);
                if (count > 0)
                {
                    cpuTime = timings[0].cpuFrameTime;
                    cpuRenderTime = timings[0].cpuRenderThreadFrameTime;
                    gpuTime = timings[0].gpuFrameTime;
                }
            }
        }

        public static FrameData GetCurrentFrameData( float timelineTime = 0, bool captureAdvancedTimings = true )
        {
            return new FrameData(Time.deltaTime * 1000f, timelineTime, captureAdvancedTimings);
        }

        public void SetFPSOverride(float fpsOverride)
        {
            _fpsOverride = fpsOverride;
        }

        public void ResetFPSOverride() { _fpsOverride = -1f; }

        public static FrameData Min(FrameData a, FrameData b, bool overrideFPS = false)
        {
            FrameData o = new FrameData();

            o.frameTime = Mathf.Min(a.frameTime, b.frameTime);
            if (overrideFPS)
                o._fpsOverride = Mathf.Min(a.fps, b.fps);
            else
                o._fpsOverride = -1f;

            o.advancedFrameTiming = a.advancedFrameTiming && b.advancedFrameTiming;
            if (o.advancedFrameTiming)
            {
                o.cpuTime = DoubleMin(a.cpuTime, b.cpuTime);
                o.cpuRenderTime = DoubleMin(a.cpuRenderTime, b.cpuRenderTime);
                o.gpuTime = DoubleMin(a.gpuTime, b.gpuTime);
            }

            return o;
        }
        public void MinWith(FrameData other, bool overrideFPS = false)
        {
            this = Min(this, other, overrideFPS);
        }

        public static FrameData Max(FrameData a, FrameData b, bool overrideFPS = false)
        {
            FrameData o = new FrameData();

            o.frameTime = Mathf.Max(a.frameTime, b.frameTime);
            if (overrideFPS)
                o._fpsOverride = Mathf.Max(a.fps, b.fps);
            else
                o._fpsOverride = -1f;

            o.advancedFrameTiming = a.advancedFrameTiming && b.advancedFrameTiming;
            if (o.advancedFrameTiming)
            {
                o.cpuTime = DoubleMax(a.cpuTime, b.cpuTime);
                o.cpuRenderTime = DoubleMax(a.cpuRenderTime, b.cpuRenderTime);
                o.gpuTime = DoubleMax(a.gpuTime, b.gpuTime);
            }

            return o;
        }

        public void MaxWith(FrameData other, bool overrideFPS = false)
        {
            this = Max(this, other, overrideFPS);
        }

        public static FrameData Average(FrameData a, int countA, FrameData b, int countB, bool overrideFPS = false)
        {
            FrameData o = new FrameData();
            float divider = 1.0f / (countA + countB);
            o.frameTime = (a.frameTime * countA + b.frameTime * countB) * divider;
            if (overrideFPS)
                o._fpsOverride = (a.fps * countA + b.fps * countB) * divider;
            else
                o._fpsOverride = -1f;

            o.advancedFrameTiming = a.advancedFrameTiming && b.advancedFrameTiming;
            if (o.advancedFrameTiming)
            {
                o.cpuTime = (a.cpuTime * countA + b.cpuTime * countB) * divider;
                o.cpuRenderTime = (a.cpuRenderTime * countA + b.cpuRenderTime * countB) * divider;
                o.gpuTime = (a.gpuTime * countA + b.gpuTime * countB) * divider;
            }

            return o;
        }

        public void AverageWith(FrameData other, int count, bool overrideFPS = false)
        {
            this = Average(this, count - 1, other, 1, overrideFPS);
        }

        public static FrameData Lerp(FrameData a, FrameData b, float t)
        {
            FrameData o = new FrameData();
            o.frameTime = Mathf.Lerp(a.frameTime, b.frameTime, t);
            o._fpsOverride = -1f;

            o.advancedFrameTiming = a.advancedFrameTiming && b.advancedFrameTiming;
            if (o.advancedFrameTiming)
            {
                o.cpuTime = DoubleLerp(a.cpuTime, b.cpuTime, t);
                o.cpuRenderTime = DoubleLerp(a.cpuRenderTime, b.cpuRenderTime, t);
                o.gpuTime = DoubleLerp(a.gpuTime, b.gpuTime, t);
            }

            return o;
        }

        public static FrameData MinMultiple(List<FrameData> frameTimes)
        {
            if (frameTimes == null || frameTimes.Count < 1)
                return new FrameData();

            FrameData o = frameTimes[0];
            for (int i = 1; i < frameTimes.Count; i++)
            {
                o.MinWith(frameTimes[i], true);
            }

            return o;
        }

        public static FrameData MaxMultiple(List<FrameData> frameTimes)
        {
            if (frameTimes == null || frameTimes.Count < 1)
                return new FrameData();

            FrameData o = frameTimes[0];
            for (int i = 1; i < frameTimes.Count; i++)
            {
                o.MaxWith(frameTimes[i], true);
            }

            return o;
        }

        private static double DoubleMin(double a, double b)
        {
            return (a < b) ? a : b;
        }
        private static double DoubleMax(double a, double b)
        {
            return (a > b) ? a : b;
        }
        private static double DoubleLerp(double a, double b, double t)
        {
            return a * (1 - t) + b * t;
        }

        override public string ToString()
        {
            return $"FrameData{{frameTime: {frameTime} ms, fps: {fps}, advancedFrameTiming: {advancedFrameTiming}, cpuTime: {cpuTime} ms, cpuRenderTime: {cpuRenderTime} ms, gpuTime: {gpuTime} ms}}";
        }

        public string GetValueString (DataType dataType)
        {
            switch (dataType)
            {
                case DataType.FPS:
                    return fps.ToString();
                case DataType.CPUTime:
                    return cpuTime.ToString();
                case DataType.CPURenderTime:
                    return cpuRenderTime.ToString();
                case DataType.GPUTime:
                    return gpuTime.ToString();
                default:
                    return frameTime.ToString();
            }
        }

        public float GetValue(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.FPS:
                    return fps;
                case DataType.CPUTime:
                    return (float)cpuTime;
                case DataType.CPURenderTime:
                    return (float)cpuRenderTime;
                case DataType.GPUTime:
                    return (float)gpuTime;
                default:
                    return frameTime;
            }
        }
    }
}