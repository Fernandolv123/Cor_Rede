using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//Alternativas a la serialización: crear una lista global y serializar el propio indice
public class NetworkPlayer : NetworkBehaviour
{
    //por defecto las networks variables se comparten entre todos los usuarios
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    //Creamos una variable Int de tipo NetworkVariable para poder pasarla entre programas
    //Existe un problema con pasar una variable de tipo Material, y es que este tipo de datos no esta serializado, por l oque deberíamos hacerlo por nosotros mismos
    public NetworkVariable<int> Index = new NetworkVariable<int>();

    //Creamos una variable local con la lista de materiales con las que consta el jugador para cambiar
    public List<Material> listaMateriales;

    //OnNetworkSpawn se ejecutará siempre que se cree una nueva instancia del jugador
    public override void OnNetworkSpawn()
    {
        //La variable IsOwner devuelve true cuando el id del jugador sea el tuyo
        if (IsOwner)
        {
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
        int indice = Random.Range(0,listaMateriales.Count);
        while (indice == Index.Value){
            indice = Random.Range(0,listaMateriales.Count);
        }
        //Es necesario utilizar el .value para acceder al tipo de variable, si no accederemos a la network variable
        Index.Value = indice;
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
}