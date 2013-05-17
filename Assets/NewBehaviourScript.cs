using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour
{
    GestureManager manager;

    public GameObject penPrefab;
    private GameObject penInstance;
    public float captureInterval = .1f;
    private float _captureTick = 0f;
    private Vector3 _lastMousePosition;

    private string _all = "可识别的手势: ";
    private string _lastMatch = string.Empty;

    void Start()
    {
        manager = new GestureManager(50);

        AddGesture("圆圈", new int[] { 4, 5, 6, 7, 0, 1, 2, 3, });
        AddGesture("下(|)", new int[] { 6, });
        AddGesture("上(|)", new int[] { 2, });
        AddGesture("左(-)", new int[] { 4, });
        AddGesture("右(-)", new int[] { 0, });
        AddGesture("左右(--)", new int[] { 4, 0, });
        AddGesture("右左(--)", new int[] { 0, 4, });
        AddGesture("上下(||)", new int[] { 2, 6, });
        AddGesture("下上(||)", new int[] { 6, 2, });
        AddGesture("Z", new int[] { 0, 5, 0, });
        AddGesture("√", new int[] { 7, 1, });
        AddGesture("^", new int[] { 1, 7, });

        manager.Committed += manager_Committed;
    }

    void AddGesture(string content, int[] data)
    {
        if (!_all.Contains(content))
            _all += content + " ___ ";
        manager.AddGesture(content, data);
    }

    void manager_Committed(object sender, GestureCommitEvent e)
    {
        _lastMatch = "识别结果: " + (e.prefab != null ? e.prefab.text : "无法识别");
    }

    void NewPen()
    {
        if (penInstance != null)
            Object.Destroy(penInstance);

        penInstance = Object.Instantiate(penPrefab) as GameObject;
        penInstance.transform.parent = transform;
        penInstance.transform.localPosition = Vector3.zero;
        penInstance.SetActive(true);
    }

    void SetPenPosition()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            transform.position = hitInfo.point;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            manager.Begin(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            _captureTick = 0f;
            _lastMousePosition = Input.mousePosition;
            SetPenPosition();
            NewPen();
        }
        else if (Input.GetMouseButton(0))
        {
            if (_lastMousePosition != Input.mousePosition)
            {
                _captureTick += Time.deltaTime;

                if (_captureTick >= captureInterval)
                {
                    _captureTick = 0f;
                    manager.Update(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                    SetPenPosition();
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            manager.End(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0f, 0f, Screen.width, 50f), "鼠标左键在空白处划出手势,松开鼠标后进行识别");
        GUI.Label(new Rect(0f, 50f, Screen.width, 50f), _all);
        GUI.Label(new Rect(0f, 100f, Screen.width, 50f), _lastMatch);
    }
}
