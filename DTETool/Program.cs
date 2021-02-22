using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
class Program
{
	static void Main(string[] inArgs)
	{
		var vs = new VSController();
		vs.Run(inArgs);
	}
}
public class VSController
{
	DTE myDTE { get => Marshal.GetActiveObject("VisualStudio.DTE.15.0") as DTE; }
	public void Run(string[] inArgs)
	{
		var command = Parse(inArgs, 0);
		var filepath = Parse(inArgs, 1);
		var line = int.Parse(Parse(inArgs, 2, "1"));
		var col = int.Parse(Parse(inArgs, 3, "1"));
		switch(command)
		{
			case "OpenFile": OpenFile(filepath, line, col); break;
			case "BreakPoint": BreakPoint(filepath, line, col); break;
			case "Attach": Attach(filepath); break;
			default: break;
		}
	}
	string Parse(string[] inArgs, int inIndex, string inDefault = null)
	{
		if(inArgs == null || inIndex < 0 || inIndex >= inArgs.Length)
		{
			return inDefault;
		}
		return inArgs[inIndex];

	}
	void Attach(string inName)
	{
		var dte = myDTE;
		if(dte.Debugger.DebuggedProcesses.Count != 0)
		{
			dte.Debugger.DetachAll();
			return;
		}
		if(inName == null)
		{
			return;
		}
		foreach(var item in dte.Debugger.LocalProcesses)
		{
			var proc = (Process)item;
			if(proc.Name.Contains(inName))
			{
				proc.Attach();
				return;
			}
		}
	}
	void OpenFile(string inPath, int inLine, int inCol)
	{
		if(inPath == null)
		{
			return;
		}
		var dte = myDTE;
		dte.ItemOperations.OpenFile(inPath);
		var selection = dte.ActiveDocument.Selection as TextSelection;
		selection.MoveToLineAndOffset(inLine, inCol);
	}
	void BreakPoint(string inPath, int inLine, int inCol)
	{
		if(inPath == null)
		{
			return;
		}
		var dte = myDTE;
		var breakpoints = dte.Debugger.Breakpoints;
		for(int i = 0; i < breakpoints.Count; i++)
		{
			var point = breakpoints.Item(i + 1);
			if(Path.GetFullPath(point.File) == Path.GetFullPath(inPath) && point.FileLine == inLine)
			{
				point.Delete();
				return;
			}
		}
		breakpoints.Add("", inPath, inLine, inCol);
	}
}
