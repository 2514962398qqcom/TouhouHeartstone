﻿using System;
using System.Linq;
using System.Collections.Generic;

using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class Game
    {
        public CardEngine engine { get; }
        Dictionary<Player, IFrontend> dicPlayerFrontend { get; } = new Dictionary<Player, IFrontend>();
        public Game(IGameEnvironment env, bool shuffle = true, params CardDefine[] cards)
        {
            engine = new CardEngine(env, new HeartStoneRule(env), (int)DateTime.Now.ToBinary());
            engine.setProp("shuffle", shuffle);
            engine.afterEvent += afterEvent;
        }
        public int registerCardDefine(CardDefine define)
        {
            return engine.rule.pool.register(define);
        }
        public int[] getUsableCards(int playerIndex)
        {
            return engine.getPlayerAt(playerIndex)["Hand"].Concat(engine.getPlayerAt(playerIndex)["Skill"]).
                   Where(c => { return string.IsNullOrEmpty(isUsable(c)); }).
                   Select(c => { return c.id; }).ToArray();
        }
        public string isUsable(int rid)
        {
            return isUsable(engine.getCard(rid));
        }
        private string isUsable(Card card)
        {
            Player player = card.pile.owner;
            //通用规则
            if (player.getProp<int>("gem") < card.define.getProp<int>("cost"))//法力值不足不能用
                return "Unusable_NotEnoughGem";
            //if ((card.define.type == CardDefineType.spell ||
            //     card.define.type == CardDefineType.skill) &&
            //     getTargets(rid, "onUse").Length < 1)//法术，技能卡没有合适的目标不可以用
            //    return "Unusable_NoValidTarget";
            //卡片自己的规则
            return card.define.isUsable(engine, player, card);
        }
        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="frontend">实现接口的前端对象，Game会通过这个对象与前端进行沟通。</param>
        /// <param name="deck">玩家使用的卡组，数组的第一个整数代表玩家使用的角色卡，后30个是玩家使用的卡组。</param>
        public void addPlayer(IFrontend frontend, int[] deck)
        {
            Card[] cards = new Card[deck.Length - 1];
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i] = new Card(engine.rule.pool[deck[i + 1]]);
            }
            Player player = new Player(engine, new Pile[]
            {
                new Pile("Master",new Card(engine.rule.pool[deck[0]])),
                new Pile("Skill",new Card(engine.rule.pool[(engine.rule.pool[deck[0]] as MasterCardDefine).skillID])),
                new Pile("Deck",cards),
                new Pile("Init"),
                new Pile("Hand"),
                new Pile("Warp"),
                new Pile("Field"),
                new Pile("Grave")
            });
            engine.addPlayer(player);
            dicPlayerFrontend.Add(player, frontend);
        }
        /// <summary>
        /// 游戏初始化
        /// </summary>
        public void init()
        {
            engine.init();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="cardsRID"></param>
        public void initReplace(int playerIndex, int[] cardsRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            engine.initReplace(player, cardsRID.Select(id => { return player["Init"].First(c => { return c.id == id; }); }).ToArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="cardRID"></param>
        /// <param name="targetPosition"></param>
        /// <param name="targetCardRID"></param>
        public void use(int playerIndex, int cardRID, int targetPosition, int[] targetCardsRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            if (engine.getProp<Player>("currentPlayer") != player)
            {
                EventWitness witness = new UseWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.use_NotYourTurn);
                sendWitness(witness);
                return;
            }
            Card card = player["Hand"].First(c => { return c.id == cardRID; });
            if (player.getProp<int>("gem") < card.define.getProp<int>("cost"))
            {
                EventWitness witness = new UseWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.use_NoEnoughGem);
                sendWitness(witness);
                return;
            }
            Card[] targetCards = targetCardsRID.Select(targetCardRID => { return targetCardRID > -1 ? engine.getCard(targetCardRID) : null; }).ToArray();
            engine.use(player, card, targetPosition, targetCards);
        }
        public void attack(int playerIndex, int cardRID, int targetCardRID)
        {
            Player player = engine.getPlayerAt(playerIndex);
            if (engine.getProp<Player>("currentPlayer") != player)
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_NotYourTurn);
                sendWitness(witness);
                return;
            }
            Card card = engine.getCard(cardRID);
            if (!card.getProp<bool>("isReady"))
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_waitOneTurn);
                sendWitness(witness);
                return;
            }
            if (card.getProp<int>("attackTimes") > 0)
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_AlreadyAttacked);
                sendWitness(witness);
                return;
            }
            Card targetCard = engine.getCard(targetCardRID);
            if (targetCard.pile.owner["Field"].Any(c => { return c.getProp<bool>("taunt"); }) && targetCard.getProp<bool>("taunt") == false)
            {
                EventWitness witness = new AttackWitness();
                witness.setVar("error", true);
                witness.setVar("code", ErrorCode.attack_AttackTauntFirst);
                sendWitness(witness);
                return;
            }
            engine.attack(player, card, targetCard);
        }
        public void turnEnd(int playerIndex)
        {
            Player player = engine.getPlayerAt(playerIndex);
            engine.turnEnd(player);
        }
        public int getNextPlayerIndex(int playerIndex)
        {
            Player player = engine.getPlayerAt(playerIndex);
            Player[] sortedPlayers = engine.getProp<Player[]>("sortedPlayers");
            int index = Array.IndexOf(sortedPlayers, player);
            index++;
            if (index == sortedPlayers.Length)
                index = 0;
            return engine.getPlayerIndex(sortedPlayers[index]);
        }
        public int getCardDID(int cardRID)
        {
            return engine.getCard(cardRID).define.id;
        }
        public int[] getCardsDID(int[] cardsRID)
        {
            return cardsRID.Select(rid => { return getCardDID(rid); }).ToArray();
        }
        public bool isValidTarget(int cardRID, string effectName, int targetCardRID)
        {
            Card card = engine.getCard(cardRID);
            Card targetCard = engine.getCard(targetCardRID);
            Effect onUseEffect = card.define.effects?.FirstOrDefault(e => { return e.trigger == effectName; });
            return onUseEffect.checkTarget(engine, card.pile.owner, card, targetCard);
        }
        public int[] getTargets(int cardRID, string effectName)
        {
            Card card = engine.getCard(cardRID);
            Effect onUseEffect = card.define.effects?.FirstOrDefault(e => { return e.trigger == effectName; });
            if (onUseEffect != null)
                return engine.getCharacters(c => { return onUseEffect.checkTarget(engine, card.pile.owner, card, c); }).Select(c => { return c.id; }).ToArray();
            else
                return new int[0];
        }
        private void afterEvent(Event @event)
        {
            if (@event.parent == null)
            {
                foreach (Player player in engine.getPlayers())
                {
                    EventWitness[] wArray = generateWitnessTree(engine, player, @event);
                    for (int i = 0; i < wArray.Length; i++)
                    {
                        dicPlayerFrontend[player].sendWitness(wArray[i]);
                    }
                }
            }
        }
        void sendWitness(EventWitness witness)
        {
            foreach (Player player in engine.getPlayers())
            {
                dicPlayerFrontend[player].sendWitness(witness);
            }
        }
        EventWitness[] generateWitnessTree(CardEngine engine, Player player, Event e)
        {
            List<EventWitness> wlist = new List<EventWitness>();
            if (e is VisibleEvent)
            {
                EventWitness w = (e as VisibleEvent).getWitness(engine, player);
                for (int i = 0; i < e.before.Count; i++)
                {
                    wlist.AddRange(generateWitnessTree(engine, player, e.before[i]));
                }
                for (int i = 0; i < e.child.Count; i++)
                {
                    w.child.AddRange(generateWitnessTree(engine, player, e.child[i]));
                }
                wlist.Add(w);
                for (int i = 0; i < e.after.Count; i++)
                {
                    wlist.AddRange(generateWitnessTree(engine, player, e.after[i]));
                }
                return wlist.ToArray();
            }
            else
            {
                for (int i = 0; i < e.child.Count; i++)
                {
                    wlist.AddRange(generateWitnessTree(engine, player, e.child[i]));
                }
                return wlist.ToArray();
            }
        }
    }
}