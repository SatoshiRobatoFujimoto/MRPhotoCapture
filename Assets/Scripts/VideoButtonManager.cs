using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Buttons;

public class VideoButtonManager : MonoBehaviour, IInputClickHandler
{

    public GameObject photoCaptureObject;

    //VideoCapture APIはEditor上では使えない
    VideoCapture videoCaptureObject = null;

    static readonly float maxRecordingTime = 5.0f;
    float stopRecordingTimer = float.MaxValue;

    private bool released = false;
    private float timer;
    public GameObject videoText;

    void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
        videoText.SetActive(false);
        gameObject.GetComponent<CompoundButtonMesh>().enabled = false;
    }

    void Update()
    {
        if (released == true)
        {
            // タップしたら撮影できるよロゴ 2sec
            // Tap to take a picture 
            if (timer < 2.0f)
            {
                timer += Time.deltaTime;
                videoText.GetComponent<TextMesh>().text = "Tap to take\r\na video";
            }
            else
            {
                videoText.SetActive(false);
            }
        }
        else
        {
            videoText.SetActive(false);
        }

        if (videoCaptureObject == null || !videoCaptureObject.IsRecording)
        {
            return;
        }

        if (Time.time > stopRecordingTimer)
        {
            videoCaptureObject.StopRecordingAsync(OnStoppedRecordingVideo);
            released = false;
            photoCaptureObject.SetActive(true);
        }

    }

    /// AirTapイベントのハンドラ
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (GazeManager.Instance.HitObject != null)
        {
            Debug.Log(GazeManager.Instance.HitObject.name);
            if (GazeManager.Instance.HitObject.name == "VideoButton")
            {
                // 3,2,1で録画開始
                released = !released;
                if (released == true)
                {
                    timer = 0.0f;
                    videoText.SetActive(true);
                    photoCaptureObject.SetActive(false);
                }
            }
        }
        else
        {
            videoText.SetActive(true);
            videoText.GetComponent<TextMesh>().text = "Tapped Video Mode";
            Debug.Log("Tapped Video Mode");
            if (released == true)
            {
                Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                Debug.Log(cameraResolution);

                float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
                Debug.Log(cameraFramerate);

                VideoCapture.CreateAsync(true, delegate (VideoCapture videoCapture)
                {
                    Debug.Log("Created VideoCapture Instance!");
                    videoCaptureObject = videoCapture;
                    CameraParameters cameraParameters = new CameraParameters();
                    cameraParameters.hologramOpacity = 0.9f;
                    cameraParameters.frameRate = cameraFramerate;
                    cameraParameters.cameraResolutionWidth = cameraResolution.width;
                    cameraParameters.cameraResolutionHeight = cameraResolution.height;
                    cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
                    videoCaptureObject.StartVideoModeAsync(cameraParameters, VideoCapture.AudioState.ApplicationAndMicAudio, OnStartedVideoCaptureMode);
                    //if (videoCapture != null)
                    //{
                    //    m_VideoCapture = videoCapture;
                    //    Debug.Log("Created VideoCapture Instance!");

                    //    CameraParameters cameraParameters = new CameraParameters();
                    //    cameraParameters.hologramOpacity = 0.9f;
                    //    cameraParameters.frameRate = cameraFramerate;
                    //    cameraParameters.cameraResolutionWidth = cameraResolution.width;
                    //    cameraParameters.cameraResolutionHeight = cameraResolution.height;
                    //    cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

                    //    m_VideoCapture.StartVideoModeAsync(cameraParameters,
                    //        VideoCapture.AudioState.ApplicationAndMicAudio,
                    //        OnStartedVideoCaptureMode);
                    //}
                    //else
                    //{
                    //    Debug.LogError("Failed to create VideoCapture Instance!");
                    //}
                });
            }
        }
    }

    void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Started Video Capture Mode!");
        //string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
        //string filename = string.Format("TestVideo_{0}.mp4", timeStamp);
        //string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        //filepath = filepath.Replace("/", @"\");

        DateTime dt = DateTime.Now;
        string filename = dt.ToString("yyyyMMddHHmmss") + ".mp4";
#if WINDOWS_UWP
        string filepath = Windows.Storage.ApplicationData.Current.LocalFolder.Path+"/"+filename;
#else
        string filepath = Application.streamingAssetsPath + "/" + filename;
#endif

        videoCaptureObject.StartRecordingAsync(filepath, OnStartedRecordingVideo);
    }

    void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Stopped Video Capture Mode!");
    }

    void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Started Recording Video!");
        stopRecordingTimer = Time.time + maxRecordingTime;
    }

    void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Stopped Recording Video!");
        videoCaptureObject.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

}
