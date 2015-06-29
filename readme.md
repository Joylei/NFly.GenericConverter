#NFly.GenericConverter

##Intro:
convert type from one to another

##custom converter
user registered converter has the highest priority.

you can register a custom converter by:

		Converter.Register(typeof(YourType), yourConverter)


##how to use:
		var time = DateTime.Now;
		var text = Converter.Convert<string>(time);
