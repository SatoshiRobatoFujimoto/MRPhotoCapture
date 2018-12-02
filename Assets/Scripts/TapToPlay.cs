using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TapToPlay : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //StartCoroutine(PlayVideo(GetComponent<VideoPlayer>()));
    }
	
	// Update is called once per frame
	void Update () {
		
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
}
