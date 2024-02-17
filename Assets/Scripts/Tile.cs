using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 startDragPosition;
    private Vector2 endDragPosition;
    private int moveAngle;
    private GameView gameView;

    private void Start()
    {
        gameView = GameView.Instance;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startDragPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        endDragPosition = eventData.position;
        //dragging
        if((endDragPosition-startDragPosition).magnitude > 0.1f)
        {
            MoveCalculator();
            gameView.MoveTheCell(transform, moveAngle);
        }
        //clicking
        else
        {
            gameView.Blast(transform);
        }
    }

    // Examine the move angle to see which way it's shifted
    private void MoveCalculator()
    {
        moveAngle = (int)(Mathf.Atan2(endDragPosition.y - startDragPosition.y, endDragPosition.x - startDragPosition.x) * 180 / Mathf.PI);
        Debug.Log("Move Angle: " + moveAngle);
    }
}
