using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : MonoBehaviour
{
    //NetworkManager contiene un singleton y variables para comprobar el modo de ejecución (server host o cliente)
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        //El caso de host es u ncaso extraño, pues es cliente y servidor al mismo tiempo, por lo que no es necesario realizar esa comprobación específica
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();

            SubmitNewColor();
        }

    GUILayout.EndArea();
    }

    static void StartButtons()
    {
        //En este punto se inicairá el tipo de conexión
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        string mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Joined as: " + mode);
    }

    static void SubmitNewColor()
    {
        //Como un host también es servidor, esta comprobación dará true para host y para servidor y false para cliente únicamente
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Change Material" : "Request Material Change"))
        {
            /*Aqui se comprueba que sea cliente puro, com ohemos visto antes un host es servidor, pero también ha de ser cliente, por lo que esta comprobación será false para host
              P.D: también se podría haber utilizado un NetworkManager.Singleton.IsHost, unity sabrá porque no lo ha hecho*/
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient )
            {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                    NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<NetworkPlayer>().ChangeMaterial();
            }
            else
            {
                NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                NetworkPlayer player = playerObject.GetComponent<NetworkPlayer>();
                player.ChangeMaterial();
            }
        }
    }
}
