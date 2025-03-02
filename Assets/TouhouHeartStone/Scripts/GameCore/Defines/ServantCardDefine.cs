﻿using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class ServantCardDefine : CardDefine, ICost
    {
        public override CardDefineType type
        {
            get { return CardDefineType.servant; }
        }
        public abstract int cost { get; }
        public abstract int attack { get; }
        public abstract int life { get; }
        public abstract int category { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            else if (propName == nameof(attack))
                return (T)(object)attack;
            else if (propName == nameof(life))
                return (T)(object)life;
            else
                return base.getProp<T>(propName);
        }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            return null;
        }
    }
}