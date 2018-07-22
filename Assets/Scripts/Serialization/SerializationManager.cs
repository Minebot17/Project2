using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

public class SerializationManager {
	private const string postfix = ".rgw";
	private List<List<string>>[][] lastLoadedSave = new List<List<string>>[100][];
	public string currentSaveName;
	
	public static void SaveWorld(string oldSaveName, string saveName) {
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;
		if (File.Exists(filePath + postfix))
			File.Delete(filePath + postfix);

		Directory.CreateDirectory(filePath);
		Directory.CreateDirectory(filePath + "/Players");
		Directory.CreateDirectory(filePath + "/Objects");

		foreach (GameObject player in GameManager.Instance.Players)
			File.WriteAllLines(filePath + "/Players/" + player.GetComponent<GameProfile>().ProfileName, SerializePlayer(player));

		for(int x = 0; x < GenerationManager.currentGeneration.size.x; x++)
			for(int y = 0; y < GenerationManager.currentGeneration.size.y; y++) {
				GameObject room = GenerationManager.spawnedRooms[x, y];
				if (room != null && GenerationManager.currentGeneration.rooms[x, y].Position == new Vector2Int(x, y) && room.GetComponent<Room>().IsObjectsInitialized) {
					string roomPath = filePath + "/Objects/" + x + "x" + y;
					Directory.CreateDirectory(roomPath);
					
					Transform objs = room.transform.Find("Objests");
					for (int j = 0; j < objs.childCount; j++)
						if (objs.GetChild(j).GetComponent<ISerializableObject>() != null) {
							int index = 0;
							if (File.Exists(roomPath + "/" + objs.GetComponent<NetworkIdentity>().assetId + "_" + index))
								index++;
							
							File.WriteAllLines(roomPath + "/" + objs.GetComponent<NetworkIdentity>().assetId + "_" + index, 
								objs.GetChild(j).GetComponent<ISerializableObject>().Serialize());
						}
				}
			}
		
		File.WriteAllLines(filePath + "/worldInfo", new [] {
			GenerationManager.currentGeneration.Seed+"", 
			GameManager.Instance.seedToGeneration+""
			// TODO: Добавить сохранение индекса уровня
		});
		
		ZipFile.CreateFromDirectory(filePath, filePath + postfix);
		Directory.Delete(filePath, true);
	}
	
	public static void LoadWorld(string saveName) {
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;
		if (!File.Exists(filePath + postfix))
			return;
		
		ZipFile.ExtractToDirectory(filePath + postfix, filePath);
		string[] worldInfo = File.ReadAllLines(filePath + "/worldInfo");
		int seedToGeneration = int.Parse(worldInfo[0]);
		int seedToSpawn = int.Parse(worldInfo[1]);
		
		GenerationInfo generation = GameManager.Instance.GetGeneration(seedToGeneration);
		GenerationManager.SpawnGeneration(RoomLoader.loadedRooms, generation, seedToSpawn, true);
		GameObject player = MonoBehaviour.Instantiate(GameManager.Instance.LocalPlayer);
		// TODO
	}


	public static List<string> GetGameProfiles(string saveName) {
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;
		if (!File.Exists(filePath + postfix))
			return null;
		
		ZipFile.ExtractToDirectory(filePath + postfix, filePath);
		List<string> result = new List<string>();
		string[] files = Directory.GetFiles(filePath + "/Players");
		foreach (string file in files) {
			result.Add(File.ReadAllLines(file)[0]);
		}

		return result;
	}

	public static List<string> SerializePlayer(GameObject player) {
		return new List<string>{ player.GetComponent<GameProfile>().Serialize() };
	}

	public static void DeserializePlayer(GameObject player, List<string> data) {
		player.GetComponent<GameProfile>().Deserialize(data[0]);
	}
}
