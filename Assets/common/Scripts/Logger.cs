using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum LogLevel
{
	INFO,
	DEBUG,
	WARNING
}

class LogMessage
{
	private LogLevel _logLevel;
	private string _message;

	public LogMessage (LogLevel logLevel, string message)
	{
		LogLevel = logLevel;
		Message = message;
	}

	public LogLevel LogLevel
    {
		get
        {
			return _logLevel;
		}

		set
        {
			_logLevel = value;
		}
	}

	public string Message
    {
		get
        {
			return _message;
		}

		set
        {
			_message = value;
		}
	}
}

// ReSharper disable once UnusedMember.Global
public class Logger : MonoBehaviour
{
	public int Max;
	private List<LogMessage> _messages;

	void Start ()
	{
		_messages = new List<LogMessage> ();
	}

    // ReSharper disable once UnusedMember.Local
	void Update ()
	{
		//Debug.Log("" + this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
	}

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
	void OnGUI ()
	{
		/*
        int top = 10;
        foreach (LogMessage m in _messages)
        {
            GUI.Label(new Rect(10, top, 600, 30), "" + _messages.IndexOf(m) + ": " + m.Message);
            top += 15;
        }

        if (GUI.Button(new Rect(10, Screen.height - 20, 10, 10), ""))
        {
            saveLog(LogLevel.DEBUG, "lol" + max);
        }*/
	}

	public void SaveLog (LogLevel logLevel, string message)
	{
		LogMessage n = new LogMessage (logLevel, message);

        if (_messages.Count >= Max)
        {
			_messages.RemoveAt (0);
		}
		_messages.Add (n);
	}
}
