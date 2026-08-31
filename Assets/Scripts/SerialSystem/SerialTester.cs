namespace SerialSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    public class SerialTester : MonoBehaviour
    {
        [SerializeField]
        private string _portName = "COM1";
        [SerializeField]
        private int _baudRate = 9600;
        [SerializeField]
        private RectTransform _root;
        [SerializeField]
        private TestDisplayer _displayerPrefab;
        private List<TestDisplayer> _nowDisplays = new List<TestDisplayer>();

        private void Start()
        {
            SerialConnector.Instance.Connect(_portName, _baudRate);
        }

        private void Update()
        {
            if (!SerialConnector.Instance.IsConnected)
            {
                return;
            }

            if (SerialConnector.Instance.TryGetData(out var datas))
            {
                if (datas.Count > _nowDisplays.Count)
                {
                    var newDisplayer = GameObject.Instantiate(_displayerPrefab, _root);
                    newDisplayer.name = $"Sensor {datas.Count + 1}";
                    _nowDisplays.Add(newDisplayer);
                }
                for (int i = 0; i < datas.Count; i++)
                {
                    _nowDisplays[i].ValueText.text = $"{datas[i] / 10.0f:F1} cm";
                }
            }
        }

        private void OnDestroy()
        {
            SerialConnector.Instance.Disconnect();
        }
    }
}
