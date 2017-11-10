# colipars
COmmand LIne PARSer

Small library using ,NET Standard 2.0 which is capable of parsing command line arguments.

Has a default implementation to output to the console and get its configuration from attributes on a classes, but can be extended to output to something else or fetch its configuration from configuration files or other sources.

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