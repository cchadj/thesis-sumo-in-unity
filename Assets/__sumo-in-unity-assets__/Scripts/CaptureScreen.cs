using System.IO;
using UnityEngine;

public class CaptureScreen : MonoBehaviour {

    private string _savePath;
    private string _imgPrefix = @"\screen-capture";
    private string _imgPostfix = ".png";
    private int _curIndex = 0;

    private void Awake()
    {
        _savePath = Application.dataPath + @"\ScreenShots\";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            while (File.Exists(_savePath + _imgPrefix + _curIndex + _imgPostfix))
                _curIndex++;

            string saveAtPath = _savePath + _imgPrefix + _curIndex + _imgPostfix;
            ScreenCapture.CaptureScreenshot(saveAtPath);
            print("Captured Screenshot saved at: " + saveAtPath);

        }
    }
}
