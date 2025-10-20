using UnityEngine;

public class HandFollower : MonoBehaviour
{
	[SerializeField] private Transform target; // обычно камера игрока
	[SerializeField] private Vector3 positionOffset = Vector3.zero; // в локальных координатах target
	[SerializeField] private Vector3 eulerOffset = Vector3.zero; // дополнительный локальный поворот
	[SerializeField] private bool compensateParentScale = true; // инвертировать масштаб родителя

	void LateUpdate()
	{
		if (target == null) return;

		// Вычисляем желаемую мировую позицию/поворот
		Quaternion targetRot = target.rotation * Quaternion.Euler(eulerOffset);
		Vector3 targetPos = target.TransformPoint(positionOffset);

		transform.SetPositionAndRotation(targetPos, targetRot);

		if (compensateParentScale)
		{
			var parent = transform.parent;
			if (parent != null)
			{
				Vector3 pls = parent.lossyScale;
				transform.localScale = new Vector3(
					Mathf.Approximately(pls.x, 0f) ? 1f : 1f / pls.x,
					Mathf.Approximately(pls.y, 0f) ? 1f : 1f / pls.y,
					Mathf.Approximately(pls.z, 0f) ? 1f : 1f / pls.z
				);
			}
			else
			{
				transform.localScale = Vector3.one;
			}
		}
	}

	public void SetTarget(Transform t)
	{
		target = t;
	}
} 