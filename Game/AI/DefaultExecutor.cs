﻿using YGOSharp.OCGWrapper.Enums;
using System;
using System.Collections.Generic;

namespace WindBot.Game.AI
{
    public abstract class DefaultExecutor : Executor
    {
        private enum CardId
        {
            MysticalSpaceTyphoon = 5318639,
            ChickenGame = 67616300
        }

        protected DefaultExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            AddExecutor(ExecutorType.Activate, (int)CardId.ChickenGame, DefaultChickenGame);
        }

        protected bool DefaultMysticalSpaceTyphoon()
        {
            foreach (ClientCard card in CurrentChain)
                if (card.Id == (int)CardId.MysticalSpaceTyphoon)
                    return false;

            return DefaultStampingDestruction();
        }

        protected bool DefaultStampingDestruction()
        {
            List<ClientCard> spells = Enemy.GetSpells();
            if (spells.Count == 0)
                return false;

            ClientCard selected = Enemy.SpellZone.GetFloodgate();

            if (selected == null)
            {
                foreach (ClientCard card in spells)
                {
                    if (Duel.Player == 1 && !card.HasType(CardType.Continuous))
                        continue;
                    selected = card;
                    if (Duel.Player == 0 && card.IsFacedown())
                        break;
                }
            }

            if (selected == null)
                return false;
            AI.SelectCard(selected);
            return true;
        }

        protected bool DefaultGalaxyCyclone()
        {
            List<ClientCard> spells = Enemy.GetSpells();
            if (spells.Count == 0)
                return false;
            ClientCard selected = null;

            if (Card.Location == CardLocation.Grave)
            {
                selected = Enemy.SpellZone.GetFloodgate();
                if (selected == null)
                {
                    foreach (ClientCard card in spells)
                    {
                        if (!card.IsFacedown())
                        {
                            selected = card;
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (ClientCard card in spells)
                {
                    if (card.IsFacedown())
                    {
                        selected = card;
                        break;
                    }
                }
            }

            if (selected == null)
                return false;

            AI.SelectCard(selected);
            return true;
        }

        protected bool DefaultBookOfMoon()
        {
            if (AI.Utils.IsEnemyBetter(true, true))
            {
                ClientCard monster = Enemy.GetMonsters().GetHighestAttackMonster();
                if (monster != null && monster.HasType(CardType.Effect) && (monster.HasType(CardType.Xyz) || monster.Level > 4))
                {
                    AI.SelectCard(monster);
                    return true;
                }
            }
            return false;
        }

        protected bool DefaultCompulsoryEvacuationDevice()
        {
            ClientCard target = AI.Utils.GetProblematicMonsterCard();
            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }
            foreach (ClientCard card in Duel.ChainTargets)
            {
                if (Card.Equals(card))
                {
                    List<ClientCard> monsters = Enemy.GetMonsters();
                    foreach (ClientCard monster in monsters)
                    {
                        AI.SelectCard(monster);
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool DefaultCallOfTheHaunted()
        {
            if (!AI.Utils.IsEnemyBetter(true, true))
                return false;
            ClientCard selected = null;
            int BestAtk = 0;
            foreach (ClientCard card in Bot.Graveyard)
            {
                if (card.Attack > BestAtk)
                {
                    BestAtk = card.Attack;
                    selected = card;
                }
            }
            AI.SelectCard(selected);
            return true;
        }

        protected bool DefaultTorrentialTribute()
        {
            return (AI.Utils.IsEnemyBetter(true, true));
        }

        protected bool DefaultHeavyStorm()
        {
            return Bot.GetSpellCount() < Enemy.GetSpellCount();
        }

        protected bool DefaultHammerShot()
        {
            return AI.Utils.IsEnemyBetter(true, false);
        }

        protected bool DefaultDarkHole()
        {
            return AI.Utils.IsEnemyBetter(false, false);
        }

        protected bool DefaultRaigeki()
        {
            return AI.Utils.IsEnemyBetter(false, false);
        }

        protected bool DefaultSpellSet()
        {
            return Card.IsTrap() && Bot.GetSpellCountWithoutField() < 4;
        }

        protected bool DefaultTributeSummon()
        {
            foreach (ClientCard card in Bot.SpellZone)
            {
                if (card != null &&
                    card.Id == Card.Id &&
                    card.HasPosition(CardPosition.FaceUp))
                    return false;
            }
            int tributecount = (int)Math.Ceiling((Card.Level - 4.0d) / 2.0d);
            for (int j = 0; j < 7; ++j)
            {
                ClientCard tributeCard = Bot.MonsterZone[j];
                if (tributeCard == null) continue;
                if (tributeCard.Attack < Card.Attack)
                    tributecount--;
            }
            return tributecount <= 0;
        }

        protected bool DefaultField()
        {
            return Bot.SpellZone[5] == null;
        }

        protected bool DefaultMonsterRepos()
        {
            bool enemyBetter = AI.Utils.IsEnemyBetter(true, true);

            if (Card.IsAttack() && enemyBetter)
                return true;
            if (Card.IsDefense() && !enemyBetter && Card.Attack >= Card.Defense)
                return true;
            return false;
        }

        protected bool DefaultTrap()
        {
            return (LastChainPlayer == -1 && Duel.LastSummonPlayer != 0) || LastChainPlayer == 1;
        }

        protected bool DefaultUniqueTrap()
        {
            if (HasChainedTrap(0))
                return false;

            foreach (ClientCard card in Bot.SpellZone)
            {
                if (card != null &&
                    card.Id == Card.Id &&
                    card.HasPosition(CardPosition.FaceUp))
                    return false;
            }

            return true;
        }

        protected bool DefaultChickenGame()
        {
            int count = 0;
            foreach (CardExecutor exec in Executors)
            {
                if (exec.Type == Type && exec.CardId == Card.Id)
                    count++;
            }
            if (count > 1 || Duel.LifePoints[0] <= 1000)
                return false;
            if (Duel.LifePoints[0] <= Duel.LifePoints[1] && ActivateDescription == AI.Utils.GetStringId((int)CardId.ChickenGame, 0))
                return true;
            if (Duel.LifePoints[0] > Duel.LifePoints[1] && ActivateDescription == AI.Utils.GetStringId((int)CardId.ChickenGame, 1))
                return true;
            return false;
        }

        protected bool DefaultDimensionalBarrier()
        {
            if (Duel.Player != 0)
            {
                List<ClientCard> monsters = Enemy.GetMonsters();
                int[] levels = new int[13];
                bool tuner = false;
                bool nontuner = false;
                foreach (ClientCard monster in monsters)
                {
                    if (monster.HasType(CardType.Tuner))
                        tuner = true;
                    else if (!monster.HasType(CardType.Xyz))
                        nontuner = true;
                    if (monster.IsOneForXyz())
                    {
                        AI.SelectOption(3);
                        return true;
                    }
                    levels[monster.Level] = levels[monster.Level] + 1;
                }
                if (tuner && nontuner)
                {
                    AI.SelectOption(2);
                    return true;
                }
                for (int i=1; i<=12; i++)
                {
                    if (levels[i]>1)
                    {
                        AI.SelectOption(3);
                        return true;
                    }
                }
                ClientCard l = Enemy.SpellZone[6];
                ClientCard r = Enemy.SpellZone[7];
                if (l != null && r != null && l.LScale != r.RScale)
                {
                    AI.SelectOption(4);
                    return true;
                }
            }
            ClientCard lastchaincard = GetLastChainCard();
            if (LastChainPlayer == 1 && lastchaincard != null && !lastchaincard.IsDisabled())
            {
                if (lastchaincard.HasType(CardType.Ritual))
                {
                    AI.SelectOption(0);
                    return true;
                }
                if (lastchaincard.HasType(CardType.Fusion))
                {
                    AI.SelectOption(1);
                    return true;
                }
                if (lastchaincard.HasType(CardType.Synchro))
                {
                    AI.SelectOption(2);
                    return true;
                }
                if (lastchaincard.HasType(CardType.Xyz))
                {
                    AI.SelectOption(3);
                    return true;
                }
                if (lastchaincard.IsFusionSpell())
                {
                    AI.SelectOption(1);
                    return true;
                }
            }
            foreach (ClientCard card in Duel.ChainTargets)
            {
                if (Card.Equals(card))
                {
                    AI.SelectOption(3);
                    return true;
                }
            }
            return false;
        }
    }
}
