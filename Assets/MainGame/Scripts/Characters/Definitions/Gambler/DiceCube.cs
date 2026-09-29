using UnityEngine;

namespace GARA.Characters.Gambler
{
    // A six-sided 3D die that tumbles and bounces in, landing with the rolled
    // face toward the camera. Mesh and pip texture are built in code.
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class DiceCube : MonoBehaviour
    {
        private const int TileSize = 64;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        // Face value - 1 -> outward normal; opposite faces sum to 7.
        private static readonly Vector3[] FaceNormals =
        {
            Vector3.back, Vector3.up, Vector3.right, Vector3.left, Vector3.down, Vector3.forward
        };

        // Per face: the right and up axes seen from outside (right x up = -normal).
        private static readonly Vector3[] FaceRights =
        {
            Vector3.right, Vector3.right, Vector3.forward, Vector3.back, Vector3.right, Vector3.left
        };

        private static readonly Vector3[] FaceUps =
        {
            Vector3.up, Vector3.forward, Vector3.up, Vector3.up, Vector3.back, Vector3.up
        };

        // Rest tilt, so two side faces show and it reads as a cube.
        private static readonly Quaternion RestTilt = Quaternion.Euler(-18f, 24f, 0f);

        private static Mesh _mesh;
        private static Texture2D _faces;

        private MeshRenderer _renderer;
        private MaterialPropertyBlock _block;
        private Quaternion _restRotation;
        private Vector3 _restPosition;
        private Vector3 _spinAxis;
        private float _spinDegrees;
        private float _bounceHeight;
        private float _duration;
        private float _elapsed;
        private float _delay;

        public bool IsRolling { get; private set; }

        public static DiceCube Create(Transform parent, Material material, float size)
        {
            var go = new GameObject("Die", typeof(MeshFilter), typeof(MeshRenderer), typeof(DiceCube));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * size;
            go.GetComponent<MeshFilter>().sharedMesh = SharedMesh();
            var die = go.GetComponent<DiceCube>();
            die._renderer = go.GetComponent<MeshRenderer>();
            die._renderer.sharedMaterial = material;
            die._renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            die._renderer.receiveShadows = false;
            die._block = new MaterialPropertyBlock();
            die.SetTint(Color.white);
            return die;
        }

        public void SetSorting(int sortingLayerId, int sortingOrder)
        {
            _renderer.sortingLayerID = sortingLayerId;
            _renderer.sortingOrder = sortingOrder;
        }

        public void SetTint(Color tint)
        {
            _block.SetTexture(MainTexId, SharedFaces());
            _block.SetColor(ColorId, tint);
            _renderer.SetPropertyBlock(_block);
        }

        // Tumbles for duration after delay, landing on face (1-6) at
        // restPosition (local).
        public void Roll(int face, Vector3 restPosition, float duration, float delay, float bounceHeight)
        {
            var index = Mathf.Clamp(face, 1, 6) - 1;
            var twist = Quaternion.AngleAxis(90f * Random.Range(0, 4), Vector3.forward);
            _restRotation = RestTilt * twist * Quaternion.FromToRotation(FaceNormals[index], Vector3.back);
            _restPosition = restPosition;
            _spinAxis = Random.onUnitSphere;
            _spinDegrees = Random.Range(540f, 900f);
            _bounceHeight = bounceHeight;
            _duration = Mathf.Max(0.01f, duration);
            _delay = delay;
            _elapsed = 0f;
            IsRolling = true;
            Apply(0f);
        }

        private void Update()
        {
            if (!IsRolling)
            {
                return;
            }

            if (_delay > 0f)
            {
                _delay -= Time.deltaTime;
                return;
            }

            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / _duration);
            Apply(t);
            if (t >= 1f)
            {
                IsRolling = false;
            }
        }

        private void Apply(float t)
        {
            var remaining = 1f - t;
            var spinLeft = remaining * remaining * remaining;
            transform.localRotation = Quaternion.AngleAxis(_spinDegrees * spinLeft, _spinAxis) * _restRotation;

            // Decaying bounces, touching down at t = 1.
            var hop = remaining * remaining * Mathf.Abs(Mathf.Cos(t * Mathf.PI * 2.5f));
            transform.localPosition = _restPosition + Vector3.up * (_bounceHeight * hop);
        }

        private static Mesh SharedMesh()
        {
            if (_mesh != null)
            {
                return _mesh;
            }

            var vertices = new Vector3[24];
            var normals = new Vector3[24];
            var uvs = new Vector2[24];
            var triangles = new int[36];
            for (var face = 0; face < 6; face++)
            {
                var n = FaceNormals[face];
                var r = FaceRights[face];
                var u = FaceUps[face];
                var v = face * 4;
                vertices[v] = (n - r - u) * 0.5f;
                vertices[v + 1] = (n - r + u) * 0.5f;
                vertices[v + 2] = (n + r + u) * 0.5f;
                vertices[v + 3] = (n + r - u) * 0.5f;
                uvs[v] = new Vector2(face / 6f, 0f);
                uvs[v + 1] = new Vector2(face / 6f, 1f);
                uvs[v + 2] = new Vector2((face + 1) / 6f, 1f);
                uvs[v + 3] = new Vector2((face + 1) / 6f, 0f);
                for (var i = 0; i < 4; i++)
                {
                    normals[v + i] = n;
                }

                var tri = face * 6;
                triangles[tri] = v;
                triangles[tri + 1] = v + 1;
                triangles[tri + 2] = v + 2;
                triangles[tri + 3] = v;
                triangles[tri + 4] = v + 2;
                triangles[tri + 5] = v + 3;
            }

            _mesh = new Mesh { name = "Die" };
            _mesh.vertices = vertices;
            _mesh.normals = normals;
            _mesh.uv = uvs;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
            return _mesh;
        }

        // Six tiles in a row, one per face value.
        private static Texture2D SharedFaces()
        {
            if (_faces != null)
            {
                return _faces;
            }

            var body = new Color32(0xF4, 0xEC, 0xD8, 0xFF);
            var edge = new Color32(0xC9, 0xBE, 0xA4, 0xFF);
            var ink = new Color32(0x1B, 0x17, 0x10, 0xFF);
            var red = new Color32(0xC8, 0x41, 0x3B, 0xFF);
            var pixels = new Color32[TileSize * 6 * TileSize];
            for (var face = 0; face < 6; face++)
            {
                var pips = PipsFor(face + 1);
                var pipColor = face == 0 ? red : ink;
                var pipRadius = face == 0 ? 0.16f : 0.1f;
                for (var y = 0; y < TileSize; y++)
                {
                    for (var x = 0; x < TileSize; x++)
                    {
                        var p = new Vector2((x + 0.5f) / TileSize, (y + 0.5f) / TileSize);
                        var border = Mathf.Min(Mathf.Min(p.x, 1f - p.x), Mathf.Min(p.y, 1f - p.y)) < 0.05f;
                        var color = border ? edge : body;
                        foreach (var pip in pips)
                        {
                            if ((p - pip).sqrMagnitude <= pipRadius * pipRadius)
                            {
                                color = pipColor;
                                break;
                            }
                        }

                        pixels[y * TileSize * 6 + face * TileSize + x] = color;
                    }
                }
            }

            _faces = new Texture2D(TileSize * 6, TileSize, TextureFormat.RGBA32, false)
            {
                name = "DieFaces",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _faces.SetPixels32(pixels);
            _faces.Apply(false, true);
            return _faces;
        }

        private static Vector2[] PipsFor(int value)
        {
            const float Low = 0.27f;
            const float High = 0.73f;
            var center = new Vector2(0.5f, 0.5f);
            var topLeft = new Vector2(Low, High);
            var topRight = new Vector2(High, High);
            var bottomLeft = new Vector2(Low, Low);
            var bottomRight = new Vector2(High, Low);
            switch (value)
            {
                case 1: return new[] { center };
                case 2: return new[] { topLeft, bottomRight };
                case 3: return new[] { topLeft, center, bottomRight };
                case 4: return new[] { topLeft, topRight, bottomLeft, bottomRight };
                case 5: return new[] { topLeft, topRight, center, bottomLeft, bottomRight };
                default: return new[] { topLeft, topRight, bottomLeft, bottomRight, new Vector2(Low, 0.5f), new Vector2(High, 0.5f) };
            }
        }
    }
}
