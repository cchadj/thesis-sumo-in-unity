using System.IO;
using UnityEngine;

public class CaptureImage : MonoBehaviour
{
    private string _savePath;
    private string _imgPrefix = @"\UnityNicosiaImage";
    private string _imgPostfix = ".png";
    private int _curIndex = 0;

    private void Awake()
    {
        _savePath = Application.dataPath + @"\ScreenShots\";
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            while (File.Exists(_savePath + _imgPrefix + _curIndex + _imgPostfix))
                _curIndex++;

            string saveAtPath = _savePath + _imgPrefix + _curIndex + _imgPostfix;
            ScreenCapture.CaptureScreenshot(saveAtPath);
            print("Captured Screenshot saved at: " + saveAtPath);
            
        }
    }
}
