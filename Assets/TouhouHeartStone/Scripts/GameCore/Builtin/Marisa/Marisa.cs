﻿using TouhouCardEngine;

namespace TouhouHeartstone.Builtin
{
    public class MagicFairy : ServantCardDefine
    {
        public override int cost { get { return 1; } }
        public override int attack { get { return 1; } }
        public override int life { get { return 2; } }
        public override int category { get { return 2000; } }
        public override int id { get; set; } = 2002;
        public override Effect[] effects
        {
            get
            {
                return new Effect[]
                {
                    //new GeneratedEffect("")
                };
            }
        }
    }
    public class Marisa : MasterCardDefine
    {
        public override int id { get; set; } = 2000;
        public override int skillID
        {
            get { return 2001; }
        }
        public override int category
        {
            get { return 2000; }
        }
        public override Effect[] effects { get; } = new Effect[0];
    }
}
