using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 startDragPosition;
    private Vector3 endDragPosition;
    private int moveAngle;

    private void OnMouseDown()
    {
        Debug.Log("Tıkladı");
        startDragPosition = transform.position;
    }

    private void OnMouseUp()
    {
        Debug.Log("Tıklama kesildi");
        endDragPosition = transform.position;
        MoveCalculator();
    }

    private void MoveCalculator()
    {
        moveAngle = (int)(Mathf.Atan2(endDragPosition.y - startDragPosition.y, endDragPosition.x - startDragPosition.x) * 180 / Mathf.PI);
        Debug.Log("Move Angle: " + moveAngle);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("tik");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("tiiik");
    }
}
