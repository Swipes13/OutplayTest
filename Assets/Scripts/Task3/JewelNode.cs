using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public struct KillingTween {
    private Tween _tween;
    public bool CompleteWhenKill;
    public Tween Tween { 
        get => _tween; 
        set {
            if (_tween != null) _tween.Kill(CompleteWhenKill);
            _tween = value;
        }
    }
}

public class JewelNode : MonoBehaviour {
    public Jewel Jewel;
    public UnityEvent OnJewelAnimStarted;
    public UnityEvent OnJewelAnimFinished;

    private IBoardJewelView boardJewelView;
    private SpriteRenderer spriteRenderer;

    private KillingTween currentTween;
    private KillingTween addTween;
    private KillingTween hintTween;

    private void Start() {
        currentTween.CompleteWhenKill = false;
        addTween.CompleteWhenKill = true;
        hintTween.CompleteWhenKill = false;
    }
    public void SetJewel(Jewel jewel, IBoardJewelView boardJewelView) {
        this.boardJewelView = boardJewelView;
        this.Jewel = jewel;
        spriteRenderer = GetComponent<SpriteRenderer>();
        ChangeSprite();
        transform.position = CurrentPos;

        Jewel.OnAfterReshuffle.AddListener(AnimateAfterReshuffle);
        Jewel.OnEmpty.AddListener(AnimateEmpty);
        Jewel.OnChangeType.AddListener(ChangeSprite);
        Jewel.OnAnimateMove.AddListener(AnimateMove);
        Jewel.OnAnimateMoveAndBack.AddListener(AnimateMoveAndBack);
        Jewel.OnAnimateFromEmpty.AddListener(AnimateFromEmpty);
        
        Jewel.OnAnimateForHintMove.AddListener(AnimateForHintMove);
        Jewel.OnAnimateForHintShine.AddListener(AnimateForHintShine);
        Jewel.OnStopAnimateHint.AddListener(StopAnimateHint);
        
        OnJewelAnimStarted?.RemoveAllListeners();
        OnJewelAnimFinished?.RemoveAllListeners();
        OnJewelAnimStarted = new UnityEvent();
        OnJewelAnimFinished = new UnityEvent();
    }

    public Vector3 GetPosForBoardPos(int x, int y) => new Vector3(BoardView.GemSize * x, BoardView.GemSize * y, 0f);
    public Vector3 CurrentPos { get  => GetPosForBoardPos(Jewel.x, Jewel.y); }
    private float CalcTimeForMove(Vector3 newPos, float speed) {
        var dist = (newPos - transform.position).magnitude;
        return dist / speed;
    }
    public void AnimateSelect() {
        addTween.Tween = transform.DOScale(new Vector2(1.1f, 1.1f), 0.55f).SetLoops(-1, LoopType.Yoyo);
    }
    public void AnimateUnselect() {
        addTween.Tween = transform.DOScale(new Vector2(1f, 1f), 0.1f);
    }
    public void AnimateAfterReshuffle() {
        var speed = 3f;
        var newPos = CurrentPos;
        var time = CalcTimeForMove(newPos, speed);
        
        UseBlockingAnim(transform.DOMove(newPos, time).SetEase(Ease.OutElastic, 0.01f, -1));
    }
    public void AnimateEmpty() {
        UseBlockingAnim(transform.DOScale(new Vector2(0f, 0f), 0.25f).SetEase(Ease.OutQuad));
    }
    private void UseBlockingAnim(params Tween[] tweens) {
        addTween.Tween = null;
        OnJewelAnimStarted.Invoke();
        var seq = DOTween.Sequence();
        foreach (var t in tweens) seq.Append(t);
        seq.AppendCallback(() => OnJewelAnimFinished.Invoke());
        currentTween.Tween = seq;
    }
    private void OnDestroy() {
        OnJewelAnimFinished.RemoveAllListeners();
        OnJewelAnimStarted.RemoveAllListeners();
    }
    public void ChangeSprite() {
        transform.localScale = new Vector2(1f, 1f);
        UpdateSprite();
    }
    private void UpdateSprite() => spriteRenderer.sprite = boardJewelView.JewelKindSprites[(int)Jewel.kind];
    private void AnimateMove(Vector2Int from) {
        transform.position = GetPosForBoardPos(from.x, from.y);
        
        var speed = 5f;
        var newPos = CurrentPos;
        var time = CalcTimeForMove(newPos, speed);
        UseBlockingAnim(transform.DOMove(newPos, time).SetEase(Ease.OutBounce, 1f));
    }
    private void AnimateFromEmpty() {
        UpdateSprite();
        addTween.Tween = transform.DOScale(new Vector2(1f, 1f), 1.55f).SetEase(Ease.OutElastic, 0.01f, -1);
    }
    public void AnimateMoveAndBack(Vector2Int to) {
        var speed = 5f;
        var toGo = GetPosForBoardPos(to.x, to.y);
        var time = CalcTimeForMove(toGo, speed);

        var myPos = transform.position;
        UseBlockingAnim(
            transform.DOMove(toGo, time).SetEase(Ease.OutQuad),
            transform.DOMove(myPos, time).SetEase(Ease.OutQuad)
        );
    }

    private void AnimateForHintMove(Vector2Int to) {
        var posFrom = new Vector3(transform.position.x, transform.position.y);
        var posTo = GetPosForBoardPos(to.x, to.y);
        var part = transform.position + (posTo - transform.position) / 6.25f;
        const float time = 1.25f;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(part, time));
        seq.Append(transform.DOMove(posFrom, time));
        seq.SetLoops(-1, LoopType.Yoyo);
        hintTween.Tween = seq;
    }
    private void AnimateForHintShine() {
        const float time = 1.25f;
        hintTween.Tween = transform.DOScale(new Vector2(1.1f, 1.1f), time).SetLoops(-1, LoopType.Yoyo);
    }
    private void StopAnimateHint() {
        transform.position = CurrentPos;
        hintTween.Tween = null;
        transform.localScale = new Vector3(1f, 1f);
        transform.localRotation = Quaternion.identity; 
    }
}