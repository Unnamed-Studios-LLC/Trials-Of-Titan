using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Server;

public class StatsLabel : MonoBehaviour
{
    private TextMeshProUGUI label;

    private Queue<float> fpsNumbers = new Queue<float>();

    public World world;

    private DateTime lastPing;

    private DateTime nextPing;

    private int ping = 0;

    private bool addedHandler = false;

    // Use this for initialization
    void Start()
    {
        label = GetComponent<TextMeshProUGUI>();

        nextPing = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        fpsNumbers.Enqueue(Time.smoothDeltaTime);
        while (fpsNumbers.Count > 10)
            fpsNumbers.Dequeue();

        float fps = 0;
        foreach (float delta in fpsNumbers)
        {
            fps += delta;
        }
        fps /= fpsNumbers.Count;
        label.text = "FPS: " + Mathf.RoundToInt(1.0f / fps).ToString();

        label.text += "\n";

        label.text += "Ping: " + ping.ToString();

        if (world.gameManager.ClientReady)
        {
            if (!addedHandler)
            {
                addedHandler = true;
                world.gameManager.client.AddHandler<TnPong>(HandlePong);
            }

            if (DateTime.Now >= nextPing)
            {
                lastPing = DateTime.Now;
                nextPing = lastPing.AddSeconds(5);

                world.gameManager.client.SendAsync(new TnPing());
            }
        }
    }

    private void HandlePong(TnPong pong)
    {
        var ms = (DateTime.Now - lastPing).TotalMilliseconds;
        ping = (int)ms;
    }
}
