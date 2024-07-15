using Photon.Pun;
using UnityEngine;

public class PlayerVideoChannel : MonoBehaviourPunCallbacks, IPunObservable
{
    AgoraManager agoraManager;

    string channelName = string.Empty;
    int channelId;
    int channelSize;

    public string ChannelName => channelName;
    public int ChannelId => channelId;
    public int ChannelSize => channelSize;

    void Awake()
    {
        if (!photonView.IsMine) return;
		
        var manager = GameObject.Find("Manager");
        agoraManager = manager.GetComponent<AgoraManager>();

        CreateNewChannel();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (!collision.CompareTag("Player")) return;

        var remotePlayerVideo = collision.GetComponent<PlayerVideoChannel>();
        if (remotePlayerVideo.ChannelName == ChannelName) return;

        if(remotePlayerVideo.ChannelSize >= ChannelSize && ChannelName != remotePlayerVideo.ChannelName)
        {
            agoraManager.Leave();

            channelId = remotePlayerVideo.ChannelId + 1;
            channelName = remotePlayerVideo.ChannelName;

            agoraManager.Join(channelName, channelId);
            channelSize = agoraManager.ChannelSize;
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (!collision.CompareTag("Player")) return;
		
		var remotePlayerVideo = collision.GetComponent<PlayerVideoChannel>();
		if(ChannelSize > 2)
		{
			agoraManager.RemoveUser((uint)remotePlayerVideo.ChannelId);
		}
		
        agoraManager.Leave();
        CreateNewChannel();
    }

    void CreateNewChannel()
    {
        GenerateRandomChannelID();
        
        channelId = 0;
        agoraManager.Join(ChannelName, ChannelId);
        channelSize = agoraManager.ChannelSize;
    }

    void GenerateRandomChannelID()
    {
        char[] stringChars = new char[12];
        System.Random random = new();

        for (int i = 0; i < stringChars.Length; i++)
        {
            int asciiIndex = random.Next(62);
            if (asciiIndex < 10)
            {
                stringChars[i] = (char)('0' + asciiIndex); // Digits 0-9
            }
            else if (asciiIndex < 36)
            {
                stringChars[i] = (char)('A' + asciiIndex - 10); // Uppercase letters A-Z
            }
            else
            {
                stringChars[i] = (char)('a' + asciiIndex - 36); // Lowercase letters a-z
            }
        }

        channelName = new(stringChars);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.Serialize(ref channelSize);
        stream.Serialize(ref channelName);
        stream.Serialize(ref channelId);
    }
}
