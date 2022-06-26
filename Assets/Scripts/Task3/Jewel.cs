using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

  public class Jewel {
        public int x;
        public int y;
        public int index;
        public Board.JewelKind kind;
        public bool IsJewel { get => this.kind >= Board.JewelKind.Red && this.kind < Board.JewelKind.Violet; }
        public bool IsEmpty { get => this.kind == Board.JewelKind.Empty; }
        
        public UnityEvent OnAfterReshuffle = new UnityEvent();
        public UnityEvent OnEmpty = new UnityEvent();
        public UnityEvent OnChangeType = new UnityEvent();
        public UnityEvent<Vector2Int> OnAnimateMove = new UnityEvent<Vector2Int>();
        public UnityEvent<Vector2Int> OnAnimateMoveAndBack = new UnityEvent<Vector2Int>();
        public UnityEvent OnAnimateFromEmpty = new UnityEvent();

        public UnityEvent<Vector2Int> OnAnimateForHintMove = new UnityEvent<Vector2Int>();
        public UnityEvent OnAnimateForHintShine = new UnityEvent();
        public UnityEvent OnStopAnimateHint = new UnityEvent();

        public Jewel(int index, int x, int y, Board.JewelKind kind) {
            this.index = index;
            this.x = x;
            this.y = y;
            this.kind = kind;
        }
        public void SetNewPos(int index, int x, int y) {
            if (this.index == index) return;
            this.index = index;
            this.x = x;
            this.y = y;
        }
        public void SetEmpty() {
            kind = Board.JewelKind.Empty;
            OnEmpty.Invoke();
        }
        public void ChangeKind(Board.JewelKind kind, bool invokeEvent = true) {
            this.kind = kind;
            if (invokeEvent) OnChangeType.Invoke();
        }
    }