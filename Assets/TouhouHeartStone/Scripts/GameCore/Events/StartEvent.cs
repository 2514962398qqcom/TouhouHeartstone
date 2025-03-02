﻿using TouhouCardEngine;

namespace TouhouHeartstone
{
    class StartEvent : VisibleEvent
    {
        public StartEvent() : base("onStart")
        {
        }
        public override void execute(TouhouCardEngine.CardEngine engine)
        {
            foreach (Player player in engine.getProp<Player[]>("sortedPlayers"))
            {
                player["Init"].moveTo(player["Init"][0, player["Init"].count - 1], player["Hand"], 0);
            }
        }
        public override EventWitness getWitness(TouhouCardEngine.CardEngine engine, Player player)
        {
            EventWitness witness = new StartWitness();
            return witness;
        }
    }
    /// <summary>
    /// 游戏开始事件
    /// </summary>
    public class StartWitness : EventWitness
    {
        public StartWitness() : base("onStart")
        {
        }
    }
    public static partial class CardEngineExtension
    {
        public static void start(this TouhouCardEngine.CardEngine engine)
        {
            engine.doEvent(new StartEvent());
        }
    }
}