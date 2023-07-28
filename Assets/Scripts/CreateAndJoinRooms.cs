using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField m_CreateInput;
    public TMP_InputField m_JoinInput;

   public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(m_CreateInput.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(m_JoinInput.text);
    }

    public void OpenK()
    {
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("SampleScene");
    }
}
