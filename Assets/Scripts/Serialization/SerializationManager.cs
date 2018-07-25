using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class SerializationManager {
	private const string postfix = ".rgw";
	private static string currentSaveName;

	public static void SaveWorld() {
		SaveWorld(currentSaveName ?? ServerEvents.singleton.NewWorldName);
	}

	public static void SaveWorld(string saveName) {
		currentSaveName = saveName;
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;

		Directory.CreateDirectory(filePath);
		Directory.CreateDirectory(filePath + "/Players");
		Directory.CreateDirectory(filePath + "/Objects");

		foreach (GameObject player in GameManager.singleton.Players)
			File.WriteAllLines(filePath + "/Players/" + player.GetComponent<GameProfile>().ProfileName, SerializePlayer(player));

		for(int x = 0; x < GenerationManager.currentGeneration.size.x; x++)
			for(int y = 0; y < GenerationManager.currentGeneration.size.y; y++) {
				GameObject room = GenerationManager.spawnedRooms[x, y];
				if (room != null && GenerationManager.currentGeneration.rooms[x, y].Position == new Vector2Int(x, y) && room.GetComponent<Room>().Initialized) {
					string roomPath = filePath + "/Objects/" + x + "x" + y;
					Directory.CreateDirectory(roomPath);
					
					Transform objs = room.transform.Find("Objects");
					for (int j = 0; j < objs.childCount; j++)
						if (objs.GetChild(j).GetComponent<ISerializableObject>() != null && objs.GetChild(j).GetComponent<NetworkIdentity>() != null) {
							int index = 0;
							while (File.Exists(roomPath + "/" + objs.GetChild(j).GetComponent<NetworkIdentity>().assetId + "_" + index))
								index++;
							
							File.WriteAllLines(roomPath + "/" + objs.GetChild(j).GetComponent<NetworkIdentity>().assetId + "_" + index, 
								objs.GetChild(j).GetComponent<ISerializableObject>().Serialize());
						}
				}
				else if (ServerEvents.singleton.LastLoadedWorld != null && ServerEvents.singleton.LastLoadedWorld.Objects[x, y] != null) {
					string roomPath = filePath + "/Objects/" + x + "x" + y;
					Directory.CreateDirectory(roomPath);

					List<LoadedWorld.LoadedObject> objects = ServerEvents.singleton.LastLoadedWorld.Objects[x, y];
					foreach (LoadedWorld.LoadedObject loadedObject in objects) {
						int index = 0;
						if (File.Exists(roomPath + "/" + loadedObject.AssetID + "_" + index))
							index++;
							
						File.WriteAllLines(roomPath + "/" + loadedObject.AssetID + "_" + index, loadedObject.Data);
					}
				}
			}
		

		/*if (currentSaveName != null) {
			ZipFile.ExtractToDirectory(Application.persistentDataPath + "/Worlds/" + currentSaveName + postfix, filePath + "_Copy");
			string[] newRooms = Directory.GetDirectories(filePath + "/Objects");
			string[] oldRooms = Directory.GetDirectories(filePath + "_Copy" + "/Objects");
			string[] needCopy = oldRooms.Where(x => !newRooms.Select(y => y.Split('\\')[1]).Contains(x.Split('\\')[1])).ToArray();
			foreach (string roomToCopy in needCopy)
				Utils.DirectoryCopy(filePath + "_Copy/Objects/" + roomToCopy, filePath + "/Objects/" + roomToCopy, true);
			Directory.Delete(filePath + "_Copy", true);
		}*/

		File.WriteAllLines(filePath + "/worldInfo", new [] {
			GenerationManager.currentGeneration.Seed+"", 
			GameManager.singleton.seedToSpawn+""
			// TODO: Добавить сохранение индекса уровня
		});
		
		if (File.Exists(filePath + postfix))
			File.Delete(filePath + postfix);
		ZipFile.CreateFromDirectory(filePath, filePath + postfix);
		Directory.Delete(filePath, true);
	}

	public static LoadedWorld LoadWorld() {
		return LoadWorld(currentSaveName);
	}

	public static LoadedWorld LoadWorld(string saveName) {
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;
		if (!File.Exists(filePath + postfix))
			return null;
		
		ZipFile.ExtractToDirectory(filePath + postfix, filePath);
		
		string[] playerFiles = Directory.GetFiles(filePath + "/Players");
		List<List<string>> players = playerFiles.Select(file => File.ReadAllLines(file).ToList()).ToList();

		List<LoadedWorld.LoadedObject>[,] loadedObjects = new List<LoadedWorld.LoadedObject>[GameManager.DefGenerationSize.x, GameManager.DefGenerationSize.y];
		string[] roomDirectories = Directory.GetDirectories(filePath + "/Objects");
		foreach (string roomDirectory in roomDirectories) {
			string[] splitted = roomDirectory.Split('\\')[1].Split('x');
			Vector2Int roomPosition = new Vector2Int(int.Parse(splitted[0]), int.Parse(splitted[1]));
			loadedObjects[roomPosition.x, roomPosition.y] = new List<LoadedWorld.LoadedObject>();
			string[] objectFiles = Directory.GetFiles(roomDirectory);
			foreach (string objectFile in objectFiles)
				loadedObjects[roomPosition.x, roomPosition.y].Add(
					new LoadedWorld.LoadedObject(
						objectFile.Split('\\')[2].Split('_')[0],
						File.ReadAllLines(objectFile).ToList()
					)
				);
		}
		
		string[] worldInfo = File.ReadAllLines(filePath + "/worldInfo");
		int seedToGeneration = int.Parse(worldInfo[0]);
		int seedToSpawn = int.Parse(worldInfo[1]);
		
		Directory.Delete(filePath, true);
		return new LoadedWorld(players, loadedObjects, seedToGeneration, seedToSpawn, 0);
	}


	public static List<string> GetGameProfiles(string saveName) {
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;
		if (!File.Exists(filePath + postfix))
			return null;
		
		ZipFile.ExtractToDirectory(filePath + postfix, filePath);
		string[] files = Directory.GetFiles(filePath + "/Players");
		Directory.Delete(filePath, true);

		return files.Select(file => File.ReadAllLines(file)[0]).ToList();
	}

	public static List<string> SerializePlayer(GameObject player) {
		return new List<string> {
			player.GetComponent<GameProfile>().Serialize(),
			player.transform.position.x+"",
			player.transform.position.y+""
		};
	}

	public static void DeserializePlayer(GameObject player, List<string> data) {
		player.GetComponent<GameProfile>().Deserialize(data[0]);
		player.transform.position = new Vector3(float.Parse(data[1]), float.Parse(data[2]), player.transform.position.z);
	}

	public class LoadedWorld {
		public List<List<string>> Players;
		public List<LoadedObject>[,] Objects;
		public LoadedWorldInfo Info;

		public LoadedWorld(List<List<string>> players, List<LoadedObject>[,] objects, int seedToGenerate, int seedToSpawn, int level) {
			Players = players;
			Objects = objects;
			Info = new LoadedWorldInfo(seedToGenerate, seedToSpawn, level);
		}

		public class LoadedWorldInfo {
			public int SeedToGenerate;
			public int SeedToSpawn;
			public int Level;


			public LoadedWorldInfo(int seedToGenerate, int seedToSpawn, int level) {
				SeedToGenerate = seedToGenerate;
				SeedToSpawn = seedToSpawn;
				Level = level;
			}
		}

		public class LoadedObject {
			public string AssetID;
			public List<string> Data;
			public LoadedObject(string assetId, List<string> data) {
				AssetID = assetId;
				Data = data;
			}
		}
	}
}
