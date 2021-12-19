using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public interface IRequest
{

    void OnAccept();

    void OnDecline();
}

public class RequestPanel : MonoBehaviour
{
    public TextMeshProUGUI titleLabel;

    private IRequest request;

    public void Request(string title, IRequest request)
    {
        titleLabel.text = title;
        this.request = request;
        Show();
    }

    public void Accept()
    {
        request?.OnAccept();
        Hide();
    }

    public void Decline()
    {
        request?.OnDecline();
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);

    }
}
