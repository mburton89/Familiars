using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CombatUnit : MonoBehaviour
{
    [SerializeField] FamiliarBase _base;
    [SerializeField] int level;
    public bool isPlayerUnit;
    public int teamPosition;
    
    [SerializeField] Image image;

    Vector3 originalPos;
    Color originalColor;

    [SerializeField] public int x;
    [SerializeField] public int y;


    List<Tile> selectableTiles = new List<Tile>();
    Stack<Tile> path = new Stack<Tile>();

    Tile currentTile;

    public BattleHUD Hud { get; set; }

    public Familiar Familiar { get; set; }

    private void Awake()
    {
        originalColor = image.color;
        originalPos = image.transform.localPosition;
    }

    public void Setup()
    {
        image.sprite = Familiar.Base.FamiliarSprite;

        if (isPlayerUnit)
        {
            this.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            this.transform.localScale = new Vector3(-1, 1, 1);
        }

        image.color = originalColor;
        //PlayerEnterAnimation();
    }

    #region Animations
    public void Display(bool display)
    {
        image.enabled = display;
    }
    
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(100, originalPos.y);
        else
            image.transform.localPosition = new Vector3(-100, originalPos.y);

        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 30f, 0.25f));
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
    #endregion

    // Tile Stuffs
    public Tile GetCurrentTile(bool setCurrent)
    {
        currentTile.current = setCurrent;
        return currentTile;
    }

    public Tile GetCurrentTile()
    {
        return GetCurrentTile(true);
    }

    public void SetCurrentTile(Tile t)
    {
        if (currentTile != null) currentTile.familiarOccupant = null;
        currentTile = t;
        this.gameObject.transform.position = currentTile.gameObject.transform.position;
        currentTile.familiarOccupant = this;
        x = currentTile.x;
        y = currentTile.y;
    }

    public List<Tile> FindSelectableTiles(TileState s, int range)
    {
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();
        List<Tile> closed = new List<Tile>();

        process.Enqueue(currentTile);
        currentTile.visited = true;
        // currentNode.parent = null;

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            selectableTiles.Add(t);
            t.selectable = true;

            closed.Add(t);

            if (t.distance < range)
            {
                List<Tile> _additions = t.GetNeighbors();

                foreach (Tile _t in _additions)
                {
                    if (_t != null)
                    {
                        if (!(_t.visited || _t.familiarOccupant != null))
                        {
                            _t.parent = t;
                            _t.visited = true;
                            _t.distance = 1 + t.distance;
                            process.Enqueue(_t);
                        }
                    }
                }
            }
        }
        
        return closed;
    }
}
