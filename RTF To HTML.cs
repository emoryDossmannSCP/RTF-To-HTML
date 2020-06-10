void Main()
{
	string rtfFile = @"rtfFile.rtf";
	string rtfRefFile = @"rtfRef.json";
	
	var rtfText = File.ReadAllText(rtfFile);
	string[] rtfTextLines = rtfText.Split(new[] { "\r\n"}, StringSplitOptions.None);
	List<ColorTable> colorTbl = new List<ColorTable>();
	List<FontTable> fontTbl = new List<FontTable>();
	List<Table> tables = new List<Table>();
	DocumentStructure doc = new DocumentStructure(); 
	Html html = new Html();
	Body body = new Body();
	double twipsToPixConvFactor = 0.067;
	
	string theText = "";
	//string html = @"<!DOCTYPE html><html><body>";
	bool ignore = false;
	foreach( var i in rtfTextLines)
	{
		//i.Dump();
		
		if (i.Contains(@"\colortbl") == true)
		{
			//i.Dump();
			
			string [] colorSets = i.Split(';');
			int colorPosCnt = 1;
			foreach( var c in colorSets.Skip(2))
			{
				//c.Dump();
				string[] colorSetItems = c.Split('\\');
				foreach( var color in colorSetItems)
				{
					//color.Dump();
					//var theColor = color.Split().Select(x => String.Join("", x.TakeWhile(b => char.IsLetter(b)))).ToList();
					//string s2 = Regex.Replace(color, @"[^A-Z]+", String.Empty);
					
					foreach(Match matches in Regex.Matches (color, @"(?'color'[a-z]+)(?'value'[0-9]+)", RegexOptions.None))
					{
						string theColor = matches.Groups["color"].ToString();
						int colorValue = Convert.ToInt32(matches.Groups["value"].ToString());
						colorTbl.Add(new ColorTable() {Position = colorPosCnt, Color = theColor, ColorValue = colorValue });
					}
					
				}
				colorPosCnt++;
			}
		}
		if (i.Contains(@"\fonttbl") == true)
		{
			
			//i.Dump();
			int fontPosCnt = 1;
			var jsonFile = File.ReadAllText(rtfRefFile);
			//dynamic rtfRefJson = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonFile);
			Newtonsoft.Json.Linq.JObject r = Newtonsoft.Json.Linq.JObject.Parse(jsonFile);
			
			var exclusions = from x in r["RtfReferences"]["exclusions"]
						   	 where (string)x["name"] == "fonttbl"
						     select (string)x["value"];
			
			
			foreach (Match matches in Regex.Matches(i, @"\w+[a-z]", RegexOptions.None))
			{
				if (exclusions.Contains(matches.Value) == false )
				{
					fontTbl.Add(new FontTable() { Position = fontPosCnt, Font = matches.Value });
				}
				fontPosCnt++;
			}

				
		}

		if (i.Contains(@"\trowd") == true)
		{
			
			//List<Table> theTable = new List<Table>();
			List<Style> tblStyle = new List<Style>();
			
			foreach (Match matches in Regex.Matches(i, @"\\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)"))
			{
				if (!String.IsNullOrEmpty(matches.Groups[1].Value) &&  matches.Groups[1].Value == "trpaddl")
				{
					tblStyle.Add(new Style() {item = "padding-left", value = Math.Round(Convert.ToDouble(matches.Groups[2].Value) * twipsToPixConvFactor, 0).ToString(), units = @"px" });
				}

				if (!String.IsNullOrEmpty(matches.Groups[1].Value) && matches.Groups[1].Value == "trpaddr")
				{
					tblStyle.Add(new Style() { item = "padding-right", value = Math.Round(Convert.ToDouble(matches.Groups[2].Value) * twipsToPixConvFactor, 0).ToString(), units = @"px" });
				}

			}
			//tblStyle.Dump();
			tables.Add(new Table() {styles = tblStyle});
			//table.position = 4;
			
			
		}
		
		else
		{
			var jsonFile = File.ReadAllText(rtfRefFile);
			//dynamic rtfRefJson = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonFile);
			Newtonsoft.Json.Linq.JObject r = Newtonsoft.Json.Linq.JObject.Parse(jsonFile);
			var exclusions = from x in r["RtfReferences"]["exclusions"]
							 where (string)x["type"] == "control"
							 select (string)x["value"];
			
			
			foreach( Match matches in Regex.Matches(i, @"\\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)") )
			{
				
				if (!String.IsNullOrEmpty(matches.Groups[5].Value) )
				{
					if ( matches.Groups[5].Value == "{")
					{
						ignore = true;
					}
					
					else if( matches.Groups[5].Value == "}")
					{
						ignore = false;
					}
				}
				
				if( !String.IsNullOrEmpty(matches.Groups[6].Value) && exclusions.Contains(matches.Groups[1].Value) == false && ignore == false)
				{
					//matches.Groups[6].Value.Dump();
					theText = $"{theText}{matches.Groups[6].Value}";
				}
			}
			
		}
	}
	//colorTbl.Dump();
	//fontTbl.Dump();
	theText.Dump();
	body.tables = tables;
	doc.body = body;
	doc.Dump();

}

// Define other methods and classes here
public class ColorTable
{
	public int Position {get; set;}
	public string Color {get; set;}
	public int ColorValue {get; set;}
}

public class FontTable
{
	public int Position { get; set; }
	public string Font { get; set; }
}
//Create a JSON file of Exclusions and reference that. 
public class Exclusions
{
	public string Type {get; set;}
	public string Name {get; set;}
	public string Value {get; set;}
	
}

public class DocumentStructure
{
	public DocType docType;
	public Html html;
	public Head head;
	public Body body;
	public Div divs;
	public Table tables;
	public Paragraph paragraphs;

}

public class DocType
{
	public string id = "DOCTYPE";
	public string tag  = "!DOCTYPE";
	public string value = "html";
	public int position = 1;
}

public class Html
{
	public string id = "html";
	public string tag = "html";
	public int position = 2;
	public List<Head> heads;
	public List<Body> body;
	//public List<Div> divs;
	
}

public class Head
{
	public string id = "head";
	public string tag = "head";
	public int position {get; set;}
	public Title title;
}

public class Title
{
	public string id = "title";
	public string tag = "title";
	public int position {get; set;}
	public string value {get; set;}
}
public class Body
{
	public string id = "body";
	public string tag = "body";
	public List<Div> divs;
	public List<Table> tables;
	public List<Paragraph> paragraphs;
	
}

public class Div
{
	public string id = "div";
	public string tag = "div";
	public List<Style> styles;
	public int position;
	
}

public class Table
{
	public string id = "table";
	public string tag = "table";
	public int position {get; set;}
	public List<Style> styles;
}

public class Paragraph
{
	
}

public class Style
{
	public string id = "style";
	public string tag = "style";
	public string item {get; set;}
	public string value {get; set;}
	public string units {get; set;}
}
