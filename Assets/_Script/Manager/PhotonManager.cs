using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviourPunCallbacks
{

    [SerializeField] Button connectBtn;
    [SerializeField] TextMeshProUGUI loadText;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        connectBtn.onClick.AddListener(OnConnectedToMaster);
        connectBtn.onClick.AddListener(() => connectBtn.gameObject.SetActive(false));
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master network");
        connectBtn.gameObject.SetActive(true);
        loadText.gameObject.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(0, 0, 0), Quaternion.identity);
    }

    public void JoinDevGame()
    {
        RoomOptions roomOptions = new()
        {
            IsVisible = false,
            MaxPlayers = 4
        };

        PhotonNetwork.JoinOrCreateRoom("DevGame", roomOptions, TypedLobby.Default);
    }
}
