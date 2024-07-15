using Agora.Rtc;
using System.Collections.Generic;
using UnityEngine;

public class AgoraManager : MonoBehaviour
{
    // Fill in your app ID
    [SerializeField] string appID = "f1edd80ad3684323ab7ae02793490580";

    [Space(15)]
    [SerializeField] RectTransform panel;
    [SerializeField] GameObject remoteView;

    IRtcEngine RtcEngine;

    readonly List<VideoSurface> videos = new();
    
    public int ChannelSize => videos.Count;

    void Start()
    {
        SetupVideoSDKEngine();
        InitEventHandler();
    }

    private void SetupVideoSDKEngine()
    {
        // Create an IRtcEngine instance
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        RtcEngineContext context = new()
        {
            appId = appID,
            channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT
        };

        // Initialize the instance
        RtcEngine.Initialize(context);
    }

    private void InitEventHandler()
    {
        RtcEngine.InitEventHandler(new UserEventHandler(this));
    }

    public void PlayerChannel()
    {
        var view = Instantiate(remoteView, panel.transform);
        var viewVideo = view.GetComponent<VideoSurface>();

        viewVideo.SetForUser();
        viewVideo.SetEnable(true);
        videos.Add(viewVideo);
    }

    public void UserJoined(uint id, RtcConnection connection)
    {
        var view = Instantiate(remoteView, panel.transform);
        var viewVideo = view.GetComponent<VideoSurface>();

        viewVideo.SetForUser(id, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        viewVideo.SetEnable(true);
        videos.Add(viewVideo);
    }
	
	public void RemoveUser(uint id)
	{
		 foreach (var video in videos)
        {
            if (video.ID == id)
            {
                videos.Remove(video);
                return;
            }
        }
	}	

    public void UserLeave(uint id)
    {
        foreach (var video in videos)
        {
            if (video.ID == id)
            {
                videos.Remove(video);
                Destroy(video.gameObject);
                return;
            }
        }
    }

    public void Leave()
    {
        Debug.Log("Leaving channel");
        RtcEngine.LeaveChannel();
        RtcEngine.DisableVideo();
        RtcEngine.DisableAudio();

        videos.ForEach(video => { Destroy(video.gameObject); });
        videos.Clear();
    }

    public void Join(string channelName, int id)
    {
        // Enable the video module
        RtcEngine.EnableVideo();
        RtcEngine.EnableAudio();

        // Set channel media options
        ChannelMediaOptions options = new();
        options.autoSubscribeAudio.SetValue(true);
        options.autoSubscribeVideo.SetValue(true);
        options.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        RtcEngine.JoinChannel(string.Empty, channelName, (uint)id, options);
    }

    void OnApplicationQuit()
    {
        if (RtcEngine != null)
        {
            Leave();
            // Destroy IRtcEngine
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }
}

// Implement your own EventHandler class by inheriting the IRtcEngineEventHandler interface class implementation
internal class UserEventHandler : IRtcEngineEventHandler
{
    private readonly AgoraManager manager;
    internal UserEventHandler(AgoraManager videoSample)
    {
        manager = videoSample;
    }

    // error callback
    public override void OnError(int err, string msg)
    {
    }

    // Triggered when a local user successfully joins the channel
    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        Debug.Log($"Joined Channel {connection.channelId}");
        manager.PlayerChannel();
    }

    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        manager.UserJoined(uid, connection);
        Debug.Log("Remote user joined");
    }

    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        manager.UserLeave(uid);
        Debug.Log($"User leave. id => {uid}. {reason}");
    }
}