using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfo {

	public readonly Vector2Int Position;
	public Vector2Int Size;
	public List<GateInfo> Gates = new List<GateInfo>();
	public bool IsReserved;
	public string ReservedRoomName;
	
	public RoomInfo(Vector2Int position) {
		Position = position;
		Size = new Vector2Int(1, 1);
	}

	public void Reserve(string roomName) {
		IsReserved = true;
		ReservedRoomName = roomName;
	}

	public void GenerateGates(RoomInfo[,] rooms) {
		Gates = new List<GateInfo>();
		for (int x = 0; x < Size.x; x++) {
			AddGate(new Vector2Int(0, x), rooms);
			AddGate(new Vector2Int(1, x), rooms);
		}

		for (int y = 0; y < Size.y; y++) {
			AddGate(new Vector2Int(2, y), rooms);
			AddGate(new Vector2Int(3, y), rooms);
		}
	}

	public void ReconectAllGatesRemoveEmpty(GenerationInfo generation) {
		Gates.ForEach(gate => {
			Vector2Int vec = gate.GetScaledVector(Size) + Position;
			if (vec.x < generation.Rooms.GetLength(0) && vec.y < generation.Rooms.GetLength(1) && vec.x >= 0 && vec.y >= 0)
				gate.RoomTo = vec;
		});
		Gates.RemoveAll(gate => generation.Rooms[gate.RoomTo.x, gate.RoomTo.y] == null);
		List<GateInfo> gates = new List<GateInfo>(Gates);
		gates.ForEach(x => generation.ConstructGateIfNotExists(x.RoomFrom, generation.Rooms[x.RoomTo.x, x.RoomTo.y]));
		Gates.RemoveAll(gate => generation.Rooms[gate.RoomTo.x, gate.RoomTo.y] == null);
	}

	public void RemoveRandomGates(RoomInfo[,] rooms, float procent) {
		Gates.RemoveAll(x => {
			bool delete = InitScane.rnd.NextDouble() < procent;
			if (delete && rooms[x.RoomTo.x, x.RoomTo.y] != null) {
				RoomInfo roomTo = rooms[x.RoomTo.x, x.RoomTo.y];
				GateInfo gateToRemoveToo = roomTo.Gates.Find(y => y.RoomTo == Position);
				roomTo.Gates.Remove(gateToRemoveToo);
			}

			return delete;
		});
	}

	public List<RoomInfo> GetConnectedRooms(RoomInfo[,] rooms) {
		List<RoomInfo> result = new List<RoomInfo>();
		foreach (GateInfo gate in Gates)
			if (gate?.RoomTo != null && rooms[gate.RoomTo.x, gate.RoomTo.y] != null)
				result.Add(rooms[gate.RoomTo.x, gate.RoomTo.y]);
		return result;
	}

	public void AddGate(Vector2Int LocalPosition, RoomInfo[,] rooms) {
		Gates.RemoveAll(x => x.LocalPosition == LocalPosition);
		Gates.Add(new GateInfo(LocalPosition, this));
		Gates.ForEach(gate => {
			Vector2Int vec = gate.GetScaledVector(Size) + Position;
			if (vec.x < rooms.GetLength(0) && vec.y < rooms.GetLength(1) && vec.x >= 0 && vec.y >= 0)
				gate.RoomTo = vec;
		});
	}

	public override bool Equals(object obj) {
		return obj != null && ((RoomInfo) obj).Position == Position;
	}

	public override int GetHashCode() {
		return Position.GetHashCode();
	}
}
