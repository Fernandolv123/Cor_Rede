using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

//Alternativas a la serialización: crear una lista global y serializar el propio indice
public class NetworkPlayer : NetworkBehaviour
{
    //Creo una variable privada para guardar la razon de desconexión
    private string disconnectReason;
    
    //por defecto las networks variables se comparten entre todos los usuarios
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    //Creamos una variable Int de tipo NetworkVariable para poder pasarla entre programas
    //Existe un problema con pasar una variable de tipo Material, y es que este tipo de datos no esta serializado, por l oque deberíamos hacerlo por nosotros mismos
    public NetworkVariable<int> Index = new NetworkVariable<int>();
    //Creamos una varialbe booleana para no ejecutar cierta parte del código por primera vez en el SubmitNewMaterialRpc()
    private bool firstime=true;
    //public NetworkList<int> listIndex = new NetworkList<int>();

    //Creamos una variable local con la lista de materiales con las que consta el jugador para cambiar
    public List<Material> listaMateriales;

    //OnNetworkSpawn se ejecutará siempre que se cree una nueva instancia del jugador
    public override void OnNetworkSpawn()
    {
        //Comprobamos el numero de conexiones en el lado del servidor
        if(NetworkManager.Singleton.IsServer){
            //Desconectamos al ultimo cliente añadido
            if (NetworkGameManager.ServerCapped()){
                //Pongo un tiempo de buffer para esperar a que el NetworkManager haga su trabajo y evitar excepciones por race conditions
                Invoke("KillList",0.005f);
                return;
            }
        }
        //La variable IsOwner devuelve true cuando el id del jugador sea el tuyo
        if (IsOwner)
        {
            //suscribimos al usuario para que sea notificado en caso de desconexión
            NetworkManager.OnClientDisconnectCallback += Disconected();
            //Por tanto aqui solo entrará para el jugador recien spawneado
            ChangeInitialPositionRpc();
            ChangeMaterial();
        }
    }

    public void ChangeMaterial(){
        SubmitNewMaterialRpc();
    }

    /*RPC remote procedure call llamar a una funcon que esta en otro ordenador, al poner server rpc, llama a una funcion del servidor
    este send to server provoca que se pueda hacer una llamada a este método desde fuera de este programa u ordenador*/
    [Rpc(SendTo.Server)]
    //cambiamos la posición inicial para los nuevos jugadores
    //Es IMPERATIVO que este tipo de métodos acaben en 'Rpc', si no provoca errores de compilación
    void ChangeInitialPositionRpc(RpcParams rpcParams = default)
    {
        var randomPosition = GetRandomPositionOnPlane();
        transform.position = randomPosition;
        Position.Value = randomPosition;
    }

    [Rpc(SendTo.Server)]
    void SubmitNewMaterialRpc(RpcParams rpcParams = default){
        //Esta es la comprobación de entrada para determinar los colores y sus contrapartes en la lista
        /*Debug.Log(NetworkGameManager.instance.materialesSinDueño.Count+"-----Entrando-----"+Index.Value);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[0]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[1]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[2]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[3]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[4]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[5]);*/

        int newMaterial = Random.Range(0,NetworkGameManager.instance.materialesSinDueño.Count);
        while (NetworkGameManager.instance.materialesSinDueño[newMaterial] == null){
            newMaterial = Random.Range(0,NetworkGameManager.instance.materialesSinDueño.Count);
        }
        //La NetworkVariable de Index se establece por defecto a 0, lo que significa que si no ponemos esta comprobación y el último color es el correspondiente al indice 0, este se pondrá como disponible a pesar de estar en uso
        //Esto es debido a que debería haber hecho una diferencia entre métodos de creación, y de cambio, perteneciendo este a estos últimos, el método de inicialización no debería contener esta parte
        if (firstime){
            firstime = false;
        } else {
            //Ponemos como disponible el material anterior antes de cambiar al nuevo
            NetworkGameManager.instance.materialesSinDueño[Index.Value] = listaMateriales[Index.Value];
        }

        //Es necesario utilizar el .value para acceder al tipo de variable, si no accederemos a la network variable
        Index.Value = newMaterial;

        //ponemos el material actual como no disponible
        NetworkGameManager.instance.materialesSinDueño[Index.Value] = null;

        //Esta es la comprobación de salida para determinar los colores y sus contrapartes en la lista
        /*Debug.Log(NetworkGameManager.instance.materialesSinDueño.Count+"-----Saliendo-----"+Index.Value);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[0]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[1]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[2]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[3]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[4]);
        Debug.Log(NetworkGameManager.instance.materialesSinDueño[5]);*/
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    void Update()
    {
        GetComponent<MeshRenderer>().material = listaMateriales[Index.Value];
        transform.position = Position.Value;
    }

    public System.Action<ulong> Disconected(){
        //este método se llamará cuando se desconecte el usuario
        //Todavia no entiendo como funciona Disconnected Reason, seguiré investigando
        Debug.Log("You've been disconected for: "+NetworkManager.Singleton.DisconnectReason);
        return null;
    }

    public void KillList(){
        disconnectReason = "Maximun number of users reached";
        NetworkManager.Singleton.DisconnectClient(NetworkManager.ConnectedClientsIds[NetworkManager.ConnectedClientsIds.Count-1],"Maximun number of users reached");
        //Todavia no entiendo como funciona Disconnected Reason, seguiré investigando
        Debug.Log(NetworkManager.Singleton.DisconnectReason);
    }
}
