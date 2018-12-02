using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Buttons;

public class CameraButtonManager : MonoBehaviour, IInputClickHandler
{
    public GameObject videoCaptureObject;

    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;

    private bool released = false;
    private float timer;
    public GameObject cameraText;

    public GameObject quad;

    // Use this for initialization
    void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
        cameraText.SetActive(false);
        gameObject.GetComponent<CompoundButtonMesh>().enabled = false;
    }

    void Update()
    {
        if(released == true)
        {
            // タップしたら撮影できるよロゴ 2sec
            // Tap to take a picture 
            if (timer < 2.0f)
            {
                timer += Time.deltaTime;
                cameraText.GetComponent<TextMesh>().text = "Tap to take\r\na picture";
            }
            else
            {
                cameraText.SetActive(false);
            }
        }
        else
        {
            cameraText.SetActive(false);
        }
    }

    /// AirTapイベントのハンドラ
    public void OnInputClicked(InputClickedEventData eventData)
    {
        // クリックしたオブジェクトがボタンだったら
        //Debug.Log(GazeManager.Instance.HitObject.name);
        if (GazeManager.Instance.HitObject != null)
        {
            if (GazeManager.Instance.HitObject.name == "CameraButton")
            {
                released = !released;
                if (released == true)
                {
                    timer = 0.0f;
                    cameraText.SetActive(true);
                    //gameObject.GetComponent<CompoundButtonMesh>().enabled = false;
                    videoCaptureObject.SetActive(false);
                }
            }
        }
        else
        {
            cameraText.SetActive(true);
            cameraText.GetComponent<TextMesh>().text = "Tapped Camera Mode";
            Debug.Log("Tapped Camera Mode");
            if (released == true)
            {
                Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

                // Create a PhotoCapture object
                PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject)
                {
                    photoCaptureObject = captureObject;
                    CameraParameters cameraParameters = new CameraParameters();
                    cameraParameters.hologramOpacity = 0.9f;
                    cameraParameters.cameraResolutionWidth = cameraResolution.width;
                    cameraParameters.cameraResolutionHeight = cameraResolution.height;
                    cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
                    // Activate the camera
                    photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result)
                    {
                        // Take a picture
                        photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                    });
                });

                // 撮影1回のみ
                released = false;
                videoCaptureObject.SetActive(true);
            }
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into our target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        //byte[] texByte = targetTexture.EncodeToJPG();
        byte[] texByte = targetTexture.EncodeToPNG();
        DateTime dt = DateTime.Now;
        //string filename = dt.ToString("yyyyMMddHHmmss.jpg");
        string filename = dt.ToString("yyyyMMddHHmmss") + ".png";
        Debug.Log(filename);

        GameObject obj = GameObject.Instantiate(quad);
        obj.GetComponent<Renderer>().material.SetTexture("_MainTex", targetTexture);
        Vector3 pos = new Vector3(0.0f, 0.0f, 2.0f);
        obj.transform.position = Camera.main.transform.TransformPoint(pos);
        obj.transform.LookAt(Camera.main.transform);
        obj.transform.Rotate(0.0f, 180.0f, 0.0f);

#if WINDOWS_UWP
        File.WriteAllBytes(Windows.Storage.ApplicationData.Current.LocalFolder.Path+"/"+filename, texByte);
#else
        File.WriteAllBytes(Application.streamingAssetsPath + "/" + filename, texByte);
#endif
        // Deactivate our camera
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown our photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}
