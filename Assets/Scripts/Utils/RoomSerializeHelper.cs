using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RoomSerializeHelper {

	[Serializable]
	public struct SaveFile {
		public IntArray[] matrix;
		public List<RoomObject> objects;
		public int location;
	}

	[Serializable]
	public class IntArray {
		public int[] array;
	}

	[Serializable]
	public class RoomObject {
		public string prefabName;
		public Veci coords;
		public Vecb mirror;
		public int ID;
		public int type;
		public string[] data;
	}

	[Serializable]
	public struct Veci {
		public int x;
		public int y;

		public Veci(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public bool isZero() { return x.Equals(0) && y.Equals(0); }
		public override string ToString() { return "X: " + x.ToString() + " Y: " + y.ToString(); }
		public override int GetHashCode() { return x.GetHashCode() + y.GetHashCode(); }
		public override bool Equals(object obj) { return x.Equals(((Veci)obj).x) && y.Equals(((Veci)obj).y); }
	}

	[Serializable]
	public struct Vecb {
		public bool x;
		public bool y;

		public Vecb(bool x, bool y) {
			this.x = x;
			this.y = y;
		}

		public bool isZero() { return x.Equals(false) && y.Equals(false); }
		public override string ToString() { return "X: " + x.ToString() + " Y: " + y.ToString(); }
		public override int GetHashCode() { return x.GetHashCode() + y.GetHashCode(); }
		public override bool Equals(object obj) { return x.Equals(((Vecb)obj).x) && y.Equals(((Vecb)obj).y); }
	}
}

