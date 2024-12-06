using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using WebSocketSharp;
using System.Net.Http; // Required for Network

public class NetworkButtons : MonoBehaviour
{
    void OnGUI(){
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer){
            if(GUILayout.Button("Host Game")){
                NetworkManager.Singleton.StartHost();
            }
            if(GUILayout.Button("Join Game")){
                NetworkManager.Singleton.StartClient();

            }
            // string ipField = GUILayout.TextField("", GUILayout.Width(200));
            // if(ipField != ""){
            //     NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(ipField);
            //     if(GUILayout.Button("Connect to IP")){
            //         NetworkManager.Singleton.StartClient();
            //     }
            // }
        }

        GUILayout.EndArea();
    }
}
