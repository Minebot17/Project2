using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateInfo {

	public Vector2Int LocalPosition; // side: 0 - down, 1 - up, 2 - left, 3 - right; index: 0,1,2,3...
	public RoomInfo RoomFrom;
	public Vector2Int RoomTo;

	public GateInfo(Vector2Int localPosition, RoomInfo roomFrom) {
		LocalPosition = localPosition;
		RoomFrom = roomFrom;
	}

	// индекс ток от 1ого, а сторона ток 0, 1, 2 и т.д. до roomSize + 1
	public static Vector2Int VectorToLocalPosition(Vector2Int roomSize, Vector2Int vector) {
		int side = vector.x == 0 ? 2 : vector.x == roomSize.x + 1 ? 3 : vector.y == 0 ? 0 : 1;
		int index = side == 0 || side == 1 ? vector.x - 1 : vector.y - 1;
		return new Vector2Int(side, index);
	}

	public static Vector2Int RoomObjectToLocalPosition(Vector3 coords) {
		int side =
			coords.y == 0 ? 0 :
			(coords.y + 23) % 277 == 0 ? 1 :
			coords.x == 0 ? 2 : 3;
		int index = side == 0 || side == 1 ? ((int)coords.x - 214) / 495 : ((int)coords.y - 105) / 277;
		return new Vector2Int(side, index);
	}

	public static Vector2Int LocalPositionToVector(Vector2Int localPos) {
		Vector2Int sideVector = localPos.x == 0 ? Vector2Int.down : localPos.x == 1 ? Vector2Int.up : localPos.x == 2 ? Vector2Int.left : Vector2Int.right;
		return sideVector + new Vector2Int(sideVector.x == 0 ? localPos.y : 0, sideVector.y == 0 ? localPos.y : 0);
	}

	public Vector2Int GetScaledVector(Vector2Int roomSize) {
		switch (LocalPosition.x) {
			case 0:
				return new Vector2Int(LocalPosition.y, -1);
			case 1:
				return new Vector2Int(LocalPosition.y, roomSize.y);
			case 2:
				return new Vector2Int(-1, LocalPosition.y);
			case 3:
				return new Vector2Int(roomSize.x, LocalPosition.y);
		}

		return new Vector2Int();
		/*Vector2Int vector = LocalPositionToVector();
		bool upDown = LocalPosition.x == 0 || LocalPosition.x == 1;
		return new Vector2Int(upDown ? vector.x * (roomSize.x-1) : vector.x, !upDown ? vector.y * (roomSize.y-1) : vector.y);*/
	}

	public Vector2Int LocalPositionToVector() {
		Vector2Int sideVector = SideToVector();
		return sideVector + new Vector2Int(sideVector.x == 0 ? LocalPosition.y : 0, sideVector.y == 0 ? LocalPosition.y : 0);
	}
	
	private Vector2Int SideToVector() {
		return LocalPosition.x == 0 ? Vector2Int.down : LocalPosition.x == 1 ? Vector2Int.up : LocalPosition.x == 2 ? Vector2Int.left : Vector2Int.right;
	}
}
