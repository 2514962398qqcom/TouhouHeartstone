﻿using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class SummonEvent : VisibleEvent
    {
        public SummonEvent(Player player, Card card, int position) : base("onSummon")
        {
            this.player = player;
            this.card = card;
            this.position = position;
        }
        Player player { get; }
        Card card { get; }
        int position { get; }
        public override void execute(TouhouCardEngine.CardEngine engine)
        {
            card.pile.moveTo(card, player["Field"], position);
            if (card.define is ServantCardDefine)
            {
                card.setProp("life", (card.define as ServantCardDefine).life);
                card.setProp("attack", (card.define as ServantCardDefine).attack);
                card.setProp("isReady", false);
            }
            else if (card.define is GeneratedCardDefine)
            {
                card.setProp("life", card.define.getProp<int>("life"));
                card.setProp("attack", card.define.getProp<int>("attack"));
                card.setProp("isReady", false);
            }
        }
        public override EventWitness getWitness(TouhouCardEngine.CardEngine engine, Player player)
        {
            EventWitness witness = new SummonWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("cardRID", card.id);
            witness.setVar("cardDID", card.define.id);
            witness.setVar("position", position);
            return witness;
        }
    }
    /// <summary>
    /// 召唤随从事件
    /// </summary>
    public class SummonWitness : EventWitness
    {
        /// <summary>
        /// 召唤随从的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 被召唤的卡片RID
        /// </summary>
        public int cardRID
        {
            get { return getVar<int>("cardRID"); }
        }
        /// <summary>
        /// 被召唤的卡片DID
        /// </summary>
        public int cardDID
        {
            get { return getVar<int>("cardDID"); }
        }
        /// <summary>
        /// 被召唤的随从位于战场上的位置
        /// </summary>
        public int position
        {
            get { return getVar<int>("position"); }
        }
        public SummonWitness() : base("onSummon")
        {
        }
    }
}