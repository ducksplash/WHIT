using System;
using System.IO;
using Unity.Profiling;
using UnityEngine;

public class SpikeLogger : MonoBehaviour
{
    // Frame timings
    ProfilerRecorder _mainThreadTime;
    ProfilerRecorder _renderThreadTime;
    ProfilerRecorder _gcAlloc;

    StreamWriter _writer;

    void OnEnable()
    {
        // Note: Units are nanoseconds for the time recorders.
        _mainThreadTime   = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 60);
        _renderThreadTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread", 60);

        // GC alloc in bytes per frame (if available in your Unity version)
        _gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 60);

        var path = Path.Combine(Application.persistentDataPath, $"spike_log_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        _writer = new StreamWriter(path);
        _writer.WriteLine("frame,main_ms,render_ms,gc_bytes");
        Debug.Log($"SpikeLogger writing to: {path}");
    }

    void Update()
    {
        double mainMs   = _mainThreadTime.LastValue / 1_000_000.0;
        double renderMs = _renderThreadTime.LastValue / 1_000_000.0;
        long gcBytes    = _gcAlloc.Valid ? _gcAlloc.LastValue : -1;

        _writer.WriteLine($"{Time.frameCount},{mainMs:F3},{renderMs:F3},{gcBytes}");
    }

    void OnDisable()
    {
        _writer?.Flush();
        _writer?.Close();

        _mainThreadTime.Dispose();
        _renderThreadTime.Dispose();
        _gcAlloc.Dispose();
    }
}