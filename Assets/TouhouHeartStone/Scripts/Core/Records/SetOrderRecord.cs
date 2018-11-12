﻿using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class SetOrderRecord : Record
    {
        int[] _targetOrder = null;
        int[] _originOrder = null;
        public SetOrderRecord(int[] orderedPlayerId)
        {
            _targetOrder = orderedPlayerId;
        }
        public override Dictionary<int, Witness> apply(Game game)
        {
            if (game.players.orderedPlayers != null)
            {
                _originOrder = new int[game.players.orderedPlayers.Length];
                for (int i = 0; i < _originOrder.Length; i++)
                {
                    _originOrder[i] = game.players.orderedPlayers[i].id;
                }
            }
            else
                _originOrder = null;
            if (_targetOrder != null)
            {
                Player[] orderedPlayers = new Player[_targetOrder.Length];
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    orderedPlayers[i] = game.players.getPlayer(_targetOrder[i]);
                }
                game.players.orderedPlayers = orderedPlayers;
            }
            else
                game.players.orderedPlayers = null;
            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new SetOrderWitness(_targetOrder));
            }
            return dicWitness;
        }
        public override Dictionary<int, Witness> revert(Game game)
        {
            if (_originOrder != null)
            {
                Player[] orderedPlayers = new Player[_originOrder.Length];
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    orderedPlayers[i] = game.players.getPlayer(_originOrder[i]);
                }
                game.players.orderedPlayers = orderedPlayers;
            }
            else
                game.players.orderedPlayers = null;
            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new SetOrderWitness(_originOrder));
            }
            return dicWitness;
        }
    }
}