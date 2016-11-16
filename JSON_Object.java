package Gizmo;

import java.util.ArrayList;
import java.util.List;

/**
 * @描述 Magic code
 * @创建人 易志鹏
 * @创建时间 2016年11月15日 下午3:18:31
 * @version
 */
public class JSON_Object {
	public String name;
	public String val;
	public List<JSON_Object> objArr;

	public static JSON_Object Build(String json) {
		json = deDeco(deSpace(json));
		JSON_Object self = new JSON_Object();
		self.objArr = new ArrayList<>();
		char[] cs = json.toCharArray();
		int cursor = 0;
		for (int i = 0; i < cs.length; i++) {
			if (cs[i] == '{') {
				int t = findEnd(json.substring(cursor, json.length()), '{', '}');
				String[] parts = new String[2];
				String cut = json.substring(cursor, cursor + t + 1);
				if (cut.startsWith("{")) {
					JSON_Object tObj = Build(cut);
					self.objArr.add(tObj);
					i = cursor + t;
					while (i < json.length() && cs[i] != ',')
						i++;
					i++;
					if (i > json.length())
						break;
					cursor = i;
					continue;
				}
				parts[0] = cut.substring(0, cut.indexOf(':'));
				parts[1] = cut.substring(cut.indexOf(':') + 1, cut.length());
				JSON_Object tObj = Build(parts[1]);
				tObj.name = deDecoStr(deSpace(parts[0]));
				self.objArr.add(tObj);
				i = cursor + t;
				while (i < json.length() && cs[i] != ',')
					i++;
				i++;
				if (i > json.length())
					break;
				cursor = i;
			} else if (cs[i] == '[') {
				int t = findEnd(json.substring(cursor, json.length()), '[', ']');
				String cut = json.substring(cursor, cursor + t + 1);
				int cutI = cut.indexOf(':');
				JSON_Object tObj;
				if (cutI < 0) {
					tObj = Build(cut);
				} else {
					String[] parts = new String[2];
					parts[0] = cut.substring(0, cutI);
					parts[1] = cut.substring(cutI + 1, cut.length());
					tObj = Build(parts[1]);
					tObj.name = deDecoStr(deSpace(parts[0]));
				}
				self.objArr.add(tObj);
				i = cursor + t;
				while (i < json.length() && cs[i] != ',')
					i++;
				i++;
				if (i > json.length())
					break;
				cursor = i;
			} else if (cs[i] == ',' || i >= cs.length - 1) {
				JSON_Object tObj = new JSON_Object();
				String[] parts = json.substring(cursor, i + 1).split(":");
				if (parts.length == 1) {
					tObj.val = deDecoStr(deSpace(parts[0]));
				} else {
					tObj.name = deDecoStr(deSpace(parts[0]));
					tObj.val = deDecoStr(deSpace(parts[1]));
				}
				self.objArr.add(tObj);
				cursor = i + 1;
				if (cursor > json.length())
					break;
			}
		}
		return self;
	}

	public String select(String selector) {
		JSON_Object obj = selectObj(selector);
		if (obj.val == null)
			return "[Object]";
		return selectObj(selector).val;
	}

	private JSON_Object selectObj(String selector) {
		if (!selector.contains(".") && !selector.contains("["))
			return searchByName(selector);
		JSON_Object obj = this;
		char[] cs = selector.toCharArray();
		int cursor = 0;
		for (int i = 0; i < cs.length; i++) {
			if (cs[i] == '[') {
				if (cs[i - 1] != ']') {
					obj = obj.searchByName(selector.substring(cursor, i));
					cursor = i;
				}
				int t = findEnd(selector.substring(cursor, selector.length()), '[', ']');
				String cut = deDeco(selector.substring(cursor + 1, cursor + t));
				int ib;
				try {
					ib = Integer.valueOf(cut);
					obj = obj.objArr.get(ib);
				} catch (Exception e) {
					obj = obj.objArr.get(Integer.valueOf(select(cut)));
				}
				i = cursor + t;
				if (i + 1 < cs.length && cs[i + 1] == '.')
					i++;
				cursor = i + 1;
			} else if (cs[i] == '.' || i >= selector.length() - 1) {
				String newSelec = selector.substring(cursor, i + 1);
				if (newSelec.endsWith("."))
					newSelec = newSelec.substring(0, newSelec.length() - 1);
				obj = obj.searchByName(newSelec);
				cursor = i + 1;
			}
		}
		return obj;
	}

	private JSON_Object searchByName(String name) {
		for (JSON_Object obj : objArr) {
			if (obj.name.equals(name))
				return obj;
		}
		return null;
	}

	private static int findEnd(String str, char cStart, char cEnd) {
		char[] cs = str.toCharArray();
		int stack = 0;
		int endI = -1;
		boolean start = false;
		for (int i = 0; i < cs.length; i++) {
			if (cs[i] == cStart) {
				start = true;
				stack++;
			} else if (cs[i] == cEnd)
				stack--;
			if (start && stack == 0) {
				endI = i;
				break;
			}
		}
		return endI;
	}

	private static String deSpace(String str) {
		while (str.startsWith(" "))
			str = str.substring(1, str.length());
		while (str.endsWith(" "))
			str = str.substring(0, str.length() - 1);
		return str;
	}

	private static String deDeco(String str) {
		if (str.startsWith("{") && str.endsWith("}"))
			str = str.substring(1, str.length() - 1);
		else if (str.startsWith("[") && str.endsWith("]"))
			str = str.substring(1, str.length() - 1);
		return str;
	}

	private static String deDecoStr(String str) {
		if (str.endsWith(","))
			str = str.substring(0, str.length() - 1);
		if (str.startsWith("\"") && str.endsWith("\""))
			str = str.substring(1, str.length() - 1);
		return str;
	}

	public static void main(String[] args) {
		String json = "{\"a\": {\"aa\":\"hahaha\", \"bb\":{\"aabbc\": [{\"ss\": \"string\"}, \"ha!\", 3]}}, \"b\": 1, \"c\":[1,2,\"abc\", [[1,2,3],6,7,8]]}";
		JSON_Object object = JSON_Object.Build(json);
		System.out.println(object.select("a.bb.aabbc[0]"));
		System.out.println(object.select("c[3][0][1]"));
		System.out.println(object.select("a.bb.aabbc[c[3][0][1]]"));
		System.out.println(object.select("a.bb.aabbc[0].ss"));
	}
}
