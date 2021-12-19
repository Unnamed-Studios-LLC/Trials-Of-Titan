using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Iap;

public enum StoreManagerState
{
    None,
    Loading,
    LoadingError,
    Ready
}

public abstract class StoreManager
{
    public StoreManagerState state { get; private set; } = StoreManagerState.Loading;

    protected StoreMenu menu;

    public StoreManager(StoreMenu menu)
    {
        this.menu = menu;
    }

    protected void SetState(StoreManagerState state)
    {
        this.state = state;
    }

    public abstract void Initialize();

    public abstract void StartPurchase(IapProduct product);

    public abstract void GetPrice(IapProduct product, out string priceString);

    public abstract void LateUpdate();

    public abstract void Enable();

    public abstract void Disable();
}
