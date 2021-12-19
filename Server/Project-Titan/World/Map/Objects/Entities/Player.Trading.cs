using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using TitanDatabase.Models;

namespace World.Map.Objects.Entities
{
    public partial class Player
    {
        private Dictionary<string, TradeRequest> tradeRequests = new Dictionary<string, TradeRequest>();
        private List<TradeRequest> tradeRequestList = new List<TradeRequest>();

        private Trade currentTrade;

        private void TickTrading(ref WorldTime time)
        {
            TickTradeRequests();
        }

        private void TickTradeRequests()
        {
            if (tradeRequestList.Count == 0) return;

            for (int i = 0; i < tradeRequestList.Count; i++)
            {
                var request = tradeRequestList[i];
                if (DateTime.Now > request.timeoutTime)
                {
                    tradeRequestList.RemoveAt(i);
                    tradeRequests.Remove(request.otherPlayer.ToLower());
                    i--;
                }
            }
        }

        private bool TryGetTradeRequest(string from, out TradeRequest request)
        {
            return tradeRequests.TryGetValue(from.ToLower(), out request);
        }

        private void RemoveTradeRequest(TradeRequest request)
        {
            tradeRequests.Remove(request.otherPlayer.ToLower());
            tradeRequestList.Remove(request);
        }

        public void SendTradeRequest(Player to)
        {
            if (TryGetTradeRequest(to.playerName.Value, out var request))
            {
                RemoveTradeRequest(request);
                if (to.GetTradingWith() != null)
                {
                    AddChat(ChatData.Error($"{to.playerName.Value} is already trading."));
                    return;
                }

                CancelTrade();
                StartTrade(to);
                return;
            }
            
            // TODO implement rate limiting
            to.ReceiveTradeRequest(this);
        }

        private void ReceiveTradeRequest(Player from)
        {
            var name = from.playerName.Value;
            if (TryGetTradeRequest(name, out var request))
                request.RefreshTimeout();
            else
            {
                request = new TradeRequest(name);
                tradeRequests[name.ToLower()] = request;
                tradeRequestList.Add(request);
            }
            client.SendAsync(new TnTradeRequest(name));
        }

        private void StartTrade(Player other)
        {
            currentTrade = new Trade(this, other);
            other.JoinTrade(currentTrade);
        }

        private void JoinTrade(Trade trade)
        {
            currentTrade = trade;
        }

        private Item[] GetTradeItems()
        {
            var items = new Item[8];
            for (int i = 0; i < items.Length; i++)
            {
                var item = GetItem(i + 4);
                items[i] = item == null ? Item.Blank : item.itemData;
            }
            return items;
        }

        private void EndTrade()
        {
            currentTrade = null;
        }

        private List<ServerItem> GetOfferItems(TradeOffer offer)
        {
            var items = new List<ServerItem>();
            for (int i = 0; i < 8; i++)
                if (offer[i])
                    items.Add(GetItem(i + 4));
            return items;
        }

        private List<ServerItem> TakeOfferItems(TradeOffer offer)
        {
            var items = new List<ServerItem>();
            for (int i = 0; i < 8; i++)
                if (offer[i])
                {
                    var item = GetItem(i + 4);
                    SetItem(i + 4, null);
                    items.Add(item);
                }
            return items;
        }

        private void GiveItems(List<ServerItem> items)
        {
            int item = 0;
            for (int i = 4; i < 12 && item < items.Count; i++)
            {
                if (GetItem(i) != null) continue;
                SetItem(i, items[item++]);
            }
        }

        private int GetOpenSlotCount()
        {
            int count = 0;
            for (int i = 4; i < 12; i++)
                if (GetItem(i) == null)
                    count++;
            return count;
        }

        public Player GetTradingWith()
        {
            if (currentTrade == null) return null;
            return currentTrade.GetOtherPlayer(this);
        }

        public void UpdateTrade(TnTradeUpdate update)
        {
            currentTrade?.UpdateOffer(this, update);
        }

        public void CancelTrade()
        {
            currentTrade?.CancelTrade(this);
        }

        private class TradeRequest
        {
            public string otherPlayer;

            public DateTime timeoutTime;

            public TradeRequest(string playerName)
            {
                otherPlayer = playerName;
                RefreshTimeout();
            }

            public void RefreshTimeout()
            {
                timeoutTime = DateTime.Now.AddSeconds(20);
            }
        }

        private class Trade
        {
            private Player playerA;
            private Player playerB;

            private TradeOffer offerA;
            private TradeOffer offerB;

            private int tradeVersion;

            private bool acceptedA;
            private bool acceptedB;

            public Trade(Player playerA, Player playerB)
            {
                this.playerA = playerA;
                this.playerB = playerB;

                playerA.client.SendAsync(new TnTradeStart(playerB.gameId, playerB.GetTradeItems()));
                playerB.client.SendAsync(new TnTradeStart(playerA.gameId, playerA.GetTradeItems()));
            }

            public void UpdateOffer(Player player, TnTradeUpdate update)
            {
                if (update.accepted)
                {
                    AcceptTrade(player, update.version);
                    return;
                }

                acceptedA = false;
                acceptedB = false;

                if (player == playerA)
                    offerA = update.offer;
                else if (player == playerB)
                    offerB = update.offer;

                SendUpdates();
            }

            private void SendUpdates()
            {
                tradeVersion++;
                playerA.client.SendAsync(new TnTradeUpdate(tradeVersion, acceptedB, offerB));
                playerB.client.SendAsync(new TnTradeUpdate(tradeVersion, acceptedA, offerA));
            }

            public void AcceptTrade(Player player, int version)
            {
                if (version != tradeVersion)
                {
                    SendUpdates();
                    return;
                }

                if (!ValidTrade())
                {
                    if (player == playerA)
                        playerA.client.SendAsync(new TnTradeUpdate(tradeVersion, acceptedB, offerB));
                    else
                        playerB.client.SendAsync(new TnTradeUpdate(tradeVersion, acceptedA, offerA));
                    return;
                }

                if (player == playerA)
                    acceptedA = true;
                else if (player == playerB)
                    acceptedB = true;

                if (acceptedA && acceptedB)
                {
                    ExecuteTrade();
                }
                else
                {
                    tradeVersion++;
                    if (player == playerA)
                        playerB.client.SendAsync(new TnTradeUpdate(tradeVersion, acceptedA, offerA));
                    else
                        playerA.client.SendAsync(new TnTradeUpdate(tradeVersion, acceptedB, offerB));
                }
            }

            public void CancelTrade(Player player)
            {
                var other = player == playerA ? playerB : playerA;
                other.client.SendAsync(new TnTradeResult(TradeResult.Cancelled));
                other.AddChat(ChatData.Error($"{player.playerName.Value} has cancelled the trade"));

                EndTrade();
            }

            private bool ValidTrade()
            {
                var itemsA = playerA.GetOfferItems(offerA);
                var itemsB = playerB.GetOfferItems(offerB);

                if (ContainsSoulbound(itemsA) || ContainsSoulbound(itemsB)) return false;

                var openSlotsA = playerA.GetOpenSlotCount();
                var openSlotsB = playerB.GetOpenSlotCount();

                if (openSlotsA + itemsA.Count < itemsB.Count) return false;
                if (openSlotsB + itemsB.Count < itemsA.Count) return false;

                return true;
            }

            private bool ContainsSoulbound(IEnumerable<ServerItem> items)
            {
                foreach (var item in items)
                {
                    if (item == null) continue;
                    if (item.itemData.soulbound)
                        return true;
                }
                return false;
            }

            private void ExecuteTrade()
            {
                var itemsFromA = playerA.TakeOfferItems(offerA);
                var itemsFromB = playerB.TakeOfferItems(offerB);

                playerA.GiveItems(itemsFromB);
                playerB.GiveItems(itemsFromA);

                playerA.client.Send(new TnTradeResult(TradeResult.Success));
                playerB.client.Send(new TnTradeResult(TradeResult.Success));

                EndTrade();
            }

            private void EndTrade()
            {
                playerA.EndTrade();
                playerB.EndTrade();
            }

            public Player GetOtherPlayer(Player player)
            {
                return player == playerA ? playerB : playerA;
            }
        }

    }
}
