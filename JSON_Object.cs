using System;
using System.Collections.Generic;

/**
 * @Author WheatBerry
 */
class JSON_Object
{
	public string name;
	public string val;
	public List<JSON_Object> objArr;

	public static JSON_Object Build(string json)
	{
		json = deDeco(deSpace(json));
		JSON_Object self = new JSON_Object();
		self.objArr = new List<JSON_Object>();
		char[] cs = json.ToCharArray();
		int cursor = 0;
		for (int i = 0; i < cs.Length; i++)
		{
			if (cs[i] == '{')
			{
				int t = findEnd(CsSubString(json, cursor, json.Length), '{', '}');
				string[] parts = new string[2];
				string cut = CsSubString(json, cursor, cursor + t + 1);
				if (cut.StartsWith(","))
					cut = cut.Substring(1, cut.Length - 1);
				JSON_Object tempObj;
				if (cut.StartsWith("{"))
				{
					tempObj = Build(cut);
					self.objArr.Add(tempObj);
					i = cursor + t;
					while (i < json.Length && cs[i] != ',')
						i++;
					if (i > json.Length)
						break;
					cursor = i;
					continue;
				}
				parts[0] = CsSubString(cut, 0, cut.IndexOf(':'));
				Console.Write("cut: " + cut);
				parts[1] = CsSubString(cut, cut.IndexOf(':') + 1, cut.Length);
				tempObj = Build(parts[1]);
				tempObj.name = deDecoStr(deSpace(parts[0]));
				self.objArr.Add(tempObj);
				i = cursor + t;
				while (i < json.Length && cs[i] != ',')
					i++;
				if (i > json.Length)
					break;
				cursor = i;
			}
			else if (cs[i] == '[')
			{
				int t = findEnd(CsSubString(json, cursor, json.Length), '[', ']');
				string cut = CsSubString(json, cursor, cursor + t + 1);
				int cutI = cut.IndexOf(':');
				JSON_Object tempObj;
				if (cutI < 0)
				{
					tempObj = Build(cut);
				}
				else
				{
					string[] parts = new string[2];
					parts[0] = CsSubString(cut, 0, cutI);
					if (parts[0].StartsWith("{"))
						parts[0] = parts[0].Substring(1, parts[0].Length - 1);
					parts[1] = CsSubString(cut, cutI + 1, cut.Length);
					tempObj = Build(parts[1]);
					tempObj.name = deDecoStr(deSpace(parts[0]));
				}
				self.objArr.Add(tempObj);
				i = cursor + t;
				while (i < json.Length && cs[i] != ',')
					i++;
				i++;
				if (i > json.Length)
					break;
				cursor = i;
				if (cs[i] == '[')
					i--;
			}
			else if (cs[i] == ',' || i >= cs.Length - 1)
			{
				JSON_Object tempObj = new JSON_Object();
				string[] parts = CsSubString(json, cursor, i + 1).Split(':');
				if (parts.Length == 1)
				{
					tempObj.val = deDecoStr(deSpace(parts[0]));
				}
				else
				{
					tempObj.name = deDecoStr(deSpace(parts[0]));
					tempObj.val = deDecoStr(deSpace(parts[1]));
				}
				self.objArr.Add(tempObj);
				cursor = i + 1;
				if (cursor > json.Length)
					break;
			}
		}
		return self;
	}

	public string select(string selector)
	{
		JSON_Object obj = SelectObj(selector);
		if (obj == null)
			return null;
		if (obj.val == null)
			return "[Object]";
		return SelectObj(selector).val;
	}

	public JSON_Object SelectObj(string selector)
	{
		if (!selector.Contains(".") && !selector.Contains("["))
			return searchByName(selector);
		JSON_Object obj = this;
		char[] cs = selector.ToCharArray();
		int cursor = 0;
		for (int i = 0; i < cs.Length; i++)
		{
			if (cs[i] == '[')
			{
				if (cs[i - 1] != ']')
				{
					obj = obj.searchByName(CsSubString(selector, cursor, i));
					cursor = i;
				}
				int t = findEnd(CsSubString(selector, cursor, selector.Length), '[', ']');
				string cut = deDeco(CsSubString(selector, cursor + 1, cursor + t));
				int ib;
				try
				{
					ib = int.Parse(cut);
					obj = obj.objArr[ib];
				}
				catch (Exception e)
				{
					Console.Error.Write(e);
					obj = obj.objArr[int.Parse(select(cut))];
				}
				i = cursor + t;
				if (i + 1 < cs.Length && cs[i + 1] == '.')
					i++;
				cursor = i + 1;
			}
			else if (cs[i] == '.' || i >= selector.Length - 1)
			{
				string newSelec = CsSubString(selector, cursor, i + 1);
				if (newSelec.EndsWith("."))
					newSelec = CsSubString(newSelec, 0, newSelec.Length - 1);
				obj = obj.searchByName(newSelec);
				cursor = i + 1;
			}
		}
		return obj;
	}

	private JSON_Object searchByName(string name)
	{
		foreach (JSON_Object obj in objArr)
		{
			if (obj.name.Equals(name))
				return obj;
		}
		return null;
	}

	private static int findEnd(string str, char cStart, char cEnd)
	{
		char[] cs = str.ToCharArray();
		int stack = 0;
		int endI = -1;
		bool start = false;
		for (int i = 0; i < cs.Length; i++)
		{
			if (cs[i] == cStart)
			{
				start = true;
				stack++;
			}
			else if (cs[i] == cEnd)
				stack--;
			if (start && stack == 0)
			{
				endI = i;
				break;
			}
		}
		return endI;
	}

	private static string deSpace(string str)
	{
		while (str.StartsWith(" "))
			str = CsSubString(str, 1, str.Length);
		while (str.EndsWith(" "))
			str = CsSubString(str, 0, str.Length - 1);
		return str;
	}

	private static string deDeco(string str)
	{
		if (str.StartsWith("{") && str.EndsWith("}"))
			str = CsSubString(str, 1, str.Length - 1);
		else if (str.StartsWith("[") && str.EndsWith("]"))
			str = CsSubString(str, 1, str.Length - 1);
		return str;
	}

	private static string deDecoStr(string str)
	{
		if (str.EndsWith(","))
			str = CsSubString(str, 0, str.Length - 1);
		if (str.StartsWith("\"") && str.EndsWith("\""))
			str = CsSubString(str, 1, str.Length - 1);
		return str;
	}

	private static string CsSubString(string str, int begin, int end)
	{
		return str.Substring(begin, end - begin);
	}
}
