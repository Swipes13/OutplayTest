using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IBoardJewelView {
    public List<Sprite> JewelKindSprites { get; }
}

public class BoardView : MonoBehaviour, IBoardJewelView {
    public const float GemSize = 0.9f;

    private struct JewelHint {
        public List<Jewel> ShineJewels;
        public Jewel Jewel;
        public Jewel SwapJewel;
    }

    public GameObject JewelPrefab;
    public List<Sprite> JewelSprites;
    public List<Sprite> JewelKindSprites { get => JewelSprites; }

    private Board board;
    private List<JewelNode> JewelNodes = new List<JewelNode>();

    private int _waitForAnimations = 0;
    private int waitForAnimations {
        get => _waitForAnimations;
        set {
            var was = _waitForAnimations;
            _waitForAnimations = value;
            if (was == 0 && _waitForAnimations > 0) {
                SelectedJewelNode?.AnimateUnselect();
                SelectedJewelNode = null;
                board.BlockedByAnim = true;
            }
            if (was > 0 && _waitForAnimations == 0) {
                board.BlockedByAnim = false;
            }
        }
    }

    public void SetBoard(Board board) {
        JewelNodes.ForEach(j => DestroyImmediate(j.gameObject));
        JewelNodes.Clear();
        this.board = board;
        for (var i = 0; i < this.board.Jewels.Count; i++) {
            var jewel = this.board.Jewels[i];
            var pos = board.GetPos(i);
            CreateJewelNode(jewel);
        }
        board.OnStable.AddListener(ShowHint);
        board.OnHideHint.AddListener(HideHint);

        ShowHint();
    } 
    public GameObject CreateJewelNode(Jewel jewel) {
        var jNode = Instantiate(JewelPrefab, transform);
        var jewelNode = jNode.GetComponent<JewelNode>();
        jewelNode.SetJewel(jewel, this);
        jewelNode.OnJewelAnimStarted.AddListener(() => waitForAnimations++);
        jewelNode.OnJewelAnimFinished.AddListener(() => waitForAnimations--);
        JewelNodes.Add(jewelNode);
        return jNode;
    }

    public void Update() {
        if (Input.GetMouseButtonUp(0)) OnMouseUp();
    }
    JewelNode SelectedJewelNode;
    public void OnMouseUp() {
        if (!board.CanSwap) return;
        
        var camMousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var ray = new Ray2D(new Vector2(camMousePoint.x, camMousePoint.y), Vector2.zero);
        var hit = Physics2D.Raycast(ray.origin, ray.direction, 0f);
        
        if (hit.collider != null) {
            if (SelectedJewelNode == null) {
                SelectedJewelNode = hit.collider.gameObject.GetComponent<JewelNode>();
                SelectedJewelNode.AnimateSelect();
            } else {
                SelectedJewelNode.AnimateUnselect();
                var newJewelNode = hit.collider.gameObject.GetComponent<JewelNode>();
                if (SelectedJewelNode == newJewelNode) {
                    SelectedJewelNode = null;
                    return;
                }

                var actualNeis = board.IsInOneHLineNei(SelectedJewelNode.Jewel, newJewelNode.Jewel) || board.IsInOneVLineNei(SelectedJewelNode.Jewel, newJewelNode.Jewel);
                if (actualNeis) {
                    board.PlayerSwap(SelectedJewelNode.Jewel, newJewelNode.Jewel);
                    SelectedJewelNode = null;
                } else {
                    SelectedJewelNode = newJewelNode;
                    SelectedJewelNode.AnimateSelect();
                }
            }
        }
    }

    private bool hintShowed = false;
    private JewelHint jewelHint;
    private void PrepareJewelHint() {
        board.PossibleMoves.Sort((a, b) => b.GetPower() - a.GetPower());
        var first = board.PossibleMoves[0];
        var allBest = board.PossibleMoves.Where(pm => pm.GetPower() == first.GetPower()).ToList();
        var randomBest = Utils.RandomFromList(allBest);

        jewelHint.Jewel = board.GetJewel(randomBest.x, randomBest.y);
        jewelHint.SwapJewel = board.GetJewelNei(jewelHint.Jewel, randomBest.direction);
        jewelHint.ShineJewels = randomBest.matches
            .SelectMany(m => m.jewels)
            .Where(j => j != jewelHint.Jewel && j != jewelHint.SwapJewel)
            .ToList();
    }
    private void ShowHint() {
        if (hintShowed) return;
        if (board.PossibleMoves.Count == 0) return;

        PrepareJewelHint();
        jewelHint.Jewel.OnAnimateForHintMove.Invoke(new Vector2Int(jewelHint.SwapJewel.x, jewelHint.SwapJewel.y));
        jewelHint.SwapJewel.OnAnimateForHintMove.Invoke(new Vector2Int(jewelHint.Jewel.x, jewelHint.Jewel.y));
        jewelHint.ShineJewels.ForEach(j => j.OnAnimateForHintShine.Invoke());

        hintShowed = true;
    }
    private void HideHint() {
        if (!hintShowed) return;

        jewelHint.Jewel.OnStopAnimateHint.Invoke();
        jewelHint.SwapJewel.OnStopAnimateHint.Invoke();
        jewelHint.ShineJewels.ForEach(j => j.OnStopAnimateHint.Invoke());

        hintShowed = false;
    }
}