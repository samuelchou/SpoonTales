namespace SerialSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class SerialConnector
    {
        private const string targetVid = "1A86";    // Arduino Nano 的 VID
        private const int MaxBufferSize = 16;
        private string _portName = "COM1";
        private int _baudRate = 9600;
        private SerialPort _serialPort;
        private readonly object _lockObject = new();
        private int _byteCount = -1;
        private byte[] _byteBuffer = new byte[MaxBufferSize];
        private int _readByteCount = 0;

        // singleton
        private static SerialConnector _instance;

        public static SerialConnector Instance
        {
            get
            {
                _instance ??= new SerialConnector();
                return _instance;
            }
        }

        public bool IsConnected { get; private set; } = false;

        ~SerialConnector()
        {
            Disconnect();
        }

        public void LogAvailablePorts()
        {
            // 1. 先用 Unity 支援的標準方法列出基本 COM 名稱
            string[] basicPorts = SerialPort.GetPortNames();
            UnityEngine.Debug.Log($"[標準偵測] 發現 {basicPorts.Length} 個基礎序列埠。");

            // 2. 如果是 Windows 編輯器，我們用 PowerShell 抓取 VID/PID 與裝置名稱
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            DetectWindowsDeviceDetails();
#else
            Debug.Log("非 Windows 平台，跳過詳細裝置硬體資訊偵測。");
#endif
        }

        public void AutoConnectArduinoNano(int baudRate)
        {
            var portName = string.Empty;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            portName = FindWindowsArduinoNanoPort();
            if (!string.IsNullOrEmpty(portName))
            {
                Connect(portName, baudRate);
                Debug.Log($"自動連接到 Arduino Nano，使用序列埠: {portName}");
            }
            else
            {
                Debug.LogWarning("未找到 Arduino Nano 的序列埠，請確認裝置已連接。");
            }
#else
            Debug.LogWarning("非 Windows 平台，無法自動連接 Arduino Nano。");
#endif
        }

        public void Connect(string portName, int baudRate)
        {
            if (IsConnected)
            {
                Debug.LogWarning("Already connected, ignore.");
                return;
            }
            _portName = portName;
            _baudRate = baudRate;
            // Add actual serial connection logic here
            try
            {
                _serialPort = new SerialPort(portName, baudRate)
                {
                    ReadTimeout = 50,  // 設定讀取逾時（毫秒），避免執行緒卡死
                    WriteTimeout = 50 // 設定寫入逾時
                };
                _serialPort.DataReceived += OnDataReceived; // 註冊資料接收事件
                _serialPort.Open();            // 開啟連接

                IsConnected = true;

                Debug.Log($"序列埠 {portName} 成功開啟。");
            }
            catch (Exception e)
            {
                Debug.LogError($"無法開啟序列埠: {e.Message}");
            }
        }

        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            IsConnected = false;
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                Debug.Log($"序列埠 {_portName} 已關閉。");
            }
        }

        public bool TryGetData(out List<int> dataList)
        {
            if (!IsConnected)
            {
                dataList = null;
                return false;
            }

            lock (_lockObject)
            {
                if (_byteCount <= 0 || _byteCount != _readByteCount)
                {
                    dataList = null;
                    return false;
                }
                dataList = new List<int>();
                for (int i = 0; i < _byteCount; i++)
                {
                    dataList.Add(_byteBuffer[i]);
                }
                return true;
            }
        }

        // 背景執行緒迴圈
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs eargs)
        {
            if (!IsConnected)
            {
                return;
            }
            if (_serialPort != (SerialPort)sender)
            {
                Debug.LogWarning("接收到的資料來自未知的序列埠。");
                return;
            }
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Debug.LogWarning("Serial port is not open.");
                return;
            }

            try
            {
                lock (_lockObject)
                {
                    if (_byteCount <= 0 || _readByteCount >= _byteCount)
                    {
                        _byteCount = _serialPort.ReadByte();
                        _readByteCount = 0;
                    }
                    var nowReadByteCount = _serialPort.Read(_byteBuffer, _readByteCount, _byteCount - _readByteCount);
                    _readByteCount += nowReadByteCount;
                }
            }
            catch (TimeoutException)
            {
                // 逾時屬於正常現象，繼續循環即可
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"讀取發生錯誤: {ex.Message}");
            }
        }

        private void DetectWindowsDeviceDetails()
        {
            try
            {
                // 設定外部進程呼叫 PowerShell 查詢 PnP 裝置
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Get-CimInstance Win32_PnPEntity | Where-Object { $_.Name -cmatch 'COM' } | Select-Object Name, DeviceID | Format-Table -HideTableHeaders\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (string.IsNullOrEmpty(output))
                    {
                        Debug.Log("[詳細偵測] 未找到帶有 COM 的 Windows 裝置詳細資訊。");
                        return;
                    }

                    // 處理輸出結果
                    string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine))
                        {
                            Debug.Log($"[Windows 裝置詳細資訊]: {trimmedLine}");

                            // 範例：若需要用 Regex 抓取 VID/PID 可以這樣做
                            // Match match = Regex.Match(trimmedLine, @"VID_([0-9A-Fa-f]{4})&PID_([0-9A-Fa-f]{4})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"使用 PowerShell 偵測裝置詳細資訊時失敗: {ex.Message}");
            }
        }

        private string FindWindowsArduinoNanoPort()
        {
            try
            {
                // 設定外部進程呼叫 PowerShell 查詢 PnP 裝置
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Get-CimInstance Win32_PnPEntity | Where-Object { $_.Name -cmatch 'COM' } | Select-Object Name, DeviceID | Format-Table -HideTableHeaders\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (string.IsNullOrEmpty(output))
                    {
                        Debug.Log("[詳細偵測] 未找到帶有 COM 的 Windows 裝置詳細資訊。");
                        return string.Empty;
                    }

                    // 處理輸出結果
                    string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine))
                        {
                            var match = Regex.Match(trimmedLine, $"VID_{targetVid}");
                            if (match.Success)
                            {
                                // get port name
                                var portMatch = Regex.Match(trimmedLine, @"COM\d+");
                                if (portMatch.Success)
                                {
                                    return portMatch.Groups[0].Value;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"使用 PowerShell 偵測裝置詳細資訊時失敗: {ex.Message}");
            }
            return string.Empty;
        }
    }
}
