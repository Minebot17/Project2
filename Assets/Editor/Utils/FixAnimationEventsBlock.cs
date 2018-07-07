using System.Collections;
using System.Collections.Generic;
using NPOI.HSSF.Record;
using UnityEditor;
using UnityEngine;

public class FixAnimationEventsBlock : IUtilBlock {

	private string name;
	
	public void Draw() {
		name = EditorGUILayout.TextField("FBX name", name);

		if (GUILayout.Button("FIX THIS SHIT!!!")) {
			ModelImporter importer = (ModelImporter) AssetImporter.GetAtPath("Assets/Meshes/" + name + "_auto.fbx");
			ModelImporterClipAnimation[] animations = importer.clipAnimations;
			foreach (ModelImporterClipAnimation clip in animations) {
				int meele = 0;
				int range = 0;
				foreach (AnimationEvent @event in clip.events) {
					if (@event.functionName.Equals("EndAttack"))
						break;

					bool melee = @event.functionName.Equals("AttackMelee");
					@event.objectReferenceParameter =
						(ScriptableObject) AssetDatabase.LoadMainAssetAtPath(
							$"Assets/Resources/AttackInfoData/{name}/{clip.name}_{(melee ? meele++ : range++)}_{(melee ? "M" : "R")}.asset");
				}
			}

			importer.importCameras = false;
			importer.SaveAndReimport();
		}
	}

	public string GetName() {
		return "Fix Animation Events. Uh suka!";
	}
}
