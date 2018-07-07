using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;
using YandexDiskSharp;

public class PlayerImporter : AssetPostprocessor {
	private const string AccesToken = "AQAAAAANmHY-AAUDAnyfXZItw0DRnYfkgco71qw";
	private const string DiskPath = "P2/ОФИС/АВТОИМПОРТ/{0}.xlsx";
	private const string LocalPath = "PlayerStats.xlsx";
	private const string AssetInfoPath = "Assets/Resources/AttackInfoData";

	public void OnPostprocessModel(GameObject gameObject) {
		if (!gameObject.name.Contains("_auto"))
			return;
		
		ModelImporter importer = (ModelImporter) assetImporter;
		//Кадр;Урон;ОтталкиваниеX;ОтталкиваниеY;СмещениеX;СмещениеY;РазмерX;РазмерY

		if (!importer.importCameras) {
			importer.importCameras = true;
			return;
		}

		if (File.Exists(LocalPath))
			File.Delete(LocalPath);

		string meshName = gameObject.name.Substring(0, gameObject.name.Length - 5);
		RestClient disk = new RestClient(AccesToken);
		WebClient web = new WebClient { Encoding = Encoding.ASCII };
		File.WriteAllBytes(LocalPath, web.DownloadData(disk.GetResourceDownloadLink(string.Format(DiskPath, meshName)).Href));
		using (FileStream file = new FileStream(LocalPath, FileMode.Open)) {
			XSSFWorkbook book = new XSSFWorkbook(file);
			ISheet sheet = book.GetSheetAt(0);
			int animationCount = (int) sheet.GetRow(2).GetCell(7).NumericCellValue;
			int animationIndex = (int) sheet.GetRow(4).GetCell(7).NumericCellValue;
			ModelImporterClipAnimation[] animations = new ModelImporterClipAnimation[animationCount];
			ClipAnimationInfoCurve[] defaultCurve = importer.defaultClipAnimations[animationIndex].curves;
			for (int i = 0; i < animationCount; i++) {
				ModelImporterClipAnimation clip = importer.defaultClipAnimations[animationIndex];
				string[] timings = sheet.GetRow(i + 1).GetCell(3).StringCellValue.Split(new []{'-'}, StringSplitOptions.RemoveEmptyEntries);
				clip.curves = defaultCurve;
				clip.name = sheet.GetRow(i + 1).GetCell(2).StringCellValue;
				clip.firstFrame = float.Parse(timings[0].Replace('.', ','));
				clip.lastFrame = float.Parse(timings[1].Replace('.', ','));
				clip.loopTime = true;

				string[] attacksString = sheet.GetRow(i + 1).GetCell(5) == null ? null : sheet.GetRow(i + 1).GetCell(5).StringCellValue.Split('/');
				string[] rangeString = sheet.GetRow(i + 1).GetCell(6) == null ? null : sheet.GetRow(i + 1).GetCell(6).StringCellValue.Split('/');
				if ((attacksString != null || rangeString != null) && ((rangeString != null && (!rangeString[0].Equals("") && !rangeString[0].Equals(" "))) || (attacksString != null && (!attacksString[0].Equals("") && !attacksString[0].Equals(" "))))) {
					clip.loopTime = false;
					if (!AssetDatabase.IsValidFolder(AssetInfoPath + "/" + meshName))
						AssetDatabase.CreateFolder(AssetInfoPath, meshName);
					List<AnimationEvent> events = new List<AnimationEvent>();
					
					if (attacksString != null && !attacksString[0].Equals("") && !attacksString[0].Equals(" ")) {
						for (int j = 0; j < attacksString.Length; j++) {
							int[] attacksData = Array.ConvertAll(attacksString[j].Split(';'), int.Parse);
							AnimationEvent @event = new AnimationEvent {
								time = attacksData[0] / (clip.lastFrame - clip.firstFrame + 1f),
								functionName = "AttackMelee"
							};
							MeleeAttackInfo info = (MeleeAttackInfo) ScriptableObject.CreateInstance(typeof(MeleeAttackInfo));
							info.Index = j;
							info.Damage = attacksData[1];
							info.Kick = new Vector2(attacksData[2], attacksData[3]);
							info.Point = new Vector2(attacksData[4], attacksData[5]);
							info.Size = new Vector2(attacksData[6], attacksData[7]);
							info.Splash = attacksData[8] == 1;
							AssetDatabase.CreateAsset(info, $"Assets/Resources/AttackInfoData/{meshName}/{clip.name}_{info.Index}_M.asset");
							AssetDatabase.SaveAssets();
							@event.objectReferenceParameter =
								(ScriptableObject) AssetDatabase.LoadMainAssetAtPath(
									$"Assets/Resources/AttackInfoData/{meshName}/{clip.name}_{info.Index}_M.asset");
							events.Add(@event);
						}

						events.Add(new AnimationEvent {
							time = 0.95f,
							functionName = "EndAttack"
						});
					}

					if (rangeString != null && !rangeString[0].Equals("") && !rangeString[0].Equals(" ")) {
						for (int j = 0; j < rangeString.Length; j++) {
							string[] rangeData = rangeString[j].Split(';');
							AnimationEvent @event = new AnimationEvent {
								time = int.Parse(rangeData[0]) / (clip.lastFrame - clip.firstFrame + 1f),
								functionName = "AttackProjectile"
							};
							ProjectileAttackInfo info = (ProjectileAttackInfo) ScriptableObject.CreateInstance(typeof(ProjectileAttackInfo));
							info.Index = j;
							info.Name = rangeData[1];
							info.TrustForce = int.Parse(rangeData[2]);
							AssetDatabase.CreateAsset(info, $"Assets/Resources/AttackInfoData/{meshName}/{clip.name}_{info.Index}_R.asset");
							AssetDatabase.SaveAssets();
							@event.objectReferenceParameter =
								(ScriptableObject) AssetDatabase.LoadMainAssetAtPath(
									$"Assets/Resources/AttackInfoData/{meshName}/{clip.name}_{info.Index}_R.asset");
							events.Add(@event);
						}
					}

					clip.events = events.ToArray();
				}

				animations[i] = clip;
			}

			importer.clipAnimations = animations;
		}
	}
}
