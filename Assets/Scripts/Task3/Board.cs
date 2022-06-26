using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Board {
    public struct Move {
        public int x;
        public int y;
        public MoveDirection direction;

        // This is for saved calculations
        public List<Match> matches;
        public int GetPower() => matches.Sum(m => m.power);
    };

    public class Match {
        public bool wasMatch = false;
        public HashSet<Jewel> jewels = new HashSet<Jewel>();
        public int power { get => wasMatch ? jewels.Count : 0; private set {} }
    }

    const int MaxRegenCount = 20;

    public enum State { None, WaitCreate, Stable, Broken, CheckMatch, CheckPossibleMoves, CheckReshuffle, CheckMove }
    public enum JewelKind { NeedToGen = -1, Empty = 0, Red, Orange, Yellow, Green, Blue, Indigo, Violet };
    public enum MoveDirection { Up, Down, Left, Right };
    
    public List<Jewel> Jewels;
    public List<Move> PossibleMoves = new List<Move>();
    public List<Match> PossibleMatches = new List<Match>();

    public UnityEvent OnStable = new UnityEvent();
    public UnityEvent OnHideHint = new UnityEvent();
    
    public bool IsStable { get => this.state == State.Stable; private set {} }
    public bool CanSwap { get => this.IsStable && BlockedByAnim == false; private set {} }
    public bool HaveNoMoves { get => this.PossibleMoves.Count == 0; private set {} }
    public bool HaveNoMatches { get => this.PossibleMatches.Count == 0; private set {} }

    private void CalculatePossibleMatches() {
        // TODO: Recalc only when Board need it! (some lil improvement)
        PossibleMatches = Jewels.Where(j => j.IsJewel).Select(j => CalcMatch(j)).Where(m => m.wasMatch).ToList(); 
    }

    private State _nextState = State.None;
    private bool _blockedByAnim = false;
    public bool BlockedByAnim {
        get => _blockedByAnim;
        set {
            this._blockedByAnim = value;
            if (_blockedByAnim == false && this._nextState != State.None) {
                this.state = this._nextState;
            }
        }
    }

    private State _state = State.WaitCreate;
    public State state { 
        get => _state; 
        set {
            if (BlockedByAnim) {
                Debug.Log($"[Board] new state delayed {this._state} => {value}");
                this._nextState = value;
                return;    
            }
            this._state = value;
            this._nextState = State.None;

            Debug.Log($"[Board] new state {this._state}");
            switch (this._state) {
                case State.CheckMove: CheckMove(); break;
                case State.CheckMatch: CheckMatch(); break;
                case State.CheckPossibleMoves: CheckPossibleMoves(); break;
                case State.CheckReshuffle: CheckReshuffle(); break;
                default: break;
            };

            if (this._state == State.Stable) OnStable.Invoke();
        } 
    }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int JewelGenCount { get; private set; } 

    public Vector2Int GetPos(int index) => new Vector2Int(index % Width, Mathf.FloorToInt(index / Width));
    public int GetIndex(int x, int y) => x + y * Width;
    public Jewel GetJewel(int x, int y) => GetJewel(GetIndex(x, y));
    public Jewel GetJewel(int index) => this.Jewels[index];
    public void SetJewel(int x, int y, JewelKind kind) => SetJewel(GetIndex(x, y), kind);
    public void SetJewel(int index, JewelKind kind) => this.Jewels[index].kind = kind;
    public bool IsInOneVLineNei(Jewel j1, Jewel j2) => Mathf.Abs(j1.y - j2.y) == 1 && j1.x == j2.x;
    public bool IsInOneHLineNei(Jewel j1, Jewel j2) => Mathf.Abs(j1.x - j2.x) == 1 && j1.y == j2.y;
    
    public Board(int width, int height, int genCount) {
        Width = width;
        Height = height;
        JewelGenCount = genCount;
        CreateWorkingBoard();
    }
    private Move? CreateBoardWithMove() {
        CreateEmptyBoard(JewelKind.NeedToGen);
        RegenJewels();
        return CalculateBestMoveForBoard();
    }
    private void CreateWorkingBoard() {
        var countRegen = 0;
        var move = CreateBoardWithMove();
        while (move == null) {
            move = CreateBoardWithMove();
            if (++countRegen > MaxRegenCount) break;
        }
        if (move == null) {
            state = State.Broken;
            Debug.LogWarning("[Can't create board with possible move] check conditions");
        } else {
            state = State.Stable;
        }
    }
    private void CreateEmptyBoard(JewelKind basicKind) {
        Jewels = new List<Jewel>(Width * Height); 
        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                Jewels.Add(new Jewel(GetIndex(x, y), x, y, basicKind));
            }
        }
    }

    // If there is start board (for example from json) -> possible return false and regen again
    private bool RegenJewels() {
        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                var jewel = GetJewel(x, y);
                if (jewel.kind == JewelKind.NeedToGen) {
                    var count = JewelGenCount;
                    var allCount = 1.To(JewelGenCount).Select(i => (JewelKind)i).ToHashSet();
                    
                    var firstNeis = new List<JewelKind>();
                    if (x > 1) { // Sure that no m3 horiz
                        var left = GetJewel(x - 1, y);
                        var leftLeft = GetJewel(x - 2, y);
                        if (left.kind == leftLeft.kind) allCount.Remove(left.kind);
                    }
                    if (y > 1) { // Sure that no m3 vertical
                        var down = GetJewel(x, y - 1);
                        var downDown = GetJewel(x, y - 2);
                        if (down.kind == downDown.kind) allCount.Remove(down.kind);
                    } 
                    // If there are some predefined JewelKinds (like from json) -> need to sure about other neigs!

                    var newJewelKind = Utils.RandomFromList(allCount.ToList());
                    SetJewel(x, y, newJewelKind);
                } 
            }
        }
        return true;
    }
    private void Reshuffle() {
        Jewels = Utils.ShuffleList(Jewels);
        Jewels.Select((j, i) => (j, i)).ToList().ForEach(t => {
            var pos = GetPos(t.i);
            t.j.SetNewPos(t.i, pos.x, pos.y);
        });
    }
    private void AfterSuccessReshuffle() {
        Jewels.ForEach(j => j.OnAfterReshuffle.Invoke());
    }

    public Move? CalculateBestMoveForBoard() {
        Move? bestMove = null;
        var bestPower = 0;
        PossibleMoves.Clear();
        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                var jewel = GetJewel(x, y);
                if (!jewel.IsJewel) continue;

                var neisToTest = new List<MoveDirection> { 
                    MoveDirection.Left, 
                    MoveDirection.Down 
                }.Select(dir => (jewel: GetJewelNei(jewel, dir), dir: dir)).Where(t => t.jewel != null).ToList();

                neisToTest.ForEach(swapTest => {
                    var matches = CalcMatchOnSwap(jewel, swapTest.jewel);
                    if (matches.Count > 0) {
                        var currentPower = matches.Sum(m => m.power);
                        var currentMove = new Move { x = x, y = y, direction = swapTest.dir, matches = matches }; 
                        PossibleMoves.Add(currentMove);
                        if (bestPower < currentPower) bestMove = currentMove;
                    }
                });
            }
        }
        // The best way to store all possible moves and show Best by Sort and then select random of best power.
        return bestMove;
    }
    public List<Match> CalcMatchOnSwap(Jewel jewel, Jewel swap) {
        this.SwapTypes(jewel, swap);
        var matches = new List<Match> {
            this.CalcMatch(jewel), 
            this.CalcMatch(swap)
        }.Where(m => m.wasMatch).ToList();
        this.SwapTypes(jewel, swap);
        return matches;
    }
    public void PlayerSwap(Jewel jewel, Jewel swap) {
        var hasMatch = CalcMatchOnSwap(jewel, swap).Count > 0;        
        if (hasMatch) {
            OnHideHint.Invoke();
            this.SwapKinds(jewel, swap);
            jewel.OnAnimateMove.Invoke(new Vector2Int(swap.x, swap.y));
            swap.OnAnimateMove.Invoke(new Vector2Int(jewel.x, jewel.y));
            this.state = State.CheckMatch;
        } else {
            jewel.OnAnimateMoveAndBack.Invoke(new Vector2Int(swap.x, swap.y));
            swap.OnAnimateMoveAndBack.Invoke(new Vector2Int(jewel.x, jewel.y));
        }
    }
    // returns the match line power
    private Match CalcMatch(Jewel jewel) {
        var resultMatch = new Match();
        if (!jewel.IsJewel) return resultMatch;

        var allDirs = new List<MoveDirection> { MoveDirection.Left, MoveDirection.Right, MoveDirection.Up, MoveDirection.Down };
        var halfLines = allDirs.Select(dir => {
            var next = GetJewelNei(jewel, dir);
            var same = new List<Jewel>();
            while (next != null && next.kind == jewel.kind) {
                same.Add(next);
                next = GetJewelNei(next, dir);
            }
            return same;
        }).ToList();
        if (halfLines[0].Count + halfLines[1].Count >= 2) { // Horizontal M3
            resultMatch.wasMatch = true;
            resultMatch.jewels.Add(jewel);
            halfLines[0].ForEach(j => resultMatch.jewels.Add(j));
            halfLines[1].ForEach(j => resultMatch.jewels.Add(j));
        }
        if (halfLines[2].Count + halfLines[3].Count >= 2) { // Vertical M3
            resultMatch.wasMatch = true;
            resultMatch.jewels.Add(jewel);
            halfLines[2].ForEach(j => resultMatch.jewels.Add(j));
            halfLines[3].ForEach(j => resultMatch.jewels.Add(j));
        }
        return resultMatch;
    }

    public Jewel GetJewelNei(Jewel jewel, MoveDirection dir) => dir switch {
        MoveDirection.Up => jewel.y < Height - 1 ? GetJewel(jewel.x, jewel.y + 1) : null,
        MoveDirection.Down => jewel.y > 0 ? GetJewel(jewel.x, jewel.y - 1) : null,
        MoveDirection.Left => jewel.x > 0 ? GetJewel(jewel.x - 1, jewel.y) : null,
        MoveDirection.Right => jewel.x < Width - 1 ? GetJewel(jewel.x + 1, jewel.y) : null,

        _ => null
    };

    // @mark deprecated => SwapKinds instead with invokeEvent = false
    private void SwapTypes(Jewel jewel1, Jewel jewel2) {
        var temp = jewel1.kind; 
        SetJewel(jewel1.index, jewel2.kind);
        SetJewel(jewel2.index, temp);
    }
    private void SwapKinds(Jewel jewel1, Jewel jewel2, bool invokeEvent = true) {
        var tempKind = jewel1.kind; 
        jewel1.ChangeKind(jewel2.kind, invokeEvent);
        jewel2.ChangeKind(tempKind, invokeEvent);
    }
    private bool CheckMatch() {
        CalculatePossibleMatches();

        if (PossibleMatches.Count > 0) {
            // TODO: Give points for everyJewel
            var uniqJewels = PossibleMatches.SelectMany(m => m.jewels).ToList();
            
            uniqJewels.ForEach(jewel => {
                if (jewel.IsJewel) {
                    jewel.SetEmpty();
                }
            });
            
            this.state = State.CheckMove;
            return true;
        } else {
            state = State.CheckPossibleMoves;
            return false;
        }
    }
    private void CheckPossibleMoves() {
        CalculateBestMoveForBoard();
        if (HaveNoMoves) this.state = State.CheckReshuffle;
        else this.state = State.Stable; 
    }
    private void CheckReshuffle() {
        var countReshuffle = 0;
        while (HaveNoMatches && HaveNoMoves) {
            Reshuffle();
            CalculatePossibleMatches();
            CalculateBestMoveForBoard();
            if (++countReshuffle > MaxRegenCount) break;
        }
        if (HaveNoMatches && HaveNoMoves) {
            Debug.LogWarning("Now board is broken!");
            this.state = State.Broken;
        } else {
            AfterSuccessReshuffle();
            if (HaveNoMatches) this.state = State.Stable;
            else this.state = State.CheckMatch;
        }
    }
    private void CheckMove() {
        var wasMove = MoveNonStable();
        this.state = State.CheckMatch;
    }
    private bool MoveNonStable() {
        var wasMove = false;
        var forEmptyCreateOffset = Enumerable.Repeat(0, Width).ToList();
        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                var j = GetJewel(x, y);
                if (j.IsEmpty) {
                    var up = j;
                    do { up = GetJewelNei(up, MoveDirection.Up); }
                    while (up != null && up.IsEmpty);

                    if (up != null) {
                        SwapKinds(up, j);
                        j.OnAnimateMove.Invoke(new Vector2Int(up.x, up.y));
                        wasMove = true;
                    } else { // CreateEmpty from Top
                        var offset = forEmptyCreateOffset[x]++;
                        var rKind = (JewelKind)(Random.Range(0, JewelGenCount) + 1);
                        j.ChangeKind(rKind, false);
                        j.OnAnimateFromEmpty.Invoke();
                        j.OnAnimateMove.Invoke(new Vector2Int(j.x, Height + offset));
                    }
                }
            }
        }
        return wasMove;
    }
};