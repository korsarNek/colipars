# colipars
COmmand LIne PARSer

Another library for parsing command line arguments using C#.
Depends on .Net Standard 2.0

Has a default implementation to output to the console and get its configuration from attributes on a classes, but can be extended to output to something else or fetch its configuration from configuration files or other sources.

## How to install

```sh
dotnet add package colipars
```

## Examples

### Named arguments
```
create --name test -d 0.42
```

```cs
[Verb("create")]
class CreateCommand
{
    [NamedOption("name", Description = "Name of the element to create")]
    public string Name { get; set; }

    [NamedOption("double", Alias = "d", Description = "A floating point value.")]
    public double Number { get; set; }
}
```

### Positional arguments

```
setPosition 0.40 -100
```

```cs
[Verb("setPosition")]
class SetPositionCommand
{
    [PositionalOption(0, Description = "X coordinate")]
    public float X { get; set; }

    [PositionalOption(1, Description = "Y coordinate")]
    public float Y { get; set; }
}
```

### Lists and flags with custom converters
Supports flags and lists with custom converters based on TypeConverters.

```
convert -vvv -names Alfred Freddy Jason
```

```cs
[Verb("convert")]
class ConvertCommand
{
    [CollectionTypeConverter(typeof(UserConverter))]
    [NamedCollectionOption("names", Alias = "n")]
    public ICollection<User> Users { get; } = new List<User>();

    [TypeConverter(typeof(VerbosityLevelConverter))]
    [FlagOption("verbosity", ShortHand = "v")]
    public VerbosityLevel Verbosity { get; set; }
}
```
Uses a special CollectionTypeConverter attribute to specify a converter for the elements of a collection instead of the whole collection.

## How to use

```cs
var exitCode = Cli.Setup.ClassAttributes<Command>().Parse(args).Map(
    (Command command) => command.Execute(),
    (IEnumerable<IError> errors) => 1
);
```

```cs
var command = Cli.Setup.ClassAttributes<Command>().Parse(args).GetCustomObject();
```

Help can be shown by using the -h flag or from code, this includes the description of the attributes.
```cs
Cli.Setup.ClassAttributes<Command>().ShowHelp();
```

### Configuration

If a default command is specified, the verb doesn't have to be specified at the beginning of the argument list.
```cs
Cli.Setup.ClassAttributes<ConvertCommand>((c) => c.UseAsDefault<ConvertCommand>());
```

Per default, the local use always CultureInfo.InvariantCulture, that can be changed in a similar way.
```cs
Cli.Setup.ClassAttributes<SetPositionCommand>((c) => c.CultureInfo = CultureInfo.CurrentCulture);
```

### Method verbs

It's possible to use the Verb attributes not only on classes but also on methods.
```cs
class MyContainer
{
    [Verb("handleNumbers")]
    public int HandleNumbers([NamedCollectionOption("numbers")] IEnumerable<int> numbers)
    {
        ...
        return 0;
    }
}
```

```cs
Cli.Setup.MethodAttributes<MyContainer>().Parse(args).Execute();
```