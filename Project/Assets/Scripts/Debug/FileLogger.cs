using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 文件日志记录器 - 将所有 Debug.Log 输出写入设备本地文件。
/// 日志文件路径: Application.persistentDataPath/unity_log.txt
/// Android 路径通常为: /storage/emulated/0/Android/data/{包名}/files/unity_log.txt
/// </summary>
public class FileLogger : MonoBehaviour
{
    private static string _logFilePath;
    private static StreamWriter _writer;

    private void Awake()
    {
        _logFilePath = Path.Combine(Application.persistentDataPath, "unity_log.txt");

        try
        {
            // 每次启动清空旧日志
            _writer = new StreamWriter(_logFilePath, false);
            _writer.AutoFlush = true;
            _writer.WriteLine($"=== Unity Log Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            _writer.WriteLine($"Platform: {Application.platform}");
            _writer.WriteLine($"Device: {SystemInfo.deviceModel}");
            _writer.WriteLine($"PersistentDataPath: {Application.persistentDataPath}");
            _writer.WriteLine("========================================");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FileLogger] 无法创建日志文件: {e.Message}");
        }
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        if (_writer == null) return;

        try
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string prefix = type switch
            {
                LogType.Error => "ERROR",
                LogType.Exception => "EXCEPTION",
                LogType.Warning => "WARN",
                _ => "INFO"
            };

            _writer.WriteLine($"[{timestamp}] [{prefix}] {message}");

            if (type == LogType.Error || type == LogType.Exception)
            {
                _writer.WriteLine($"  StackTrace: {stackTrace}");
            }
        }
        catch
        {
            // 写入失败时静默忽略，避免递归
        }
    }

    private void OnDestroy()
    {
        if (_writer != null)
        {
            _writer.WriteLine($"=== Unity Log Ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            _writer.Close();
            _writer = null;
        }
    }
}
