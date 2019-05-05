﻿using System;
using System.Collections.Generic;

namespace TouhouHeartstone.Backend
{
    public class GeneratedCardDefine : CardDefine
    {
        Dictionary<string, object> dicProp { get; } = new Dictionary<string, object>();
        public void setProp<T>(string propName, T value)
        {
            dicProp[propName] = value;
        }
        public override T getProp<T>(string propName)
        {
            if (dicProp.ContainsKey(propName) && dicProp[propName] is T)
                return (T)dicProp[propName];
            else
                return default;
        }
        public override int id
        {
            get { return getProp<int>("id"); }
        }
        public CardType type
        {
            get { return (CardType)getProp<int>("type"); }
        }
    }
}