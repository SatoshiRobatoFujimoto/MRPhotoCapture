using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity.InputModule;
using UnityEngine.Video;
//using HoloToolkit.Unity.Buttons;

public class TapManager : MonoBehaviour, IInputClickHandler
{

    private int captureMode = 0; //0: Nothing, 1: Camera, 2,3: Video
    private bool released = false;
    private float timer;

    public GameObject captureCanvas;

    // PhotoCapture
    private PhotoCapture photoCaptureObject = null;
    private Texture2D targetTexture = null;
    public GameObject cameraText;
    public GameObject photoDisplayObject;

    // VideoCapture
    private VideoCapture videoCaptureObject = null;
    //static readonly float maxRecordingTime = 5.0f;
    //private float stopRecordingTimer = float.MaxValue;
    public GameObject videoText;
    public GameObject videoDisplayObject;
    private string videoFilepath;

    private Texture2D texture_icon_cancel;
    private Texture2D texture_icon_camera;
    private Texture2D texture_icon_video;

    // Use this for initialization
    void Start()
    {
        released = false;
        InputManager.Instance.PushFallbackInputHandler(gameObject);

        cameraText.SetActive(false);
        videoText.SetActive(false);

        texture_icon_cancel = Resources.Load("icon_126052_256_2") as Texture2D;
        texture_icon_camera = Resources.Load("icon_111970_256_2") as Texture2D;
        texture_icon_video = Resources.Load("icon_110130_256_2") as Texture2D;
    }

// Update is called once per frame
void Update()
    {
        if (released == true)
        {
            // タップしたら撮影できるよロゴ 2sec
            // Tap to take a picture 
            if (timer < 2.0f)
            {
                timer += Time.deltaTime;
                if(captureMode == 1)
                {
                    cameraText.GetComponent<TextMesh>().text = "Tap to take\r\na picture";
                }
                else if(captureMode == 2)
                {
                    videoText.GetComponent<TextMesh>().text = "Tap to take\r\na video";
                }
            }
            else
            {
                cameraText.SetActive(false);
                videoText.SetActive(false);
            }
        }
        else
        {
            cameraText.SetActive(false);
            videoText.SetActive(false);
            if (captureMode == 4)
            {
                captureMode = 0;
            }
        }
    }

    public void OnPhotoCaptureButtonClicked()
    {
        if(captureMode == 0)
        {
            captureMode = 1;
            released = true;
            timer = 0.0f;
            cameraText.SetActive(true);
            //captureCanvas.SetActive(false);
            captureCanvas.transform.Find("VideoCaptureButton").gameObject.SetActive(false);
            //キャンセルボタンに変更
            Texture2D texture = Resources.Load("icon_126052_256_2") as Texture2D;
            Image img = captureCanvas.transform.Find("PhotoCaptureButton").gameObject.GetComponent<Image>();
            img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        else if (captureMode == 1)
        {
            //Debug.Log("Cancel Capture 1");
            //キャンセルボタンをもとに戻す
            Image img_icon_camera = captureCanvas.transform.Find("PhotoCaptureButton").gameObject.GetComponent<Image>();
            img_icon_camera.sprite = Sprite.Create(texture_icon_camera, new Rect(0, 0, texture_icon_camera.width, texture_icon_camera.height), Vector2.zero);
            captureCanvas.transform.Find("VideoCaptureButton").gameObject.SetActive(true);
            released = false;
            captureMode = 0;
        }

    }

    public void OnVideoCaptureButtonClicked()
    {
        if(captureMode == 0)
        {
            captureMode = 2;
            released = true;
            timer = 0.0f;
            videoText.SetActive(true);
            captureCanvas.transform.Find("PhotoCaptureButton").gameObject.SetActive(false);
            //キャンセルボタンの表示
            Image img_icon_cancel = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Image>();
            img_icon_cancel.sprite = Sprite.Create(texture_icon_cancel, new Rect(0, 0, texture_icon_cancel.width, texture_icon_cancel.height), Vector2.zero);
        }
        else if (captureMode == 2)
        {
            //Debug.Log("Cancel Capture 2");
            //キャンセルボタンをもとに戻す
            Image img_icon_video = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Image>();
            img_icon_video.sprite = Sprite.Create(texture_icon_video, new Rect(0, 0, texture_icon_video.width, texture_icon_video.height), Vector2.zero);
            captureCanvas.transform.Find("PhotoCaptureButton").gameObject.SetActive(true);
            //ビデオボタンの色を戻す
            ColorBlock cb = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Button>().colors;
            cb.normalColor = new Color(1.0f, 1.0f, 1.0f);
            cb.highlightedColor = new Color(1.0f, 1.0f, 1.0f);
            captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Button>().colors = cb;
            released = false;
            captureMode = 0;
        }
        else if (captureMode == 3)
        {
            //動画撮影停止
#if WINDOWS_UWP
            if (videoCaptureObject == null || !videoCaptureObject.IsRecording)
            {
                return;
            }
#else
            if (captureMode != 3 && released != true)
            {
                return;
            }
#endif

            //videoText.SetActive(true);
            //videoText.GetComponent<TextMesh>().text = (stopRecordingTimer - Time.time).ToString();
            //if (Time.time > stopRecordingTimer)
            //{
#if WINDOWS_UWP
            videoCaptureObject.StopRecordingAsync(OnStoppedRecordingVideo);
#else

            GameObject obj = GameObject.Instantiate(videoDisplayObject);
            obj.GetComponent<VideoPlayer>().source = VideoSource.Url;
            obj.GetComponent<VideoPlayer>().url = Application.streamingAssetsPath + "/" + "20181108_161952_HoloLens.mp4";

            Vector3 pos = new Vector3(0.0f, 0.0f, 2.0f);
            obj.transform.position = Camera.main.transform.TransformPoint(pos);
            obj.transform.LookAt(Camera.main.transform);
            obj.transform.Rotate(0.0f, 180.0f, 0.0f);
            //StartCoroutine(PlayVideo(obj.GetComponent<VideoPlayer>()));
            
            //キャンセルボタンをもとに戻す
            Image img_icon_video = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Image>();
            img_icon_video.sprite = Sprite.Create(texture_icon_video, new Rect(0, 0, texture_icon_video.width, texture_icon_video.height), Vector2.zero);
            captureCanvas.transform.Find("PhotoCaptureButton").gameObject.SetActive(true);
            //ビデオボタンの色を戻す
            ColorBlock cb = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Button>().colors;
            cb.normalColor = new Color(1.0f, 1.0f, 1.0f);
            cb.highlightedColor = new Color(1.0f, 1.0f, 1.0f);
            captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Button>().colors = cb;
            released = false;
            captureMode = 0;
#endif
            //stopRecordingTimer = float.MaxValue;
            //}
        }
    }

    private IEnumerator PlayVideo(VideoPlayer videoPlayer)
    {
        // We must set the audio before calling Prepare, otherwise it won't play the audio
        var audioSource = videoPlayer.GetComponent<AudioSource>();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);

        // Wait until ready
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
        //videoImage.texture = videoPlayer.texture;

        while (videoPlayer.isPlaying)
            yield return null;

        //onVideoFinished.Invoke(this);
    }

    /// AirTapイベントのハンドラ
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (GazeManager.Instance.HitObject != null)
        {
            //Debug.Log(GazeManager.Instance.HitObject.name);
            if (GazeManager.Instance.HitObject.name == "PhotoCaptureButton")
            {
                OnPhotoCaptureButtonClicked();
            }
            else if(GazeManager.Instance.HitObject.name == "VideoCaptureButton")
            {
                OnVideoCaptureButtonClicked();
            }
            else if(GazeManager.Instance.HitObject.name == "Text")
            {
                if(GazeManager.Instance.HitObject.transform.parent.name == "PhotoCaptureButton")
                {
                    OnPhotoCaptureButtonClicked();
                }
                else if(GazeManager.Instance.HitObject.transform.parent.name == "VideoCaptureButton")
                {
                    OnVideoCaptureButtonClicked();
                }
                else
                {
                    //Debug.Log("Capture 1");
                    Capture();
                }
            }
            else
            {
                //Debug.Log("Capture 2");
                Capture();
            }
        }
        else
        {
            //Debug.Log("Capture 3");
            Capture();
        }
    }

    void Capture()
    {
        // 撮影モードなら各モードで撮影
        if (captureMode == 1)
        {
            //Debug.Log("Tapped Camera Mode");
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
            }
        }
        else if (captureMode == 2)
        {
            //Debug.Log("Tapped Video Mode");
            if (released == true)
            {
                Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                //Debug.Log(cameraResolution);

                float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
                //Debug.Log(cameraFramerate);

#if WINDOWS_UWP
                VideoCapture.CreateAsync(true, delegate (VideoCapture videoCapture)
                {
                    if (videoCapture != null)
                    {
                        //Debug.Log("Created VideoCapture Instance!");
                        videoCaptureObject = videoCapture;
                        CameraParameters cameraParameters = new CameraParameters();
                        cameraParameters.hologramOpacity = 0.9f;
                        cameraParameters.frameRate = cameraFramerate;
                        cameraParameters.cameraResolutionWidth = cameraResolution.width;
                        cameraParameters.cameraResolutionHeight = cameraResolution.height;
                        cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
                        videoCaptureObject.StartVideoModeAsync(cameraParameters, VideoCapture.AudioState.ApplicationAndMicAudio, OnStartedVideoCaptureMode);
                    }
                    //else
                    //{
                    //    Debug.LogError("Failed to create VideoCapture Instance!");
                    //}
                });
#endif
                //stopRecordingTimer = Time.time + maxRecordingTime;
                //キャンセルボタンをもとに戻す
                Image img_icon_video = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Image>();
                img_icon_video.sprite = Sprite.Create(texture_icon_video, new Rect(0, 0, texture_icon_video.width, texture_icon_video.height), Vector2.zero);
                //ビデオボタンの色を赤くする
                ColorBlock cb = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Button>().colors;
                cb.normalColor = new Color(1.0f, 0.0f, 0.0f);
                cb.highlightedColor = new Color(1.0f, 0.0f, 0.0f);
                captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Button>().colors = cb;

                captureMode = 3;
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
        //string filename = dt.ToString("yyyyMMdd_HHmmss") + "_HoloLens.jpg";
        string filename = dt.ToString("yyyyMMdd_HHmmss") + "_HoloLens.png";
        //Debug.Log(filename);

        GameObject obj = GameObject.Instantiate(photoDisplayObject);
        obj.GetComponent<Renderer>().material.SetTexture("_MainTex", targetTexture);
        Vector3 pos = new Vector3(0.0f, 0.0f, 2.0f);
        obj.transform.position = Camera.main.transform.TransformPoint(pos);
        obj.transform.LookAt(Camera.main.transform);
        obj.transform.Rotate(0.0f, 180.0f, 0.0f);

#if WINDOWS_UWP
        //File.WriteAllBytes(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "/" + filename, texByte);
        File.WriteAllBytes(Windows.Storage.KnownFolders.CameraRoll.Path + "/" + filename, texByte);
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

        Image img_icon_camera = captureCanvas.transform.Find("PhotoCaptureButton").gameObject.GetComponent<Image>();
        img_icon_camera.sprite = Sprite.Create(texture_icon_camera, new Rect(0, 0, texture_icon_camera.width, texture_icon_camera.height), Vector2.zero);
        captureCanvas.transform.Find("VideoCaptureButton").gameObject.SetActive(true);
        released = false;
        captureMode = 0;
    }

    void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        DateTime dt = DateTime.Now;
        string filename = dt.ToString("yyyyMMdd_HHmmss") + "_HoloLens.mp4";
#if WINDOWS_UWP
        videoFilepath = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "/" + filename;
        //videoFilepath = Windows.Storage.KnownFolders.CameraRoll.Path + "/" + filename;        
        //videoFilepath = Windows.Storage.KnownFolders.CameraRoll.Path + "/" + "20181109_013040_HoloLens.mp4";        
        //videoFilepath = "C:/Data/Users/good_/Pictures/Camera Roll/20181109_013040_HoloLens.mp4";
#else
        videoFilepath = Application.streamingAssetsPath + "/" + filename;
#endif
        videoCaptureObject.StartRecordingAsync(videoFilepath, OnStartedRecordingVideo);
    }

    void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        //Debug.Log("Stopped Video Capture Mode!");
        videoCaptureObject.Dispose();
        videoCaptureObject = null;

        GameObject obj = GameObject.Instantiate(videoDisplayObject);
        obj.GetComponent<VideoPlayer>().source = VideoSource.Url;
#if WINDOWS_UWP
        obj.GetComponent<VideoPlayer>().url = videoFilepath;
        //obj.GetComponent<VideoPlayer>().url = Application.streamingAssetsPath + "/" + "20181108_161952_HoloLens.mp4";
#else
        obj.GetComponent<VideoPlayer>().url = Application.streamingAssetsPath + "/" + "20181108_161952_HoloLens.mp4";
#endif
        Vector3 pos = new Vector3(0.0f, 0.0f, 2.0f);
        obj.transform.position = Camera.main.transform.TransformPoint(pos);
        obj.transform.LookAt(Camera.main.transform);
        obj.transform.Rotate(0.0f, 180.0f, 0.0f);
        //StartCoroutine(PlayVideo(obj.GetComponent<VideoPlayer>()));

        //キャンセルボタンをもとに戻す
        Image img_icon_video = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Image>();
        img_icon_video.sprite = Sprite.Create(texture_icon_video, new Rect(0, 0, texture_icon_video.width, texture_icon_video.height), Vector2.zero);
        captureCanvas.transform.Find("PhotoCaptureButton").gameObject.SetActive(true);
        //ビデオボタンの色を戻す
        ColorBlock cb = captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Button>().colors;
        cb.normalColor = new Color(1.0f, 1.0f, 1.0f);
        cb.highlightedColor = new Color(1.0f, 1.0f, 1.0f);
        captureCanvas.transform.Find("VideoCaptureButton").gameObject.GetComponent<Button>().colors = cb;
        released = false;
        captureMode = 0;
    }

    void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        //Debug.Log("Started Recording Video!");
        //stopRecordingTimer = Time.time + maxRecordingTime;
    }

    void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        //Debug.Log("Stopped Recording Video!");
        videoCaptureObject.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

}
