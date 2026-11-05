using System.Collections.Generic;
using UnityEngine;

static public class NetworkServerProcessing
{
    static readonly HashSet<int> connectedClientIDs = new HashSet<int>();

    #region Send and Receive Data Functions
    static public void ReceivedMessageFromClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log("Network msg received =  " + msg + ", from connection id = " + clientConnectionID + ", from pipeline = " + pipeline);

        string[] csv = msg.Split(',');
        if (!int.TryParse(csv[0], out int signifier))
            return;

        if (signifier == ClientToServerSignifiers.PopBalloonRequest)
        {
            if (csv.Length == 2 && int.TryParse(csv[1], out int balloonID))
                gameLogic.TryPopBalloon(balloonID, clientConnectionID);
        }
    }

    static public void SendMessageToClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient(msg, clientConnectionID, pipeline);
    }

    static public void SendMessageToAllClients(string msg, TransportPipeline pipeline)
    {
        foreach (int clientConnectionID in connectedClientIDs)
            SendMessageToClient(msg, clientConnectionID, pipeline);
    }

    static public bool HasConnectedClients()
    {
        return connectedClientIDs.Count > 0;
    }

    #endregion

    #region Connection Events

    static public void ConnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client connection, ID == " + clientConnectionID);
        connectedClientIDs.Add(clientConnectionID);
        gameLogic.SendAllUnpoppedBalloonsTo(clientConnectionID);
    }

    static public void DisconnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client disconnection, ID == " + clientConnectionID);
        connectedClientIDs.Remove(clientConnectionID);
    }

    #endregion

    #region Setup
    static NetworkServer networkServer;
    static GameLogic gameLogic;

    static public void SetNetworkServer(NetworkServer NetworkServer)
    {
        networkServer = NetworkServer;
    }
    static public NetworkServer GetNetworkServer()
    {
        return networkServer;
    }
    static public void SetGameLogic(GameLogic GameLogic)
    {
        gameLogic = GameLogic;
    }

    #endregion
}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int PopBalloonRequest = 1;     // 1,balloonID
}

static public class ServerToClientSignifiers
{
    public const int SpawnBalloon = 1;          // 1,balloonID,xPercent,yPercent
    public const int BalloonPopped = 2;         // 2,balloonID
}

#endregion
