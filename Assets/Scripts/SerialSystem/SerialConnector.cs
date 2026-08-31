namespace SerialSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using UnityEngine;

    public class SerialConnector
    {
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
    }
}
