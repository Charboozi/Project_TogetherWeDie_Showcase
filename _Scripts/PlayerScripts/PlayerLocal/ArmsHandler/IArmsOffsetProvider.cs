using UnityEngine;

public interface IArmsOffsetProvider
{
    Vector3 GetOffset();
    Quaternion GetRotation(); // <-- add this
}