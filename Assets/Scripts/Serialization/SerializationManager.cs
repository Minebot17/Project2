﻿using System.Collections;
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
		SaveWorld(currentSaveName == null ? ServerEvents.singleton.NewWorldName : currentSaveName);
	}

	public static void SaveWorld(string saveName) {
		currentSaveName = saveName;
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;
		if (File.Exists(filePath + postfix))
			File.Delete(filePath + postfix);

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
						if (objs.GetChild(j).GetComponent<ISerializableObject>() != null && objs.GetComponent<NetworkIdentity>() != null) {
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
			GameManager.singleton.seedToGeneration+""
			// TODO: Добавить сохранение индекса уровня
		});
		
		ZipFile.CreateFromDirectory(filePath, filePath + postfix);
		Directory.Delete(filePath, true);
	}
	
	public static LoadedWorld LoadWorld(string saveName) {
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;
		if (!File.Exists(filePath + postfix))
			return null;
		
		ZipFile.ExtractToDirectory(filePath + postfix, filePath);
		
		string[] playerFiles = Directory.GetFiles(filePath + "/Players");
		List<List<string>> players = playerFiles.Select(file => File.ReadAllLines(file).ToList()).ToList();

		List<LoadedWorld.LoadedObject>[,] loadedObjects = new List<LoadedWorld.LoadedObject>[GameManager.DefGenerationSize.x, GameManager.DefGenerationSize.y];
		string[] roomDirectories = Directory.GetDirectories(filePath + "Objects");
		foreach (string roomDirectory in roomDirectories) {
			string[] splitted = roomDirectory.Split('/')[roomDirectory.Split('/').Length - 1].Split('x');
			Vector2Int roomPosition = new Vector2Int(int.Parse(splitted[0]), int.Parse(splitted[1]));
			loadedObjects[roomPosition.x, roomPosition.y] = new List<LoadedWorld.LoadedObject>();
			string[] objectFiles = Directory.GetFiles(roomDirectory);
			foreach (string objectFile in objectFiles)
				loadedObjects[roomPosition.x, roomPosition.y].Add(
					new LoadedWorld.LoadedObject(
						objectFile.Split('/')[objectFile.Split('/').Length - 1].Split('_')[0],
						File.ReadAllLines(objectFile).ToList()
					)
				);
		}
		
		string[] worldInfo = File.ReadAllLines(filePath + "/worldInfo");
		int seedToGeneration = int.Parse(worldInfo[0]);
		int seedToSpawn = int.Parse(worldInfo[1]);
		
		return new LoadedWorld(players, loadedObjects, seedToGeneration, seedToSpawn, 0);
	}


	public static List<string> GetGameProfiles(string saveName) {
		string filePath = Application.persistentDataPath + "/Worlds/" + saveName;
		if (!File.Exists(filePath + postfix))
			return null;
		
		ZipFile.ExtractToDirectory(filePath + postfix, filePath);
		string[] files = Directory.GetFiles(filePath + "/Players");

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
