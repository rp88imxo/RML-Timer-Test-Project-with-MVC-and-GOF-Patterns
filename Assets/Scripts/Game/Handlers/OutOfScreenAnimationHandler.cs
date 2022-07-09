using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OutOfScreenAnimationHandler : MonoBehaviour
{
    public enum AnimationSide
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [SerializeField]
    private AnimationSide _animationSide;

    [SerializeField]
    private float _speed = 5f;

    private float _interpolator;
    private Vector2 _localPosition;
    private Vector2 _globalPosition;
    private Vector2 _outCanvasPosition;
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Coroutine _animationCoroutine;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _globalPosition = GetGlobalPosition(_rectTransform);
        _localPosition = _rectTransform.localPosition;
        _outCanvasPosition = _localPosition + GetDifferenceToOutside();
    }

    public void StartAnimate(float speedMultiplier)
    {
        _animationCoroutine = StartCoroutine(AnimationCoroutine(speedMultiplier));
    }

    public void StopAnimate()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _rectTransform.localPosition = _localPosition;
        }
    }

    IEnumerator AnimationCoroutine(float speedMultiplier)
    {
        _interpolator = 0f;

        while (_interpolator < 1f)
        {
            _rectTransform.localPosition =
                Vector2.Lerp(_outCanvasPosition,
                    _localPosition,
                    _interpolator);
            _interpolator += Time.deltaTime * (_speed * speedMultiplier);
            yield return null;
        }

        _rectTransform.localPosition = _localPosition;
    }

    private Vector2 GetGlobalPosition(RectTransform trans)
    {
        var pos = Vector3.zero;
        foreach (var parent in
            trans.GetComponentsInParent<RectTransform>())
        {
            if (parent.GetComponent<Canvas>() == null)
            {
                pos += parent.localPosition;
            }
            else
            {
                return pos;
            }
        }

        return pos;
    }

    private Vector2 GetDifferenceToOutside()
    {
        var size = _rectTransform.rect.size;
        var pivot = _rectTransform.pivot;
        var canvasSize = _canvas.GetComponent<RectTransform>().rect.size;
        var pos = _globalPosition + (canvasSize / 2.0f);
        var offset = 200f;

        switch (_animationSide)
        {
        case AnimationSide.Top:
            var distanceToTop = canvasSize.y - pos.y;
            return new Vector2(0f, distanceToTop + size.y * (pivot.y));
        case AnimationSide.Bottom:
            var distanceToBottom = pos.y;
            return new Vector2(0f,
                -distanceToBottom - size.y * (1 - pivot.y));
        case AnimationSide.Left:
            var distanceToLeft = pos.x;
            return new Vector2(-distanceToLeft
                - size.x * (1 - pivot.x)
                - offset,
                0f);
        case AnimationSide.Right:
            var distanceToRight = canvasSize.x - pos.x;
            return new Vector2(distanceToRight + size.x * (pivot.x), 0f);
        default: return Vector2.zero;
        }
    }
}