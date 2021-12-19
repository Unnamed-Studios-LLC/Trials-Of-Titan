using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IInteractable
{
    string[] InteractionOptions { get; }

    string InteractionTitle { get; }

    void Interact(int option);

    void OnEnter();

    void OnExit();
}
