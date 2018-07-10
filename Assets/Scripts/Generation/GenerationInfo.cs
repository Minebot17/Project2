using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class GenerationInfo {
	public readonly int index;
	public readonly Vector2Int size;
	public readonly RoomInfo[,] rooms;
	public readonly List<RoomInfo> reservedRooms = new List<RoomInfo>();
	public readonly int location;
	public RoomInfo startRoom;
	public RoomInfo endRoom;
	
	// Stats
	public int ReservedCount;
	public int RoomsCount;
	public int CellsCount;
	public int GatesCount;
	public Dictionary<Vector2Int, Int32> BigRoomsCount = new Dictionary<Vector2Int, int>();
	public int Seed;

	public GenerationInfo(int index, Vector2Int size, int location, int seed) {
		this.index = index;
		this.size = size;
		this.location = location;
		Seed = seed;
		
		rooms = new RoomInfo[size.x, size.y];
		for(int x = 0; x < size.x; x++)
			for(int y = 0; y < size.y; y++)
				rooms[x, y] = new RoomInfo(new Vector2Int(x, y));
	}

	public void WriteStats() {
		ReservedCount = reservedRooms.Count + 2;
		
		List<Vector2Int> poses = new List<Vector2Int>();
		for(int x = 0; x < size.x; x++)
			for(int y = 0; y < size.y; y++)
				if (rooms[x, y] != null && !poses.Contains(new Vector2Int(x, y))) {
					poses.Add(new Vector2Int(x, y));
					RoomsCount++;
				}

		foreach (Vector2Int pos in poses) {
			RoomInfo room = rooms[pos.x, pos.y];
			GatesCount += room.Gates.Count;
			if (BigRoomsCount.ContainsKey(room.Size))
				BigRoomsCount[room.Size] += 1;
			else {
				BigRoomsCount.Add(room.Size, 1);
			}
		}

		for(int x = 0; x < size.x; x++)
			for(int y = 0; y < size.y; y++)
				if (rooms[x, y] != null)
					CellsCount++;
	}
	
	public void ReserveRooms(List<Func<GenerationInfo, List<RoomInfo>>> reservers) {
		foreach (Func<GenerationInfo, List<RoomInfo>> reserver in reservers) 
			reservedRooms.AddRange(reserver.Invoke(this));
	}

	public void ClearReservation() {
		foreach (RoomInfo reservedRoom in reservedRooms) {
			reservedRoom.IsReserved = false;
			reservedRoom.ReservedRoomName = null;
		}
		reservedRooms.Clear();
	}

	public void GenerateBigRooms(Random random, float bigRoomProcent, Dictionary<Vector2Int, float> bigRoomTypesProcents) {
		for(int x = 0; x < size.x; x++)
			for (int y = 0; y < size.y; y++) {
				if (Rooms[x, y] != null && Rooms[x, y].Size == Vector2Int.one && random.NextDouble() < bigRoomProcent) {
					Dictionary<Vector2Int, float> currentTypesProcent = new Dictionary<Vector2Int, float>(bigRoomTypesProcents);

					//Удаляем невозможные размеры комнат
					foreach (Vector2Int key in currentTypesProcent.Keys.ToArray()) {
						bool isBreak = false;
						for (int xR = 0; xR < key.x; xR++) {
							for (int yR = 0; yR < key.y; yR++) {
								try {
									RoomInfo info = rooms[x + xR, y + yR];
									if (info == null || info.Size != Vector2Int.one || info.IsReserved) {
										currentTypesProcent.Remove(key);
										isBreak = true;
										break;
									}
								}
								catch (IndexOutOfRangeException e) {
									currentTypesProcent.Remove(key);
									isBreak = true;
									break;
								}
							}
							if (isBreak)
								break;
						}
					}
					if (currentTypesProcent.Count == 0)
						continue;
					
					// Рандомим тип
					List<float> values = currentTypesProcent.Values.ToList();
					float[] prosentLine = new float[currentTypesProcent.Count + 1];
					float sumBuffer = 0;
					for (int i = 0; i < prosentLine.Length - 1; i++) {
						prosentLine[i] = sumBuffer;
						sumBuffer += values[i];
					}
					prosentLine[prosentLine.Length - 1] = sumBuffer;
					Vector2Int roomSize = currentTypesProcent.Keys.ToList()[GetNumberIndexInProcentLine(prosentLine, (float) random.NextDouble() * prosentLine[prosentLine.Length - 1])];
					SetRoomSize(new Vector2Int(x, y), roomSize);
				}
			}
	}

	public void ReserveStartAndEnd(Func<RoomInfo[,], RoomInfo[]> startEndSelector) {
		RoomInfo[] result = startEndSelector.Invoke(rooms);
		startRoom = result[0];
		endRoom = result[1];
		startRoom.Reserve("start");
		endRoom.Reserve("end");
	}

	public void GenerateAndRemoveGates(Random random, float removeProcent) {
		for(int x = 0; x < size.x; x++)
			for (int y = 0; y < size.y; y++) {
				if (rooms[x, y] != null && rooms[x, y].Position == new Vector2Int(x, y)) {
					rooms[x, y].GenerateGates(rooms);
					rooms[x, y].RemoveRandomGates(random, Rooms, removeProcent);
				}
			}
	}

	public void RemoveSeparatedRooms() {
		List<RoomInfo> connectedRooms = new List<RoomInfo>();
		GetConnectedRoomsRecursive(startRoom, ref connectedRooms);
		GetConnectedRoomsRecursive(endRoom, ref connectedRooms);
		for(int x = 0; x < size.x; x++)
			for(int y = 0; y < size.y; y++)
				if (!connectedRooms.Contains(rooms[x, y]))
					rooms[x, y] = null;
	}

	public void CheckReservedRooms() {
		for (int i = 0; i < reservedRooms.Count; i++) {
			
			RoomLoader.Room roomObject = RoomLoader.loadedRooms.Find(x => x.fileName.Equals(reservedRooms[i].ReservedRoomName));
			if (roomObject == null) {
				Debug.LogWarning("Reserved room names: " + reservedRooms[i].ReservedRoomName + " not found");
				continue;
			}
			
			List<RoomObject> gateObjects = roomObject.objects.FindAll(x => x.prefabName.Contains("Gate"));
			if (gateObjects.Count % 2 == 1) {
				Debug.LogWarning("Gate count in room names: " + reservedRooms[i].ReservedRoomName + " is not even");
				continue;
			}
			
			reservedRooms[i].GenerateGates(rooms);
			Dictionary<GateInfo, int> gateTypes = new Dictionary<GateInfo, int>();
			foreach (RoomObject gateObject in gateObjects) {
				Vector3 coords = gateObject.coords;
				int side =
					coords.y == 0 ? 0 :
					(coords.y + 23) % 277 == 0 ? 1 :
					coords.x == 0 ? 2 : 3;
				int index = side == 0 || side == 1 ? ((int)coords.x - 214) / 495 : ((int)coords.y - 105) / 277;
				Vector2Int gateObjectLocation = new Vector2Int(side, index);
				GateInfo currentGate = reservedRooms[i].Gates.Find(x => x.LocalPosition == gateObjectLocation);
				gateTypes.Add(currentGate, int.Parse(gateObject.data[0]));
			}

			List<GateInfo> gates = gateTypes.Keys.ToList();
			List<GateInfo> toRemoveWithStartEnd = new List<GateInfo>();
			List<GateInfo> toRemoveWithoutStartEnd = new List<GateInfo>();
			foreach (GateInfo gate in gates) {
				if (rooms[gate.RoomTo.x, gate.RoomTo.y] == null && gateTypes[gate] == 1) {
					Vector2Int vec = gate.GetScaledVector(reservedRooms[i].Size);
					Vector2Int newVec = reservedRooms[i].Position + vec;
					Rooms[newVec.x, newVec.y] = new RoomInfo(newVec);
				}
				else if (rooms[gate.RoomTo.x, gate.RoomTo.y] != null && gateTypes[gate] == 2) {
					bool toStartOrEnd = IsGateToStartOrEnd(gate);
					bool reserved = IsGateToReserved(gate);
					if (toStartOrEnd)
						toRemoveWithStartEnd.Add(gate);
					else if (!reserved)
						toRemoveWithoutStartEnd.Add(gate);
					else {
						reservedRooms[i].IsReserved = false;
						reservedRooms[i].ReservedRoomName = null;
						reservedRooms.RemoveAt(i);
						continue;
					}
				}
			}

			if (toRemoveWithStartEnd.Count == 1) {
				reservedRooms[i].IsReserved = false;
				reservedRooms[i].ReservedRoomName = null;
				reservedRooms.RemoveAt(i);
				continue;
			}
			if (toRemoveWithStartEnd.Count != 0)
				toRemoveWithStartEnd.RemoveAt(0);

			reservedRooms[i].Gates.RemoveAll(x => {
				bool removeWithoutStartEnd = toRemoveWithoutStartEnd.Contains(x);
				if (removeWithoutStartEnd)
					RemoveFromGateRecursive(x);
				return removeWithoutStartEnd || toRemoveWithStartEnd.Contains(x);
			});
		}
	}

	public void ConstructPathToEndIfNotExists() {
		rooms[0, 0] = null;
		List<RoomInfo> connectedRooms = new List<RoomInfo>();
		GetConnectedRoomsRecursive(startRoom, ref connectedRooms);
		if (!connectedRooms.Contains(endRoom)) {
			List<RoomInfo> triedRooms = new List<RoomInfo>();
			while (true) {
				List<RoomInfo> selectedRooms = connectedRooms.Where(x => (x.Equals(startRoom) || !x.IsReserved) && x.Position != Vector2Int.zero && !triedRooms.Contains(x))
					.OrderBy(x => Vector2Int.Distance(x.Position, endRoom.Position)).ToList();
				if (selectedRooms.Count == 0)
					throw new Exception("Bad generation");
				RoomInfo nearestRoom = selectedRooms[0];

				const float accuracy = 10f;
				Vector2 oneVector = ((Vector2) (endRoom.Position - nearestRoom.Position)).normalized / accuracy;
				float distance = Vector2Int.Distance(nearestRoom.Position, endRoom.Position);
				List<Vector2Int> path = new List<Vector2Int>(){ nearestRoom.Position };
				for (Vector2 iVec = oneVector; iVec.magnitude < distance; iVec += oneVector) {
					Vector2Int newPos = new Vector2Int(Mathf.RoundToInt(iVec.x), Mathf.RoundToInt(iVec.y)) + nearestRoom.Position;
					if (newPos.x != path[path.Count - 1].x && newPos.y != path[path.Count - 1].y) {
						Vector2Int newestVector = new Vector2Int(newPos.x, path[path.Count - 1].y);
						if (rooms[newestVector.x, newestVector.y] == null)
							path.Add(newestVector);
						else
							path.Add(new Vector2Int(path[path.Count - 1].x, newPos.y));
					}

					if (!path.Contains(newPos))
						path.Add(newPos);
				}

				List<RoomInfo> pathRooms = path.ConvertAll(x => rooms[x.x, x.y] ?? (rooms[x.x, x.y] = new RoomInfo(x)));
				if (pathRooms.Exists(x => x.IsReserved && !x.Equals(startRoom) && !x.Equals(endRoom))) {
					triedRooms.Add(nearestRoom);
					continue;
				}
				
				for (int i = 0; i < pathRooms.Count - 1; i++)
					ConstructGateIfNotExists(pathRooms[i], pathRooms[i + 1]);
				
				return;
			}
		}
	}

	public void CheckAllGates() {
		for(int x = 0; x < size.x; x++)
			for(int y = 0; y < size.y; y++)
				if (rooms[x, y] != null)
					rooms[x, y].ReconectAllGatesRemoveEmpty(this);
	}

	public void ConstructGateIfNotExists(RoomInfo roomFrom, RoomInfo roomTo) {
		if (roomFrom.Gates.Exists(x => rooms[x.RoomTo.x, x.RoomTo.y] != null && rooms[x.RoomTo.x, x.RoomTo.y].Equals(roomTo)) &&
			    roomTo.Gates.Exists(x => rooms[x.RoomTo.x, x.RoomTo.y] != null && rooms[x.RoomTo.x, x.RoomTo.y].Equals(roomFrom)))
			return;

		Vector2Int localPosFrom = new Vector2Int();
		Vector2Int localPosTo = new Vector2Int();

		RoomInfo roomFromCopy = new RoomInfo(roomFrom.Position) { Size = roomFrom.Size };
		roomFromCopy.GenerateGates(rooms);
		
		RoomInfo roomToCopy = new RoomInfo(roomTo.Position) { Size = roomTo.Size };
		roomToCopy.GenerateGates(rooms);

		foreach (GateInfo gate in roomFromCopy.Gates)
			if (gate.RoomTo == roomTo.Position)
				localPosFrom = gate.LocalPosition;
		
		foreach (GateInfo gate in roomToCopy.Gates)
			if (gate.RoomTo == roomFrom.Position)
				localPosTo = gate.LocalPosition;
		
		roomFrom.AddGate(localPosFrom, rooms);
		roomTo.AddGate(localPosTo, rooms);
	}

	public bool IsGateToStartOrEnd(GateInfo gate) {
		List<RoomInfo> connectedRooms = new List<RoomInfo> { gate.RoomFrom };
		GetConnectedRoomsRecursive(rooms[gate.RoomTo.x, gate.RoomTo.y], ref connectedRooms);
		return connectedRooms.Contains(startRoom) || connectedRooms.Contains(endRoom);
	}

	public bool IsGateToReserved(GateInfo gate) {
		List<RoomInfo> connectedRooms = new List<RoomInfo> { gate.RoomFrom };
		GetConnectedRoomsRecursive(rooms[gate.RoomTo.x, gate.RoomTo.y], ref connectedRooms);
		foreach (RoomInfo reservedRoom in reservedRooms)
			if (connectedRooms.Contains(reservedRoom))
				return true;
		return false;
	}

	// Не используй, если данные проход ведет к выходу или входу!!! Чека это методом IsGateToStartOrEnd
	// Так же избегай удаление зарезервированных комнат, ибо их уже после второй резервации не вернуть!
	public void RemoveFromGateRecursive(GateInfo gate) {
		List<RoomInfo> connectedRooms = new List<RoomInfo> { gate.RoomFrom };
		GetConnectedRoomsRecursive(rooms[gate.RoomTo.x, gate.RoomTo.y], ref connectedRooms);
		connectedRooms.Remove(gate.RoomFrom);
		for(int x = 0; x < size.x; x++)
			for(int y = 0; y < size.y; y++)
				if (connectedRooms.Contains(rooms[x, y]))
					rooms[x, y] = null;
	}

	private void GetConnectedRoomsRecursive(RoomInfo startRoom, ref List<RoomInfo> rooms) {
		if (rooms.Contains(startRoom))
			return;
		
		rooms.Add(startRoom);
		List<RoomInfo> connectedRooms = startRoom.GetConnectedRooms(this.rooms);
		foreach (RoomInfo room in connectedRooms)
			GetConnectedRoomsRecursive(room, ref rooms);
	}

	private void SetRoomSize(Vector2Int position, Vector2Int roomSize) {
		RoomInfo room = rooms[position.x, position.y];
		room.Size = roomSize;
		for(int x = 0; x < roomSize.x; x++)
			for (int y = 0; y < roomSize.y; y++)
				rooms[position.x + x, position.y + y] = room;
	}

	private int GetNumberIndexInProcentLine(float[] procentLine, float number) {
		for (int i = 0; i < procentLine.Length - 1; i++)
			if (procentLine[i] < number && procentLine[i + 1] > number)
				return i;
		return -1;
	}

	public int Index => index;
	public RoomInfo[,] Rooms => rooms;
	public List<RoomInfo> ReservedRooms => reservedRooms;
	public int Location => location;
}
