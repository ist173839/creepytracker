using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ConfigProperties
{
    public static void Clear(string filename)
    {
        File.Create(filename).Close();
    }

	private static char LineComment = '%';

	public static void WriteComment (string filename, string line)
	{
		using (StreamWriter file = new StreamWriter(filename))
		{
			file.WriteLine(LineComment + line);
		}
	}

	public static void Save(string filename, string property, string value)
	{
		if (File.Exists(filename))
		{
			List<string> lines = new List<string>(File.ReadAllLines(filename));
			int index = - 1;
			foreach (string line in lines)
			{
				if (line.Split('=')[0] == property && line[0] == LineComment)
				{
					index = lines.IndexOf(line);
				}
			}

			if (index > - 1)
			{
				lines[index] = property + "=" + value;
			}
			else
			{
				lines.Add(property + "=" + value);
			}

			File.WriteAllLines(filename, lines.ToArray());
		}
		else
		{
			using (StreamWriter file = new StreamWriter(filename))
			{
				file.WriteLine(property + "=" + value);
			}
		}
	}

	public static string Load(string filename, string property)
	{
		if (File.Exists(filename))
		{
			List<string> lines = new List<string>(File.ReadAllLines(filename));
			foreach (string line in lines)
			{
				if (line.Split('=')[0] == property)
				{
					return line.Split('=')[1];
				}
			}
            Debug.Log("No property '" + property + "' in file '" + filename + "'");
		}
		else
			Debug.Log("'" + filename + "' not found");

        return "";
	}

	public static string[] LoadKinects(string filename)
	{
		List<string> kinects = new List<string>();

		if (File.Exists(filename))
		{
			List<string> lines = new List<string>(File.ReadAllLines(filename));
			foreach (string line in lines)
			{
				if (line.Split('=')[0].Split('.')[0] == "kinect")
				{
					kinects.Add(line.Split('=')[1]);
				}
			}
		}
		return kinects.ToArray();
	}
}
