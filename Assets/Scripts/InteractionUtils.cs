using UnityEngine;

public static class InteractionUtils
{
	// Возвращает безопасную дистанцию до объекта для проверки CanInteract
	public static float ComputeDistanceToInteractor(Collider collider, Transform interactor)
	{
		if (collider == null || interactor == null)
		{
			return float.PositiveInfinity;
		}

		// Поддерживаемые типы для ClosestPoint
		bool supportsClosestPoint = collider is BoxCollider || collider is SphereCollider || collider is CapsuleCollider ||
			(collider is MeshCollider mc && mc.convex);

		if (supportsClosestPoint)
		{
			Vector3 closest = collider.ClosestPoint(interactor.position);
			return Vector3.Distance(interactor.position, closest);
		}

		// Фолбэк: используем Bounds и Transform как приближение
		Bounds b = collider.bounds;
		// Кладем позицию в границы bounds, чтобы приблизить closest point
		Vector3 clamped = new Vector3(
			Mathf.Clamp(interactor.position.x, b.min.x, b.max.x),
			Mathf.Clamp(interactor.position.y, b.min.y, b.max.y),
			Mathf.Clamp(interactor.position.z, b.min.z, b.max.z)
		);
		float distBounds = Vector3.Distance(interactor.position, clamped);

		// Дополнительно учитываем pivot объекта
		float distCenter = Vector3.Distance(interactor.position, collider.transform.position);

		return Mathf.Min(distBounds, distCenter);
	}
} 