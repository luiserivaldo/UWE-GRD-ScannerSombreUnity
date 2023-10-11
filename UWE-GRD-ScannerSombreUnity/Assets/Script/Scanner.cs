using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Random = UnityEngine.Random;


[RequireComponent(typeof(LineRenderer))]
public class Scanner : MonoBehaviour
{
    private InputAction _fire;
    private InputAction _changeRadius;
    private List<Vector3> _positionsList = new();
    private List<VisualEffect> _vfxList = new();
    private VisualEffect _currentVFX;
    private Texture2D _texture;
    private Color[] _positions;
    private bool _createNewVFX;
    //private int _particleAmount;
    private LineRenderer _lineRenderer;

    private const string TEXTURE_NAME = "PositionTexture";
    private const string RESOLUTION_PARAMETER_NAME = "resolution";

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private VisualEffect _vfxPrefab;
    [SerializeField] private GameObject _vfxContainer;
    [SerializeField] private Transform _castPoint;
    [SerializeField] private float _radius = 10f;
    [SerializeField] private float _maxRadius = 10f;
    [SerializeField] private float _minRadius = 1f;
    [SerializeField] private int _pointsPerScan = 40;
    [SerializeField] private float _range = 10f;
    [SerializeField] private int resolution = 100;
    private void Start()
    {
        _fire = playerInput.actions["Fire"];
        _changeRadius = playerInput.actions["Scroll"];
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
        _createNewVFX = true;
        CreateNewVFX();
        ApplyPositions();
    }

 
    private void FixedUpdate()
    {
        Scan();
        ChangeRadius();
    }

    private void ChangeRadius()
    {
        if (_changeRadius.triggered)
        {
            _radius = Mathf.Clamp(_radius + _changeRadius.ReadValue<float>() * Time.deltaTime, _minRadius, _maxRadius);
        }
    }

    private void Scan()
    {
        if (_fire.IsPressed())
        {
            for(int i = 0; i < _pointsPerScan; i++)
            {
                Vector3 randomPoint = Random.insideUnitSphere * _radius;
                randomPoint += _castPoint.position;


                Vector3 direction = (randomPoint - transform.position).normalized;

                if(Physics.Raycast(transform.position,direction,out RaycastHit hit, _range, _layerMask))
                {
                    if(_positionsList.Count < resolution * resolution)
                    {
                        _positionsList.Add(hit.point);
                        _lineRenderer.enabled = true;
                        _lineRenderer.SetPositions(new[]
                        {
                            transform.position,
                            hit.point
                        });
                        

                    }
                    else
                    {
                        _createNewVFX = true;
                        CreateNewVFX();
                        break;
                    }
                }
            }
            ApplyPositions();
        }
        else
        {
            _lineRenderer.enabled=false;
        }
    }

    private void ApplyPositions()
    {
        Vector3[] pos = _positionsList.ToArray();
        Vector3 vfxPos = _currentVFX.transform.position;

        int loopLength = _texture.width * _texture.height;
        int posListLen = pos.Length;

        for (int i = 0; i < loopLength; i++)
        {
            Color data;

            if (i < posListLen - 1)
            {
                data = new Color(pos[i].x - vfxPos.x, pos[i].y - vfxPos.y, pos[i].z - vfxPos.z, 1);
            }
            else
            {
                data = new Color(0, 0, 0, 0);
            }
            _positions[i] = data;
        }
        _texture.SetPixels(_positions);
        _texture.Apply();
        _currentVFX.SetTexture(TEXTURE_NAME, _texture);
        _currentVFX.Reinit();
    }
    private void CreateNewVFX()
    {
        if (!_createNewVFX) return;
        _vfxList.Add(_currentVFX);
        _currentVFX = Instantiate(_vfxPrefab, transform.position, Quaternion.identity, _vfxContainer.transform);
        _currentVFX.SetUInt(RESOLUTION_PARAMETER_NAME, (uint)resolution);
        _texture = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false);
        _positions = new Color[resolution * resolution];
        _positionsList.Clear();
        _createNewVFX = false;
    }

}
