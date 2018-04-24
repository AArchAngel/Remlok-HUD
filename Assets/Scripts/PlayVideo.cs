using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayVideo : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // Will attach a VideoPlayer to the main camera.
         GameObject camera = GameObject.Find("RawImage");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        //  var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

        var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

        // This will cause our scene to be visible through the video being played.
        videoPlayer.targetCameraAlpha = 0.5F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        videoPlayer.url = "https://redirector.googlevideo.com/videoplayback?signature=9546E04F9C3287B91536FD6E968C1737038871F3.4A637782BD2D48E0208DBDBF41B89F6D65B4600D&ipbits=0&initcwndbps=1396250&expire=1523296364&c=WEB&ei=DFTLWoTbJYjDVPD7kqAC&key=yt6&lmt=1523103538576194&itag=22&mn=sn-ab5l6n6s%2Csn-p5qlsnsk&ip=2001%3A19f0%3A7402%3A95%3A5400%3Aff%3Afe6a%3Ad50a&mm=31%2C29&sparams=dur%2Cei%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&dur=427.247&mv=m&pl=53&mt=1523274676&ratebypass=yes&requiressl=yes&ms=au%2Crdu&fvip=1&mime=video%2Fmp4&source=youtube&id=o-AG78JAkCXAyGQl2zCo7RU3k47FIkLFVsrIxicLrLcFQK";

        // Skip the first 100 frames.
        videoPlayer.frame = 100;

        // Restart from beginning when done.
        videoPlayer.isLooping = true;

        // Each time we reach the end, we slow down the playback by a factor of 10.
     

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        videoPlayer.Play();
    }
	

}
