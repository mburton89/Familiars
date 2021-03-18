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

    public Familiar Familiar { get; set; }

    private void Awake()
    {
        originalColor = image.color;
        originalPos = image.transform.localPosition;
    }

    public void Setup()
    {
        //Familiar = new Familiar(_base, level);

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
        if (isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));

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

    public void FindSelectableTiles(TileState s, int range)
    {
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();
        
        process.Enqueue(currentTile);
        currentTile.visited = true;
        // currentNode.parent = null;

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            selectableTiles.Add(t);
            t.selectable = true;

            t.SetState(s);

            if (t.distance < range)
            {
                //Tile[] _additions = new Node[4];
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
    }
}
