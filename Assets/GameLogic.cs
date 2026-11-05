using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    const float SecondsBetweenBalloons = 1f;
    const int MaxUnpoppedBalloons = 30;

    // Positions are stored as a percent of the screen (0 to 1), so every client can place them on its own screen size.
    readonly Dictionary<int, Vector2> unpoppedBalloons = new Dictionary<int, Vector2>();
    int nextBalloonID;
    float secondsUntilNextBalloon;

    void Start()
    {
        NetworkServerProcessing.SetGameLogic(this);
    }

    void Update()
    {
        if (!NetworkServerProcessing.HasConnectedClients())
            return;

        secondsUntilNextBalloon -= Time.deltaTime;
        if (secondsUntilNextBalloon > 0)
            return;

        secondsUntilNextBalloon = SecondsBetweenBalloons;
        if (unpoppedBalloons.Count < MaxUnpoppedBalloons)
            SpawnBalloon();
    }

    void SpawnBalloon()
    {
        int balloonID = nextBalloonID++;
        Vector2 screenPercent = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        unpoppedBalloons.Add(balloonID, screenPercent);

        NetworkServerProcessing.SendMessageToAllClients(CreateSpawnMessage(balloonID, screenPercent), TransportPipeline.ReliableAndInOrder);
    }

    public void SendAllUnpoppedBalloonsTo(int clientConnectionID)
    {
        foreach (KeyValuePair<int, Vector2> balloon in unpoppedBalloons)
            NetworkServerProcessing.SendMessageToClient(CreateSpawnMessage(balloon.Key, balloon.Value), clientConnectionID, TransportPipeline.ReliableAndInOrder);
    }

    // The server decides who popped a balloon first: if two players click it at the same time, only the first request counts.
    public void TryPopBalloon(int balloonID, int requestingClientConnectionID)
    {
        if (!unpoppedBalloons.Remove(balloonID))
            return;

        Debug.Log("Balloon " + balloonID + " popped by client " + requestingClientConnectionID);
        string message = ServerToClientSignifiers.BalloonPopped + "," + balloonID;
        NetworkServerProcessing.SendMessageToAllClients(message, TransportPipeline.ReliableAndInOrder);
    }

    static string CreateSpawnMessage(int balloonID, Vector2 screenPercent)
    {
        // InvariantCulture keeps the decimal point a '.', so it can't be confused with the ',' separator.
        return ServerToClientSignifiers.SpawnBalloon + "," + balloonID + ","
            + screenPercent.x.ToString(CultureInfo.InvariantCulture) + ","
            + screenPercent.y.ToString(CultureInfo.InvariantCulture);
    }
}
