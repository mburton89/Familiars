using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tiers of AI
/*
 * Wild - Basically attacks randomly. If it has attacks it can use, it will 
 *              use one randomly, targeting randomly, only moving as a last option.
 * Amateur - Has some basic strategy. Knows basic type effectiveness when
 *              attacking. does not consider position unless it cant attack at all.
 * Talented - Has higher level strategy. Knows type effectiveness for 
 *              offense and defense. Tries to place themselves in positions to make the best attacks
 * Professional - Has highest level strategy. Knows both type effective 
 *              and general stats. Will assign threat priority accurately. 
 * 
 */
public enum AILevel { Wild, Amateur, Talented, Professional }

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;

    private void Awake()
    {
        Instance = this;
    }

    public AttackAction FindAttack(CombatUnit user, Field field, AILevel aiLevel)
    {
        switch (aiLevel)
        {
            case AILevel.Wild:
                return WildAI(user, field);
            case AILevel.Amateur:
                return WildAI(user, field);
            case AILevel.Talented:
                return WildAI(user, field);
            case AILevel.Professional:
                return WildAI(user, field);
        }
        return new AttackAction(user, new List<CombatUnit>(), null);

    }

    AttackAction WildAI(CombatUnit user, Field field)
    {
        List<CombatUnit> _possibleTargets = new List<CombatUnit>();
        bool _valid = false;
        Attack _attack = user.Familiar.GetRandomAttack();
        int iteration = 0;
        int _preview = 0;
        // Check to see if the current attack has any valid targets;
        Debug.Log(_attack.Base.Name);
        while (!(_valid || iteration > 50))
        {
            if (_attack.Base.Sources.Active[user.x * 3 + user.y])
            {
                _preview = 0;
                for (int i = 0; i < _attack.Base.SourceArray.Length; i++)
                {
                    if (_attack.Base.SourceArray[i].Active[user.x * 3 + user.y])
                    {
                        _preview = i;
                    }
                }
                switch (_attack.Base.AttackStyle)
                {
                    case AttackStyle.Target:
                        for (int i = 0; i < _attack.Base.Targets.Active.Length; i++)
                        {
                            if (field.GetTile(i).familiarOccupant != null && _attack.Base.TargetArray[_preview].Active[i])
                            {
                                _possibleTargets.Add(field.GetTile(i).familiarOccupant);
                            }
                        }
                        break;
                    case AttackStyle.Projectile:
                        List<Tile> _t = MatchingTiles(field, _attack.Base.TargetArray[_preview], _attack.Base.EligibleOrigins);
                        if (_t.Count == 1)
                        {
                            Tile _checkingTile = _t[0];

                            int _position = (_checkingTile.x * 3) + _checkingTile.y;
                            bool end;
                            while (_checkingTile != null)
                            {
                                end = false;
                                if (_checkingTile.familiarOccupant != null)
                                {
                                    _possibleTargets.Add(_checkingTile.familiarOccupant);
                                    end = true;
                                    break;
                                }

                                switch (_attack.Base.Direction)
                                {
                                    case 0:
                                        if (_position >= 6)
                                        {
                                            end = true;
                                            break;
                                        }
                                        else _position += 3;
                                        break;

                                }
                                if (!end)
                                {
                                    _checkingTile = field.GetTile(_position);
                                }
                                else
                                {
                                    _checkingTile = null;
                                }
                            }
                        }
                        break;
                    case AttackStyle.Area:
                        for (int i = 0; i < 9; i++)
                        {
                            #region Skips
                            if (i % 3 > _attack.Base.UpperX)
                                continue;
                            if (i % 3 < _attack.Base.LowerX)
                                continue;
                            if (i / 3 > _attack.Base.UpperY)
                                continue;
                            if (i / 3 < _attack.Base.LowerY)
                                continue;
                            #endregion

                            List<CombatUnit> _targetCheck = field.AICheckAreaAttack(i, _attack.Base.TargetArray[_preview]);
                            if (_targetCheck.Count > 0)
                            {
                                _possibleTargets = _targetCheck;
                            }
                        }
                        break;
                    case AttackStyle.AreaStatic:
                        for (int i = 0; i < 9; i++)
                        {
                            if (field.GetTile(i).familiarOccupant != null && _attack.Base.Targets.Active[i])
                            {
                                _possibleTargets.Add(field.GetTile(i).familiarOccupant);
                            }
                        }
                        break;

                }

                if (_possibleTargets.Count > 0)
                {
                    _valid = true;
                    break;
                }
            }

            _attack = user.Familiar.GetRandomAttack();
            iteration++;
        }

        List<CombatUnit> _targets = new List<CombatUnit>();

        // Select targets
        if (_valid)
        {
            switch (_attack.Base.AttackStyle)
            {
                case AttackStyle.Target:
                    _targets.Add(_possibleTargets[Random.Range(0, _possibleTargets.Count)]);
                    break;
                case AttackStyle.Projectile:
                    _targets.Add(_possibleTargets[0]);
                    break;
                case AttackStyle.Area:
                    List<List<CombatUnit>> _targetSets = new List<List<CombatUnit>>();
                    for (int i = 0; i < 9; i++)
                    {
                        #region Skips
                        if (i % 3 > _attack.Base.UpperX)
                            continue;
                        if (i % 3 < _attack.Base.LowerX)
                            continue;
                        if (i / 3 > _attack.Base.UpperY)
                            continue;
                        if (i / 3 < _attack.Base.LowerY)
                            continue;
                        #endregion

                        List<CombatUnit> _targetCheck = field.AICheckAreaAttack(i, _attack.Base.TargetArray[_preview]);
                        
                        if (_targetCheck.Count > 0)
                        {
                            _targetSets.Add(_targetCheck);
                        }
                    }

                    for (int i = 0; i < _targetSets.Count; i++)
                    {
                        _targets.Add(_targetSets[i][Random.Range(0, _targetSets[i].Count)]);
                    }
                    
                    break;
                case AttackStyle.AreaStatic:
                    for (int j = 0; j < _possibleTargets.Count; j++)
                    {
                        _targets.Add(_possibleTargets[j]);
                    }
                    break;
            }

            return new AttackAction(user, _targets, _attack);
        }
        else
        {
            // Just try to move somewhere
            List<Tile> _t = user.FindSelectableTiles(TileState.Move, user.Familiar.Base.Movement);
            Tile check = _t[0];
            int iterations = 0;
            while (iterations < 50)
            {
                check = _t[Random.Range(0, _t.Count)];

                if (!(check.x == user.x && check.y == user.y))
                {
                    break;
                }

                iterations++;
            }
            return new AttackAction(user, check);
        }
        
    }

    List<Tile> MatchingTiles(Field field, PatternBase pattern1, PatternBase pattern2)
    {
        List<Tile> tiles = new List<Tile>();

        for (int i = 0; i < pattern1.Active.Length; i++)
        {
            if (pattern1.Active[i] && pattern2.Active[i])
            {
                tiles.Add(field.GetTile(i));
            }
        }
        return tiles;
    }

}

public struct AttackAction {
    public CombatUnit User;
    public List<CombatUnit> Targets;
    public Attack Attack;
    public Tile Location;

    // Attacking
    public AttackAction (CombatUnit user, List<CombatUnit> targets, Attack attack)
    {
        User = user;
        Targets = targets;
        this.Attack = attack;
        Location = null;
    }

    // Moving only
    public AttackAction(CombatUnit user, Tile tile)
    {
        User = user;
        Targets = null;
        Attack = null;
        Location = tile;
    }

}
