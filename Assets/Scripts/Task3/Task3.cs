using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoardProperties))]
public class Task3 : MonoBehaviour {
    public BoardView boardView;

    [InspectorButton("TestClick", 120)]
    public bool test;
    private void TestClick() => Test();

    [InspectorButton("TestReshuffle", 120)]
    public bool reshuffleBoard;
    private void TestReshuffle() {
        if (!Application.isPlaying) {
            Debug.Log("Only in Play mode!");
            return;
        }
        if (board == null) {
            Debug.Log("No Board! Can'r Reshuffle");
            return;
        }
        // There is test scenario 
        if (board.BlockedByAnim) return;
        board.OnHideHint.Invoke();
        board.PossibleMoves.Clear();
        board.state = Board.State.CheckReshuffle;
    }

    Board board;

    private void Start() => Test();
    private void Test() {
        if (!Application.isPlaying) {
            Debug.Log("Only in Play mode!");
            return;
        }
        if (board != null && board.BlockedByAnim) return;
        CreateBoard();
        if (boardView != null) {
            boardView.SetBoard(board);
        }
        // This is not good approach) but for test is ok
        Camera.main.transform.position = new Vector3((board.Width - 1) * BoardView.GemSize / 2, (board.Height - 1) * BoardView.GemSize / 2, -10);
        Camera.main.orthographicSize = Mathf.Max(6f, board.Width, board.Height) / 2;
    }

    private void CreateBoard() {
        var props = GetComponent<BoardProperties>();
        board = new Board(width: props.Width, height: props.Height, genCount: props.JewelsGenCount);
    }

}