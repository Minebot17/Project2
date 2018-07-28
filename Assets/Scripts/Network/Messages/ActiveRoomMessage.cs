using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ActiveRoomMessage : MessageBase {
	public int PositionX;
	public int PositionY;
	public MessageManager.StringList NetworkIDs;
	public MessageManager.MultyStringList Data;

	public ActiveRoomMessage() { }

	public ActiveRoomMessage(Vector2Int position, List<string> networkIDs, List<List<string>> data) {
		PositionX = position.x;
		PositionY = position.y;
		NetworkIDs = new MessageManager.StringList();
		Data = new MessageManager.MultyStringList();
		NetworkIDs.AddRange(networkIDs);
		Data.AddRange(data);
	}
		
	public override void Deserialize(NetworkReader reader) {
		PositionX = reader.ReadInt32();
		PositionY = reader.ReadInt32();
		NetworkIDs = new MessageManager.StringList();
		Data = new MessageManager.MultyStringList();
		int count = reader.ReadInt32();
		for (int i = 0; i < count; i++)
			NetworkIDs.Add(reader.ReadString());

		int count0 = reader.ReadInt32();
		for (int i = 0; i < count0; i++) {
			int count1 = reader.ReadInt32();
			List<string> toAdd = new List<string>();
			for (int j = 0; j < count1; j++) {
				toAdd.Add(reader.ReadString());
			}
			Data.Add(toAdd);
		}
	}

	public override void Serialize(NetworkWriter writer) {
		writer.Write(PositionX);
		writer.Write(PositionY);
		writer.Write(NetworkIDs.Count);
		foreach (string value in NetworkIDs) {
			writer.Write(value);
		}
			
		writer.Write(Data.Count);
		foreach (List<string> list in Data) {
			writer.Write(list.Count);
			foreach (string s in list) {
				writer.Write(s);
			}
		}
	}
}
